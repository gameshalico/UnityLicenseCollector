using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityLicenseCollector.Editor
{
    public sealed class GitHubApiClient
    {
        private const string ApiBaseUrl = "https://api.github.com";

        public async Task<GitHubLicenseData> FetchLicenseAsync(string owner, string repo, string tag, CancellationToken cancellationToken)
        {
            var licenseUrl = $"{ApiBaseUrl}/repos/{owner}/{repo}/license";
            var request = UnityWebRequest.Get(licenseUrl);
            request.SetRequestHeader("Accept", "application/vnd.github.v3+json");
            request.SetRequestHeader("User-Agent", "UnityLicenseCollector");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    request.Dispose();
                    throw new OperationCanceledException();
                }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                var error = request.error;
                request.Dispose();
                throw new Exception($"Failed to fetch license from GitHub: {error}");
            }

            var json = request.downloadHandler.text;
            request.Dispose();

            return ParseLicenseResponse(json, owner, repo, tag);
        }

        private static GitHubLicenseData ParseLicenseResponse(string json, string owner, string repo, string tag)
        {
            var response = JsonUtility.FromJson<GitHubLicenseResponse>(json);

            return new GitHubLicenseData
            {
                Authors = owner,
                PackageName = repo,
                ProjectUrl = $"https://github.com/{owner}/{repo}",
                Version = tag,
                LicenseKey = response.license?.key,
                LicenseName = response.license?.name,
                LicenseSpdxId = response.license?.spdx_id,
                LicenseUrl = response.license?.url,
                LicenseNodeId = response.license?.node_id,
                LicenseHtmlUrl = response.license?.html_url,
                LicenseContent = DecodeContent(response.content, response.encoding)
            };
        }

        private static string DecodeContent(string content, string encoding)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            if (encoding == "base64")
            {
                var cleanedContent = content.Replace("\n", "").Replace("\r", "");
                var bytes = Convert.FromBase64String(cleanedContent);
                return Encoding.UTF8.GetString(bytes);
            }

            return content;
        }

        [Serializable]
        private sealed class GitHubLicenseResponse
        {
            public string name;
            public string path;
            public string sha;
            public int size;
            public string url;
            public string html_url;
            public string git_url;
            public string download_url;
            public string type;
            public string content;
            public string encoding;
            public LicenseInfo license;
        }

        [Serializable]
        private sealed class LicenseInfo
        {
            public string key;
            public string name;
            public string spdx_id;
            public string url;
            public string node_id;
            public string html_url;
        }
    }
}
