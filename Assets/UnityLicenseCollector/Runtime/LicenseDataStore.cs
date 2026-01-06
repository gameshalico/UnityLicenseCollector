using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityLicenseCollector
{
    public enum LicenseAddMode
    {
        Skip,
        Overwrite,
        Add
    }
    [CreateAssetMenu(fileName = "LicenseDataStore", menuName = "UnityLicenseCollector/License Data Store")]
    public sealed class LicenseDataStore : ScriptableObject
    {
        [SerializeField] private List<GitHubLicenseData> _gitHubLicenses = new();
        [SerializeField] private List<NuGetLicenseData> _nuGetLicenses = new();
        [SerializeField] private List<ManualLicenseData> _manualLicenses = new();
        [SerializeField] private LicenseAddMode _addMode = LicenseAddMode.Skip;

        public IReadOnlyList<GitHubLicenseData> GitHubLicenses => _gitHubLicenses;
        public IReadOnlyList<NuGetLicenseData> NuGetLicenses => _nuGetLicenses;
        public IReadOnlyList<ManualLicenseData> ManualLicenses => _manualLicenses;
        public LicenseAddMode AddMode => _addMode;

        public void AddGitHubLicense(GitHubLicenseData license)
        {
            AddLicenseInternal(_gitHubLicenses, license, l => l.PackageName == license.PackageName);
        }

        public void AddNuGetLicense(NuGetLicenseData license)
        {
            AddLicenseInternal(_nuGetLicenses, license, l => l.PackageName == license.PackageName && l.Version == license.Version);
        }

        public void AddManualLicense(ManualLicenseData license)
        {
            _manualLicenses.Add(license);
        }

        private void AddLicenseInternal<T>(List<T> list, T license, System.Func<T, bool> isDuplicate)
        {
            switch (_addMode)
            {
                case LicenseAddMode.Skip:
                    if (!list.Any(isDuplicate))
                    {
                        list.Add(license);
                    }
                    break;
                case LicenseAddMode.Overwrite:
                    var index = list.FindIndex(l => isDuplicate(l));
                    if (index >= 0)
                    {
                        list[index] = license;
                    }
                    else
                    {
                        list.Add(license);
                    }
                    break;
                case LicenseAddMode.Add:
                    list.Add(license);
                    break;
            }
        }

        public void ClearGitHubLicenses()
        {
            _gitHubLicenses.Clear();
        }

        public void ClearNuGetLicenses()
        {
            _nuGetLicenses.Clear();
        }

        public void ClearManualLicenses()
        {
            _manualLicenses.Clear();
        }

        public void ClearAll()
        {
            _gitHubLicenses.Clear();
            _nuGetLicenses.Clear();
            _manualLicenses.Clear();
        }

        public void RemoveDuplicateGitHubLicenses()
        {
            var otherPackageNames = _nuGetLicenses.Select(l => l.PackageName)
                .Concat(_manualLicenses.Select(l => l.PackageName))
                .ToHashSet();
            RemoveDuplicatesInternal(_gitHubLicenses, otherPackageNames, l => l.PackageName);
        }

        public void RemoveDuplicateNuGetLicenses()
        {
            var otherPackageNames = _gitHubLicenses.Select(l => l.PackageName)
                .Concat(_manualLicenses.Select(l => l.PackageName))
                .ToHashSet();
            RemoveDuplicatesInternal(_nuGetLicenses, otherPackageNames, l => l.PackageName);
        }

        public void RemoveDuplicateManualLicenses()
        {
            var otherPackageNames = _gitHubLicenses.Select(l => l.PackageName)
                .Concat(_nuGetLicenses.Select(l => l.PackageName))
                .ToHashSet();
            RemoveDuplicatesInternal(_manualLicenses, otherPackageNames, l => l.PackageName);
        }

        private void RemoveDuplicatesInternal<T>(List<T> targetList, HashSet<string> excludePackageNames, System.Func<T, string> getPackageName)
        {
            var uniqueLicenses = new Dictionary<string, T>();
            foreach (var license in targetList)
            {
                var packageName = getPackageName(license);
                if (!excludePackageNames.Contains(packageName))
                {
                    uniqueLicenses[packageName] = license;
                }
            }

            targetList.Clear();
            targetList.AddRange(uniqueLicenses.Values);
        }

        public IEnumerable<ILicenseData> GetAllLicenses()
        {
            foreach (var license in _gitHubLicenses)
            {
                yield return new GitHubLicenseDataAdapter(license);
            }

            foreach (var license in _nuGetLicenses)
            {
                yield return new NuGetLicenseDataAdapter(license);
            }

            foreach (var license in _manualLicenses)
            {
                yield return new ManualLicenseDataAdapter(license);
            }
        }
    }
}
