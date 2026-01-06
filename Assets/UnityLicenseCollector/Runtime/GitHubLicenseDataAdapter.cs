namespace UnityLicenseCollector
{
    public sealed class GitHubLicenseDataAdapter : ILicenseData
    {
        private readonly GitHubLicenseData _data;

        public GitHubLicenseDataAdapter(GitHubLicenseData data)
        {
            _data = data;
        }

        public string PackageName => _data.PackageName;
        public string Version => _data.Version;
        public string LicenseUrl => _data.LicenseHtmlUrl;
        public string LicenseIdentifier => _data.LicenseSpdxId;
        public string LicenseContent => _data.LicenseContent;
        public string ProjectUrl => _data.ProjectUrl;
        public string Authors => _data.Authors;
    }
}
