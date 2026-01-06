using System;
using System.IO;
using UnityEngine;

namespace UnityLicenseCollector.Editor
{
    [Serializable]
    internal sealed class ManualLicenseDataList
    {
        public ManualLicenseData[] Licenses;
    }

    public sealed class ManualLicenseDataIO
    {
        public static void ExportToJson(ManualLicenseData[] licenses, string filePath)
        {
            var wrapper = new ManualLicenseDataList { Licenses = licenses };
            var json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(filePath, json);
        }

        public static ManualLicenseData[] ImportFromJson(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var wrapper = JsonUtility.FromJson<ManualLicenseDataList>(json);
            return wrapper?.Licenses ?? Array.Empty<ManualLicenseData>();
        }
    }
}
