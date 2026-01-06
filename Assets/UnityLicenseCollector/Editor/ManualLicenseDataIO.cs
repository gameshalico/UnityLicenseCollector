using System;
using System.IO;

namespace UnityLicenseCollector.Editor
{
    public sealed class ManualLicenseDataIO
    {
        public static void ExportToJson(ManualLicenseData[] licenses, string filePath)
        {
            var json = JsonHelper.ToJsonFormatted(licenses);
            File.WriteAllText(filePath, json);
        }

        public static ManualLicenseData[] ImportFromJson(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonHelper.FromJson<ManualLicenseData>(json) ?? Array.Empty<ManualLicenseData>();
        }
    }
}
