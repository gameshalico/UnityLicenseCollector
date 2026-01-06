using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UnityLicenseCollector.Editor
{
    public sealed class NuGetLicenseHelper
    {
        private readonly GitHubApiClient _gitHubApiClient;

        public NuGetLicenseHelper()
        {
            _gitHubApiClient = new GitHubApiClient();
        }

        public async Task<NuGetLicenseData> ParseNuspecXmlAsync(string xml, string packageId, string version, CancellationToken cancellationToken = default)
        {
            var licenseData = ParseNuspecXml(xml, packageId, version);

            if (string.IsNullOrEmpty(licenseData.LicenseContent) && !string.IsNullOrEmpty(licenseData.ProjectUrl))
            {
                await TryFetchFromGitHubAsync(licenseData, cancellationToken);
            }

            return licenseData;
        }

        public async Task TryFetchFromGitHubAsync(NuGetLicenseData licenseData, CancellationToken cancellationToken = default)
        {
            var (owner, repo) = TryParseGitHubUrl(licenseData.ProjectUrl);
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                return;
            }

            try
            {
                var gitHubLicense = await _gitHubApiClient.FetchLicenseAsync(owner, repo, null, cancellationToken);
                licenseData.LicenseContent = gitHubLicense.LicenseContent;
                if (string.IsNullOrEmpty(licenseData.LicenseExpression) && !string.IsNullOrEmpty(gitHubLicense.LicenseSpdxId))
                {
                    licenseData.LicenseExpression = gitHubLicense.LicenseSpdxId;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to fetch GitHub license for {licenseData.PackageName} from {licenseData.ProjectUrl}: {ex.Message}");
            }
        }

        public static NuGetLicenseData ParseNuspecXml(string xml, string packageId, string version)
        {
            var doc = XDocument.Parse(xml);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            var metadata = doc.Root?.Element(ns + "metadata");

            var licenseUrl = GetElementValue(metadata, ns + "licenseUrl");
            var licenseElement = metadata?.Element(ns + "license");
            var licenseExpression = licenseElement?.Attribute("type")?.Value == "expression" ? licenseElement.Value?.Trim() : null;
            var licenseFile = licenseElement?.Attribute("type")?.Value == "file" ? licenseElement.Value?.Trim() : null;
            var projectUrl = GetElementValue(metadata, ns + "projectUrl");
            var authors = GetElementValue(metadata, ns + "authors");
            var description = GetElementValue(metadata, ns + "description");
            var copyright = GetElementValue(metadata, ns + "copyright");
            var tags = GetElementValue(metadata, ns + "tags");

            var licenseType = string.IsNullOrEmpty(licenseExpression) ? "file" : "expression";
            var license = string.IsNullOrEmpty(licenseExpression) ? licenseFile : licenseExpression;

            return new NuGetLicenseData
            {
                PackageName = packageId,
                Version = version,
                LicenseUrl = licenseUrl,
                LicenseExpression = licenseExpression,
                LicenseType = licenseType,
                LicenseVersion = license,
                LicenseContent = null,
                ProjectUrl = projectUrl,
                Authors = authors,
                Description = description,
                Copyright = copyright,
                Tags = tags
            };
        }

        private static string GetElementValue(XElement parent, XName elementName)
        {
            return parent?.Element(elementName)?.Value?.Trim();
        }

        public static (string owner, string repo) TryParseGitHubUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return (null, null);
            }

            var pattern = @"github\.com[/:]([^/]+)/([^/\.]+)";
            var match = Regex.Match(url, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return (match.Groups[1].Value, match.Groups[2].Value);
            }

            return (null, null);
        }
    }
}
