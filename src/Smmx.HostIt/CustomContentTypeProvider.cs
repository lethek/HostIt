using Microsoft.AspNetCore.StaticFiles;


namespace Smmx.HostIt
{
    public class CustomContentTypeProvider : FileExtensionContentTypeProvider
    {

        public CustomContentTypeProvider()
        {
            Mappings.Add(".cs", "text/plain");
        }

    }
}
