using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityLicenseCollector.Editor
{
    public sealed class NuGetLocalReader
    {
        private readonly string _installedPackagesPath;
        private readonly NuGetLicenseHelper _licenseHelper;

        public NuGetLocalReader(string installedPackagesPath)
        {
            _installedPackagesPath = installedPackagesPath;
            _licenseHelper = new NuGetLicenseHelper();
        }

        public async Task<NuGetLicenseData> ReadLicenseAsync(string packageId, string version, CancellationToken cancellationToken = default)
        {
            var packageDirectory = Path.Combine(_installedPackagesPath, $"{packageId}.{version}");

            if (!Directory.Exists(packageDirectory))
            {
                throw new DirectoryNotFoundException($"Package directory not found: {packageDirectory}");
            }

            var nuspecPath = Path.Combine(packageDirectory, $"{packageId}.nuspec");

            if (!File.Exists(nuspecPath))
            {
                throw new FileNotFoundException($"Nuspec file not found: {nuspecPath}");
            }

            var nuspecXml = File.ReadAllText(nuspecPath);
            var licenseData = NuGetLicenseHelper.ParseNuspecXml(nuspecXml, packageId, version);

            var licenseFile = Directory.GetFiles(packageDirectory)
                .FirstOrDefault(f =>
                {
                    var fileName = Path.GetFileName(f);
                    return fileName.StartsWith("LICENSE", System.StringComparison.OrdinalIgnoreCase) ||
                           fileName.StartsWith("License", System.StringComparison.OrdinalIgnoreCase) ||
                           fileName.StartsWith("license", System.StringComparison.OrdinalIgnoreCase);
                });

            if (licenseFile != null)
            {
                licenseData.LicenseContent = File.ReadAllText(licenseFile);
            }
            else
            {
                await _licenseHelper.TryFetchFromGitHubAsync(licenseData, cancellationToken);
            }

            return licenseData;
        }
    }
}
