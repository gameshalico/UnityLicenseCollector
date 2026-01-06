using System.Collections.Generic;
using System.Xml;

namespace UnityLicenseCollector.Editor
{
    public sealed class PackagesConfigParser
    {
        public List<NuGetPackageInfo> ParsePackagesConfig(string packagesConfigPath)
        {
            var packages = new List<NuGetPackageInfo>();

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(packagesConfigPath);

            var packageNodes = xmlDoc.SelectNodes("//package");
            if (packageNodes == null)
                return packages;

            foreach (XmlNode node in packageNodes)
            {
                var id = node.Attributes?["ID"]?.Value ?? node.Attributes?["id"]?.Value;
                var version = node.Attributes?["Version"]?.Value ?? node.Attributes?["version"]?.Value;

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(version))
                {
                    packages.Add(new NuGetPackageInfo
                    {
                        PackageId = id,
                        Version = version
                    });
                }
            }

            return packages;
        }
    }

    public sealed class NuGetPackageInfo
    {
        public string PackageId;
        public string Version;
    }
}
