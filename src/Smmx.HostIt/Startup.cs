using System;
using System.IO;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;


namespace Smmx.HostIt
{

    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
            Settings = config.Get<Settings>();
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseFileServer(
                new FileServerOptions {
                    FileProvider = new PhysicalFileProvider(
                        root: String.IsNullOrEmpty(Settings.Root) ? Directory.GetCurrentDirectory() : Settings.Root,
                        filters: Settings.ExclusionFilters
                    ),
                    EnableDirectoryBrowsing = true,
                    DirectoryBrowserOptions = {
                        Formatter = new CustomDirectoryFormatter(HtmlEncoder.Default)
                    },
                    StaticFileOptions = {
                        ServeUnknownFileTypes = true,
                        DefaultContentType = "application/octet-stream",
                        ContentTypeProvider = new CustomContentTypeProvider()
                    },
                    EnableDefaultFiles = Settings.EnableDefaultFiles
                }
            );
        }


        public Settings Settings { get; }
        public IConfiguration Configuration { get; }

    }

}
