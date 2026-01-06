namespace UnityLicenseCollector
{
    public sealed class NuGetLicenseDataAdapter : ILicenseData
    {
        private readonly NuGetLicenseData _data;

        public NuGetLicenseDataAdapter(NuGetLicenseData data)
        {
            _data = data;
        }

        public string PackageName => _data.PackageName;
        public string Version => _data.Version;
        public string LicenseUrl => _data.LicenseUrl ?? _data.ProjectUrl;
        public string LicenseIdentifier => _data.LicenseExpression ?? _data.LicenseType;
        public string LicenseContent => _data.LicenseContent;
        public string ProjectUrl => _data.ProjectUrl;
        public string Authors => _data.Authors;
    }
}
