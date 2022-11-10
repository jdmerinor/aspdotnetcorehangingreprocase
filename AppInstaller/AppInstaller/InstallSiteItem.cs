namespace AppInstaller
{
    public class InstallSiteItem
    {
        public InstallSiteItem(string folderName, string defaultDomainPrefix, string defaultApplicationPath, string description)
        {
            FolderName = folderName;
            DefaultDomainPrefix = defaultDomainPrefix;
            DefaultApplicationPath = defaultApplicationPath;
            Description = description;
        }

        public string FolderName { get; private set; }
        public string DefaultDomainPrefix { get; private set; }
        public string DefaultApplicationPath { get; private set; }
        public string Description { get; private set; }
        public string WebAddress
        {
            get
            {
                return DefaultDomainPrefix + "." + "dotnettest.com" + "/" + DefaultApplicationPath;
            }
        }

        public string Site
        {
            get
            {
                var slash = WebAddress.IndexOf('/');
                return slash > -1 ? WebAddress.Substring(0, slash) : WebAddress;
            }
        }

        public string ApplicationPath
        {
            get
            {
                var slash = WebAddress.IndexOf('/');
                return slash > -1 ? WebAddress.Substring(slash) : "/";
            }
        }
    }
}