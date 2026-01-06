namespace UnityLicenseCollector
{
    public sealed class ManualLicenseDataAdapter : ILicenseData
    {
        private readonly ManualLicenseData _data;

        public ManualLicenseDataAdapter(ManualLicenseData data)
        {
            _data = data;
        }

        public string PackageName => _data.PackageName;
        public string Version => _data.Version;
        public string LicenseUrl => _data.LicenseUrl;
        public string LicenseIdentifier => _data.LicenseIdentifier;
        public string LicenseContent => _data.LicenseContent;
        public string ProjectUrl => _data.ProjectUrl;
        public string Authors => _data.Authors;
    }
}
