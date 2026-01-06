namespace UnityLicenseCollector
{
    public interface ILicenseData
    {
        string PackageName { get; }
        string Authors { get; }
        string Version { get; }
        string ProjectUrl { get; }
        string LicenseUrl { get; }
        string LicenseIdentifier { get; }
        string LicenseContent { get; }
    }
}
