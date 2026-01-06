# UnityLicenseCollector
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[日本語版](./README_JP.md)

An editor extension tool that automatically retrieves license information for GitHub packages and NuGet packages in Unity projects and saves them to a ScriptableObject.

## Overview

UnityLicenseCollector is a tool for centrally managing license information of external packages used in your project. It can retrieve license information from the following three sources:

- **GitHub**: GitHub packages listed in `Packages/manifest.json`
- **NuGet**: Packages installed via NuGet package managers
- **Manual**: Custom license information added manually

![](./docs/screenshot_all.png)

## Installation

### Installation Steps

1. Open Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top left
3. Select "Add package from git URL"
4. Enter the following URL:
   ```
   https://github.com/gameshalico/UnityLicenseCollector.git?path=Assets/UnityLicenseCollector
   ```

### Optional

- To retrieve NuGet package licenses, you need one of the following:
  - [NuGet Importer](https://github.com/kumaS-nu/NuGet-importer-for-Unity)
  - [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)

※ OpenUPM is not supported

## Usage

### 1. Create LicenseDataStore

1. Right-click in the Project window
2. Select `Create > UnityLicenseCollector > LicenseDataStore`
3. Save it in any location (e.g., `Assets/LicenseData.asset`)

### 2. Retrieve License Information

When you select the created `LicenseDataStore` in the Inspector, three tabs will be displayed.

#### GitHub Tab

![](./docs/screenshot_github.png)

Retrieves package license information from GitHub.

1. Select the **GitHub** tab
2. Click the **Extract from GitHub (manifest.json)** button
3. Automatically reads `Packages/manifest.json` and retrieves licenses for packages with GitHub URLs

**Features:**
- Automatically detects GitHub packages from `Packages/manifest.json`
- Retrieves license text via GitHub API
- **Remove Duplicates**: Removes packages that duplicate those in other tabs
- **Clear GitHub Licenses**: Clears only GitHub licenses

#### NuGet Tab

![](./docs/screenshot_nuget.png)

Retrieves license information for NuGet packages.

##### Extract from Local Packages

1. Select the **NuGet** tab
2. Click the **Extract from NuGetForUnity (InstalledPackages)** button
3. Select the `InstalledPackages` directory in the folder selection dialog
   - For NuGetForUnity: `Assets/Packages/`
   - For NuGet Importer: `InstalledPackages/` in the project root
4. Automatically extracts license information

##### Extract from NuGet API

1. Select the **NuGet** tab
2. Click the **Extract from NuGet API (packages.config)** button
3. Select the `packages.config` file
4. Retrieves license information via NuGet API

**Features:**
- Retrieves metadata from local `.nuspec` files
- Prioritizes `LICENSE` files if they exist
- Attempts to retrieve via GitHub fallback if no license file is found
- **Remove Duplicates**: Removes packages that duplicate those in other tabs
- **Clear NuGet Licenses**: Clears only NuGet licenses

#### Manual Tab

![](./docs/screenshot_manual.png)

Manually manage license information.

##### Import from JSON

1. Select the **Manual** tab
2. Click the **Import from JSON** button
3. Select a JSON file containing license data

**JSON Format Example:**
```json
{
  "Licenses": [{
    "PackageName": "MyCustomPackage",
    "Version": "1.0.0",
    "LicenseName": "MIT",
    "LicenseText": "MIT License\n\nCopyright (c) 2025...",
    "Author": "Author Name",
    "RepositoryUrl": "https://github.com/username/repo"
  }]
}
```

##### Export to JSON

1. Select the **Manual** tab
2. Click the **Export to JSON** button
3. Select the destination and export

**Features:**
- Import/export in JSON format
- **Remove Duplicates**: Removes packages that duplicate those in other tabs
- **Clear Manual Licenses**: Clears only manual licenses

### 3. License Add Mode

You can configure the behavior when adding licenses using the **Add Mode** at the top of the Inspector.

- **Skip**: Skips adding if a package with the same name already exists
- **Overwrite**: Overwrites existing license information
- **Add**: Adds as a separate entry even if the same name exists

### 4. Clear All Data

The **Clear All Licenses** button allows you to delete all license data from all tabs at once.

## How License Retrieval Works

### GitHub

1. Parses `Packages/manifest.json` to extract GitHub URLs
2. Retrieves license information via GitHub API (`/repos/{owner}/{repo}/license`)
3. Decodes Base64-encoded license text
4. Saves to ScriptableObject

### NuGet (Local)

1. Detects packages from the `InstalledPackages` directory
2. XML-parses `.nuspec` files to retrieve metadata
3. Uses `LICENSE*` files if they exist
4. If no license file exists, extracts GitHub URL from `ProjectUrl` and falls back
5. Saves to ScriptableObject

### NuGet (API)

1. Retrieves package ID and version from `packages.config`
2. Downloads `.nuspec` via NuGet API (`v3-flatcontainer`)
3. XML-parses `.nuspec` to retrieve license information
4. Executes GitHub fallback process if no license is found
5. Saves to ScriptableObject

## Troubleshooting

### Cannot Retrieve GitHub Licenses

- You may have reached GitHub API rate limits (60 requests per hour)
- The package may not have a license file
- Check error messages in the Console window

### Cannot Retrieve NuGet Licenses

- The `.nuspec` file may not contain license information
- Fallback won't work if `ProjectUrl` is not GitHub
- Verify that the local `InstalledPackages` directory path is correct

### Duplicate Entries Appear

- You can remove duplicate licenses in each tab using the **Remove Duplicates** button
- Setting **Add Mode** to **Skip** or **Overwrite** prevents duplicates
