using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace UnityLicenseCollector.Editor
{
    public sealed class NuGetApiClient
    {
        private const string ApiBaseUrl = "https://api.nuget.org/v3-flatcontainer";
        private readonly NuGetLicenseHelper _licenseHelper;

        public NuGetApiClient()
        {
            _licenseHelper = new NuGetLicenseHelper();
        }

        public async Task<NuGetLicenseData> FetchLicenseAsync(string packageId, string version, CancellationToken cancellationToken = default)
        {
            var normalizedPackageId = packageId.ToLowerInvariant();
            var normalizedVersion = version.ToLowerInvariant();
            var nuspecUrl = $"{ApiBaseUrl}/{normalizedPackageId}/{normalizedVersion}/{normalizedPackageId}.nuspec";

            var request = UnityWebRequest.Get(nuspecUrl);

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
                throw new Exception($"Failed to fetch NuGet package metadata: {error}");
            }

            var nuspecXml = request.downloadHandler.text;
            request.Dispose();

            return await _licenseHelper.ParseNuspecXmlAsync(nuspecXml, packageId, version, cancellationToken);
        }
    }
}
