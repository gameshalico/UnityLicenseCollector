using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityLicenseCollector.Editor
{
    public sealed class ManifestParser
    {
        public List<GitHubPackageInfo> ParseManifest(string manifestPath)
        {
            var manifestJson = System.IO.File.ReadAllText(manifestPath);
            var packages = new List<GitHubPackageInfo>();

            var dependenciesMatch = Regex.Match(manifestJson, @"""dependencies""\s*:\s*\{([^}]+)\}");
            if (!dependenciesMatch.Success)
                return packages;

            var dependenciesContent = dependenciesMatch.Groups[1].Value;
            var dependencyMatches = Regex.Matches(dependenciesContent, @"""([^""]+)""\s*:\s*""([^""]+)""");

            foreach (Match match in dependencyMatches)
            {
                var packageName = match.Groups[1].Value;
                var packageUrl = match.Groups[2].Value;

                if (IsGitHubUrl(packageUrl))
                {
                    var packageInfo = ParseGitHubUrl(packageName, packageUrl);
                    if (packageInfo != null)
                    {
                        packages.Add(packageInfo);
                    }
                }
            }

            return packages;
        }

        private static bool IsGitHubUrl(string url)
        {
            return url.StartsWith("https://github.com/") || url.Contains("github.com");
        }

        private static GitHubPackageInfo ParseGitHubUrl(string packageName, string url)
        {
            var match = Regex.Match(url, @"github\.com/([^/]+)/([^/\.]+)");
            if (!match.Success)
                return null;

            var owner = match.Groups[1].Value;
            var repo = match.Groups[2].Value;
            var tag = ExtractTag(url);

            return new GitHubPackageInfo
            {
                PackageName = packageName,
                Owner = owner,
                Repository = repo,
                Url = url,
                Tag = tag
            };
        }

        private static string ExtractTag(string url)
        {
            var hashIndex = url.IndexOf('#');
            if (hashIndex < 0)
            {
                return null;
            }

            return url.Substring(hashIndex + 1);
        }
    }

    public sealed class GitHubPackageInfo
    {
        public string PackageName;
        public string Owner;
        public string Repository;
        public string Url;
        public string Tag;
    }
}
