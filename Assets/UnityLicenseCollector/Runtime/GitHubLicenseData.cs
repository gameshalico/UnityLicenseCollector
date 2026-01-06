using System;
using UnityEngine;

namespace UnityLicenseCollector
{
    [Serializable]
    public sealed class GitHubLicenseData
    {
        public string PackageName;
        public string Authors;
        public string Version;
        public string ProjectUrl;
        public string LicenseUrl;
        public string LicenseHtmlUrl;
        public string LicenseSpdxId;
        public string LicenseKey;
        public string LicenseName;
        public string LicenseNodeId;
        [TextArea]
        public string LicenseContent;
    }
}
