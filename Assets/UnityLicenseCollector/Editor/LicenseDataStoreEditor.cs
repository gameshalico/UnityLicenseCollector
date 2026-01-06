using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityLicenseCollector.Editor
{
    [CustomEditor(typeof(LicenseDataStore))]
    public sealed class LicenseDataStoreEditor : UnityEditor.Editor
    {
        private CancellationTokenSource _cancellationTokenSource;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var visualTree = AssetDatabase.LoadAssetByGUID<VisualTreeAsset>(
                new GUID("42d9dcd6494d4bf4b83bea1f56c2f844")
            );

            if (visualTree != null)
            {
                visualTree.CloneTree(root);

                var styleSheet = AssetDatabase.LoadAssetByGUID<StyleSheet>(
                    new GUID("9a3f7c8e2b5d4a1f8c6e9d2a3b7f5c1e")
                );

                if (styleSheet != null)
                {
                    root.styleSheets.Add(styleSheet);
                }

                SetupUI(root);
            }
            else
            {
                root.Add(new Label("UXML file not found"));
            }

            return root;
        }

        private void SetupUI(VisualElement root)
        {
            var addModeField = root.Q<EnumField>("add-mode-field");
            var extractGitHubButton = root.Q<Button>("extract-github-button");
            var extractNuGetLocalButton = root.Q<Button>("extract-nuget-local-button");
            var extractNuGetApiButton = root.Q<Button>("extract-nuget-api-button");
            var clearAllButton = root.Q<Button>("clear-all-button");
            var clearGitHubButton = root.Q<Button>("clear-github-button");
            var clearNuGetButton = root.Q<Button>("clear-nuget-button");
            var clearManualButton = root.Q<Button>("clear-manual-button");
            var removeDuplicateGitHubButton = root.Q<Button>("remove-duplicate-github-button");
            var removeDuplicateNuGetButton = root.Q<Button>("remove-duplicate-nuget-button");
            var removeDuplicateManualButton = root.Q<Button>("remove-duplicate-manual-button");
            var importManualButton = root.Q<Button>("import-manual-button");
            var exportManualButton = root.Q<Button>("export-manual-button");
            var statusLabel = root.Q<Label>("status-label");

            addModeField.BindProperty(serializedObject.FindProperty("_addMode"));

            var gitHubTab = root.Q<ToolbarToggle>("github-tab");
            var nuGetTab = root.Q<ToolbarToggle>("nuget-tab");
            var manualTab = root.Q<ToolbarToggle>("manual-tab");

            var gitHubContent = root.Q<VisualElement>("github-content");
            var nuGetContent = root.Q<VisualElement>("nuget-content");
            var manualContent = root.Q<VisualElement>("manual-content");

            var gitHubLicensesContainer = root.Q<VisualElement>("github-licenses-container");
            var nuGetLicensesContainer = root.Q<VisualElement>("nuget-licenses-container");
            var manualLicensesContainer = root.Q<VisualElement>("manual-licenses-container");

            var gitHubLicensesField = new PropertyField(serializedObject.FindProperty("_gitHubLicenses"), "GitHub Licenses");
            gitHubLicensesContainer.Add(gitHubLicensesField);

            var nuGetLicensesField = new PropertyField(serializedObject.FindProperty("_nuGetLicenses"), "NuGet Licenses");
            nuGetLicensesContainer.Add(nuGetLicensesField);

            var manualLicensesField = new PropertyField(serializedObject.FindProperty("_manualLicenses"), "Manual Licenses");
            manualLicensesContainer.Add(manualLicensesField);

            gitHubTab.value = true;
            gitHubTab.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    gitHubContent.style.display = DisplayStyle.Flex;
                    nuGetContent.style.display = DisplayStyle.None;
                    manualContent.style.display = DisplayStyle.None;
                    nuGetTab.value = false;
                    manualTab.value = false;
                }
            });

            nuGetTab.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    gitHubContent.style.display = DisplayStyle.None;
                    nuGetContent.style.display = DisplayStyle.Flex;
                    manualContent.style.display = DisplayStyle.None;
                    gitHubTab.value = false;
                    manualTab.value = false;
                }
            });

            manualTab.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    gitHubContent.style.display = DisplayStyle.None;
                    nuGetContent.style.display = DisplayStyle.None;
                    manualContent.style.display = DisplayStyle.Flex;
                    gitHubTab.value = false;
                    nuGetTab.value = false;
                }
            });

            extractGitHubButton.clicked += () => ExtractFromGitHub(statusLabel);
            extractNuGetLocalButton.clicked += () => ExtractFromNuGetLocal(statusLabel);
            extractNuGetApiButton.clicked += () => ExtractFromNuGetApi(statusLabel);
            clearAllButton.clicked += () => ClearAllLicenses(statusLabel);
            clearGitHubButton.clicked += () => ClearGitHubLicenses(statusLabel);
            clearNuGetButton.clicked += () => ClearNuGetLicenses(statusLabel);
            clearManualButton.clicked += () => ClearManualLicenses(statusLabel);
            removeDuplicateGitHubButton.clicked += () => RemoveDuplicateGitHubLicenses(statusLabel);
            removeDuplicateNuGetButton.clicked += () => RemoveDuplicateNuGetLicenses(statusLabel);
            removeDuplicateManualButton.clicked += () => RemoveDuplicateManualLicenses(statusLabel);
            importManualButton.clicked += () => ImportManualLicenses(statusLabel);
            exportManualButton.clicked += () => ExportManualLicenses(statusLabel);
        }

        private async void ExtractFromGitHub(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

            if (!File.Exists(manifestPath))
            {
                statusLabel.text = "Error: manifest.json not found";
                return;
            }

            statusLabel.text = "Extracting GitHub licenses...";

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var parser = new ManifestParser();
                var packages = parser.ParseManifest(manifestPath);

                var apiClient = new GitHubApiClient();

                Undo.RecordObject(store, "Extract GitHub Licenses");

                var totalCount = packages.Count;
                var currentCount = 0;

                foreach (var package in packages)
                {
                    currentCount++;
                    EditorUtility.DisplayProgressBar(
                        "Extracting GitHub Licenses",
                        $"Processing {package.PackageName} ({currentCount}/{totalCount})",
                        (float)currentCount / totalCount);

                    try
                    {
                        var licenseData = await apiClient.FetchLicenseAsync(
                            package.Owner,
                            package.Repository,
                            package.Tag,
                            _cancellationTokenSource.Token);

                        store.AddGitHubLicense(licenseData);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"Failed to fetch license for {package.PackageName}: {ex.Message}");
                    }
                }

                EditorUtility.ClearProgressBar();

                EditorUtility.SetDirty(store);
                AssetDatabase.SaveAssets();

                statusLabel.text = $"Extracted {packages.Count} GitHub licenses";
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                statusLabel.text = $"Error: {ex.Message}";
                Debug.LogError(ex);
            }
        }

        private async void ExtractFromNuGetApi(Label statusLabel)
        {
            var packagesConfigPath = EditorUtility.OpenFilePanel("Select packages.config", Application.dataPath, "config");
            if (string.IsNullOrEmpty(packagesConfigPath))
            {
                return;
            }

            var store = (LicenseDataStore)target;
            statusLabel.text = "Extracting NuGet licenses from API...";

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var parser = new PackagesConfigParser();
                var packages = parser.ParsePackagesConfig(packagesConfigPath);

                var apiClient = new NuGetApiClient();

                Undo.RecordObject(store, "Extract NuGet Licenses from API");

                var totalCount = packages.Count;
                var currentCount = 0;

                foreach (var package in packages)
                {
                    currentCount++;
                    EditorUtility.DisplayProgressBar(
                        "Extracting NuGet Licenses from API",
                        $"Processing {package.PackageId} ({currentCount}/{totalCount})",
                        (float)currentCount / totalCount);

                    try
                    {
                        var licenseData = await apiClient.FetchLicenseAsync(
                            package.PackageId,
                            package.Version,
                            _cancellationTokenSource.Token);

                        store.AddNuGetLicense(licenseData);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"Failed to fetch license for {package.PackageId}: {ex.Message}");
                    }
                }

                EditorUtility.ClearProgressBar();

                EditorUtility.SetDirty(store);
                AssetDatabase.SaveAssets();

                statusLabel.text = $"Extracted {packages.Count} NuGet licenses from API";
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                statusLabel.text = $"Error: {ex.Message}";
                Debug.LogError(ex);
            }
        }

        private async void ExtractFromNuGetLocal(Label statusLabel)
        {
            var installedPackagesPath = EditorUtility.OpenFolderPanel("Select InstalledPackages folder", "", "");
            if (string.IsNullOrEmpty(installedPackagesPath))
            {
                return;
            }

            var store = (LicenseDataStore)target;
            statusLabel.text = "Extracting NuGet licenses from local folder...";

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var reader = new NuGetLocalReader(installedPackagesPath);
                var parser = new NuGetPackageDirectoryParser();
                var packageDirectories = Directory.GetDirectories(installedPackagesPath);

                Undo.RecordObject(store, "Extract NuGet Licenses from Local");

                var totalCount = packageDirectories.Length;
                var currentCount = 0;

                foreach (var packageDirectory in packageDirectories)
                {
                    var directoryName = Path.GetFileName(packageDirectory);

                    if (!parser.TryParseDirectoryName(directoryName, out var packageId, out var version))
                    {
                        Debug.LogWarning($"Skipping invalid directory name: {directoryName}");
                        continue;
                    }

                    currentCount++;
                    EditorUtility.DisplayProgressBar(
                        "Extracting NuGet Licenses from Local",
                        $"Processing {packageId} ({currentCount}/{totalCount})",
                        (float)currentCount / totalCount);

                    try
                    {
                        var licenseData = await reader.ReadLicenseAsync(packageId, version, _cancellationTokenSource.Token);
                        store.AddNuGetLicense(licenseData);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"Failed to read license for {packageId}: {ex.Message}");
                    }
                }

                EditorUtility.ClearProgressBar();

                EditorUtility.SetDirty(store);
                AssetDatabase.SaveAssets();

                statusLabel.text = $"Extracted {packageDirectories.Length} NuGet licenses from local folder";
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                statusLabel.text = $"Error: {ex.Message}";
                Debug.LogError(ex);
            }
        }

        private void ClearAllLicenses(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            Undo.RecordObject(store, "Clear All Licenses");
            store.ClearAll();

            EditorUtility.SetDirty(store);
            AssetDatabase.SaveAssets();

            statusLabel.text = "All licenses cleared";
        }

        private void ClearGitHubLicenses(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            Undo.RecordObject(store, "Clear GitHub Licenses");
            store.ClearGitHubLicenses();

            EditorUtility.SetDirty(store);
            AssetDatabase.SaveAssets();

            statusLabel.text = "GitHub licenses cleared";
        }

        private void ClearNuGetLicenses(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            Undo.RecordObject(store, "Clear NuGet Licenses");
            store.ClearNuGetLicenses();

            EditorUtility.SetDirty(store);
            AssetDatabase.SaveAssets();

            statusLabel.text = "NuGet licenses cleared";
        }

        private void ClearManualLicenses(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            Undo.RecordObject(store, "Clear Manual Licenses");
            store.ClearManualLicenses();

            EditorUtility.SetDirty(store);
            AssetDatabase.SaveAssets();

            statusLabel.text = "Manual licenses cleared";
        }

        private void RemoveDuplicateGitHubLicenses(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            var originalCount = store.GitHubLicenses.Count;

            Undo.RecordObject(store, "Remove Duplicate GitHub Licenses");
            store.RemoveDuplicateGitHubLicenses();

            EditorUtility.SetDirty(store);
            AssetDatabase.SaveAssets();

            var removedCount = originalCount - store.GitHubLicenses.Count;
            statusLabel.text = $"Removed {removedCount} duplicate GitHub licenses";
        }

        private void RemoveDuplicateNuGetLicenses(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            var originalCount = store.NuGetLicenses.Count;

            Undo.RecordObject(store, "Remove Duplicate NuGet Licenses");
            store.RemoveDuplicateNuGetLicenses();

            EditorUtility.SetDirty(store);
            AssetDatabase.SaveAssets();

            var removedCount = originalCount - store.NuGetLicenses.Count;
            statusLabel.text = $"Removed {removedCount} duplicate NuGet licenses";
        }

        private void RemoveDuplicateManualLicenses(Label statusLabel)
        {
            var store = (LicenseDataStore)target;
            var originalCount = store.ManualLicenses.Count;

            Undo.RecordObject(store, "Remove Duplicate Manual Licenses");
            store.RemoveDuplicateManualLicenses();

            EditorUtility.SetDirty(store);
            AssetDatabase.SaveAssets();

            var removedCount = originalCount - store.ManualLicenses.Count;
            statusLabel.text = $"Removed {removedCount} duplicate Manual licenses";
        }

        private void ImportManualLicenses(Label statusLabel)
        {
            var filePath = EditorUtility.OpenFilePanel("Import Manual Licenses", Application.dataPath, "json");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                var licenses = ManualLicenseDataIO.ImportFromJson(filePath);
                var store = (LicenseDataStore)target;

                Undo.RecordObject(store, "Import Manual Licenses");

                foreach (var license in licenses)
                {
                    store.AddManualLicense(license);
                }

                EditorUtility.SetDirty(store);
                AssetDatabase.SaveAssets();

                statusLabel.text = $"Imported {licenses.Length} manual licenses";
            }
            catch (System.Exception ex)
            {
                statusLabel.text = $"Import failed: {ex.Message}";
                Debug.LogError(ex);
            }
        }

        private void ExportManualLicenses(Label statusLabel)
        {
            var filePath = EditorUtility.SaveFilePanel("Export Manual Licenses", Application.dataPath, "ManualLicenses", "json");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                var store = (LicenseDataStore)target;
                var licenses = new ManualLicenseData[store.ManualLicenses.Count];
                for (var i = 0; i < store.ManualLicenses.Count; i++)
                {
                    licenses[i] = store.ManualLicenses[i];
                }

                ManualLicenseDataIO.ExportToJson(licenses, filePath);
                statusLabel.text = $"Exported {licenses.Length} manual licenses";
            }
            catch (System.Exception ex)
            {
                statusLabel.text = $"Export failed: {ex.Message}";
                Debug.LogError(ex);
            }
        }

        private void OnDisable()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
