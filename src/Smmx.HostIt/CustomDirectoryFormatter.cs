using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using NaturalSort.Extension;


namespace Smmx.HostIt
{

    public class CustomDirectoryFormatter : IDirectoryFormatter
    {

        public CustomDirectoryFormatter(HtmlEncoder encoder)
        {
            _htmlEncoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
        }


        public virtual Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (contents == null) throw new ArgumentNullException(nameof(contents));

            context.Response.ContentType = "text/html; charset=utf-8";

            if (HttpMethods.IsHead(context.Request.Method)) return Task.CompletedTask;

            var pathString = context.Request.PathBase + context.Request.Path;
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat(
                "<!DOCTYPE html>\r\n<html lang=\"{0}\">", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            );
            stringBuilder.AppendFormat(
                "\r\n<head>\r\n  <title>{0} {1}</title>",
                HtmlEncode(Resources.HtmlDir_IndexOf),
                HtmlEncode(pathString.Value)
            );
            stringBuilder.Append(
                "\r\n  <style>\r\n    body {\r\n        font-family: \"Segoe UI\", \"Segoe WP\", \"Helvetica Neue\", 'RobotoRegular', sans-serif;\r\n        font-size: 14px;}\r\n    header h1 {\r\n        font-family: \"Segoe UI Light\", \"Helvetica Neue\", 'RobotoLight', \"Segoe UI\", \"Segoe WP\", sans-serif;\r\n        font-size: 28px;\r\n        font-weight: 100;\r\n        margin-top: 5px;\r\n        margin-bottom: 0px;}\r\n    #index {\r\n        border-collapse: separate;\r\n        border-spacing: 0;\r\n        margin: 0 0 20px; }\r\n    #index th {\r\n        vertical-align: bottom;\r\n        padding: 10px 5px 5px 5px;\r\n        font-weight: 400;\r\n        color: #a0a0a0;\r\n        text-align: center; }\r\n    #index td { padding: 3px 10px; }\r\n    #index th, #index td {\r\n        border-right: 1px #ddd solid;\r\n        border-bottom: 1px #ddd solid;\r\n        border-left: 1px transparent solid;\r\n        border-top: 1px transparent solid;\r\n        box-sizing: border-box; }\r\n    #index th:last-child, #index td:last-child {\r\n        border-right: 1px transparent solid; }\r\n    #index td.length, td.modified { text-align:right; }\r\n    a { color:#1ba1e2;text-decoration:none; }\r\n    a:hover { color:#13709e;text-decoration:underline; }\r\n  </style>\r\n</head>\r\n<body>\r\n  <section id=\"main\">"
            );
            stringBuilder.AppendFormat(
                "\r\n    <header><h1>{0} <a href=\"/\">/</a>",
                HtmlEncode(Resources.HtmlDir_IndexOf)
            );

            string body1 = "/";
            string str = pathString.Value;
            char[] separator = new char[1] { '/' };
            foreach (string body2 in str.Split(separator, StringSplitOptions.RemoveEmptyEntries)) {
                body1 = body1 + body2 + "/";
                stringBuilder.AppendFormat("<a href=\"{0}\">{1}/</a>", HtmlEncode(body1), HtmlEncode(body2));
            }

            stringBuilder.AppendFormat(
                CultureInfo.CurrentUICulture,
                "</h1></header>\r\n    <table id=\"index\" summary=\"{0}\">\r\n    <thead>\r\n      <tr><th abbr=\"{1}\">{1}</th><th abbr=\"{2}\">{2}</th><th abbr=\"{3}\">{4}</th></tr>\r\n    </thead>\r\n    <tbody>",
                HtmlEncode(Resources.HtmlDir_TableSummary),
                HtmlEncode(Resources.HtmlDir_Name),
                HtmlEncode(Resources.HtmlDir_Size),
                HtmlEncode(Resources.HtmlDir_Modified),
                HtmlEncode(Resources.HtmlDir_LastModified)
            );
            foreach (var fileInfo in contents.Where(info => info.IsDirectory).OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase.WithNaturalSort())) {
                try {
                    stringBuilder.AppendFormat(
                        "\r\n      <tr class=\"directory\">\r\n        <td class=\"name\"><a href=\"./{0}/\">{0}/</a></td>\r\n        <td></td>\r\n        <td class=\"modified\">{1}</td>\r\n      </tr>",
                        HtmlEncode(fileInfo.Name),
                        HtmlEncode(fileInfo.LastModified.ToString(CultureInfo.CurrentCulture))
                    );
                } catch (DirectoryNotFoundException) { } catch (FileNotFoundException) { }
            }

            foreach (var fileInfo in contents.Where(info => !info.IsDirectory).OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase.WithNaturalSort())) {
                try {
                    stringBuilder.AppendFormat(
                        "\r\n      <tr class=\"file\">\r\n        <td class=\"name\"><a href=\"./{0}\">{0}</a></td>\r\n        <td class=\"length\">{1}</td>\r\n        <td class=\"modified\">{2}</td>\r\n      </tr>",
                        HtmlEncode(fileInfo.Name),
                        HtmlEncode(fileInfo.Length.ToString("n0", CultureInfo.CurrentCulture)),
                        HtmlEncode(fileInfo.LastModified.ToString(CultureInfo.CurrentCulture))
                    );
                } catch (DirectoryNotFoundException) { } catch (FileNotFoundException) { }
            }

            stringBuilder.Append("\r\n    </tbody>\r\n    </table>\r\n  </section>\r\n</body>\r\n</html>");

            byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            context.Response.ContentLength = bytes.Length;
            return context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }


        private string HtmlEncode(string body)
            => _htmlEncoder.Encode(body);


        private readonly HtmlEncoder _htmlEncoder;
    }

}
