using System;
using System.IO;

namespace UnityLicenseCollector.Editor
{
    public sealed class NuGetPackageDirectoryParser
    {
        public (string packageId, string version) ParseDirectoryName(string directoryName)
        {
            var parts = directoryName.Split('.');
            if (parts.Length < 4)
            {
                throw new ArgumentException($"Invalid NuGet package directory name: {directoryName}");
            }

            var versionStartIndex = FindVersionStartIndex(parts);
            if (versionStartIndex < 0)
            {
                throw new ArgumentException($"Cannot determine version start index in directory name: {directoryName}");
            }

            var packageId = string.Join(".", parts, 0, versionStartIndex);
            var version = string.Join(".", parts, versionStartIndex, parts.Length - versionStartIndex);

            return (packageId, version);
        }

        public bool TryParseDirectoryName(string directoryName, out string packageId, out string version)
        {
            packageId = null;
            version = null;

            var parts = directoryName.Split('.');
            if (parts.Length < 4)
            {
                return false;
            }

            var versionStartIndex = FindVersionStartIndex(parts);
            if (versionStartIndex < 0)
            {
                return false;
            }

            packageId = string.Join(".", parts, 0, versionStartIndex);
            version = string.Join(".", parts, versionStartIndex, parts.Length - versionStartIndex);

            return true;
        }

        private int FindVersionStartIndex(string[] parts)
        {
            var versionStartIndex = -1;
            for (var i = parts.Length - 3; i >= 0; i--)
            {
                if (parts[i].Length > 0 && char.IsDigit(parts[i][0]))
                {
                    versionStartIndex = i;
                }
                else
                {
                    break;
                }
            }

            return versionStartIndex;
        }
    }
}
