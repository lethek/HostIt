using Microsoft.Extensions.FileProviders.Physical;


namespace Smmx.HostIt
{

    public class Settings
    {
        public string Root { get; set; }
        public bool EnableDefaultFiles { get; set; }
        public ExclusionFilters ExclusionFilters { get; set; }
    }

}
