using System;
using UnityEngine;

namespace UnityLicenseCollector
{
    [Serializable]
    public sealed class NuGetLicenseData
    {
        public string PackageName;
        public string Authors;
        public string Version;
        public string ProjectUrl;
        public string LicenseUrl;
        public string LicenseExpression;
        public string LicenseType;
        public string LicenseVersion;
        public string Description;
        public string Copyright;
        public string Tags;
        [TextArea]
        public string LicenseContent;
    }
}
