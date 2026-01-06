using System;
using UnityEngine;

namespace UnityLicenseCollector
{
    [Serializable]
    public sealed class ManualLicenseData
    {
        public string PackageName;
        public string Authors;
        public string Version;
        public string ProjectUrl;
        public string LicenseUrl;
        public string LicenseIdentifier;
        [TextArea]
        public string LicenseContent;
    }
}
