# Installation Guide

This guide covers all methods for installing TimeWarp.Amuru components and standalone executables.

## Table of Contents

- [Quick Start](#quick-start)
- [Component Overview](#component-overview)
- [Installing the Library](#installing-the-library)
- [Installing CLI Tools](#installing-cli-tools)
- [Installing Standalone Executables](#installing-standalone-executables)
  - [Method 1: Using dnx (One-Shot Execution)](#method-1-using-dnx-one-shot-execution)
  - [Method 2: Using timewarp install (Permanent Tool)](#method-2-using-timewarp-install-permanent-tool)
  - [Method 3: Using the Standalone Installer](#method-3-using-the-standalone-installer)
  - [Method 4: Manual Download from GitHub](#method-4-manual-download-from-github)
- [Verifying Installation](#verifying-installation)
- [Updating](#updating)
- [Uninstalling](#uninstalling)

---

## Quick Start

**For C# scripts and libraries:**
```bash
dotnet add package TimeWarp.Amuru
```

**For CLI utilities:**
```bash
dotnet tool install --global TimeWarp.Ganda
```

**For standalone executables:**
```bash
# Simplest method (one command, no permanent tool installation):
dnx --prerelease TimeWarp.Ganda install
```

---

## Component Overview

TimeWarp.Amuru provides three types of components:

| Component | Description | Installation Method |
|-----------|-------------|---------------------|
| **TimeWarp.Amuru** | Core library for shell scripting and process execution | NuGet package |
| **TimeWarp.Ganda** | Global CLI tool (`timewarp` command) with utilities | dotnet tool |
| **Standalone Executables** | AOT-compiled native executables (multiavatar, generate-avatar, etc.) | Installer script or manual download |

---

## Installing the Library

### As a NuGet Package

```bash
# Add to your project
dotnet add package TimeWarp.Amuru

# Or with specific version
dotnet add package TimeWarp.Amuru --version 1.0.0-beta.10
```

### In .NET 10 File-Based Apps

For single-file C# scripts:

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru@1.0.0-beta.10

using TimeWarp.Amuru;

await Shell.Builder("echo", "Hello, World!").RunAsync();
```

The `#:package` directive is native to .NET 10 - no external tools required.

---

## Installing CLI Tools

### TimeWarp.Ganda Global Tool

```bash
# Install globally
dotnet tool install --global TimeWarp.Ganda

# Or update existing installation
dotnet tool update --global TimeWarp.Ganda

# Verify installation
timewarp --version
```

The `timewarp` command provides various utilities:
- `timewarp convert-timestamp` - Timestamp conversion
- `timewarp generate-color` - Color generation
- And more (see [TimeWarp.Ganda documentation](../../Source/TimeWarp.Ganda/README.md))

---

## Installing Standalone Executables

Standalone executables are AOT-compiled native binaries that don't require the .NET SDK. They are distributed as platform-specific archives via GitHub Releases.

### Available Executables

The CI/CD pipeline automatically builds and publishes the following executables:

- **multiavatar** - Generate multiavatar images
- **generate-avatar** - Generate repository avatars
- **convert-timestamp** - Convert between timestamp formats
- **generate-color** - Generate color palettes
- **ssh-key-helper** - SSH key management utilities

Each executable is compiled for:
- **Linux** (linux-x64)
- **Windows** (win-x64)
- **macOS** (osx-x64)

### Choosing an Installation Method

Each method serves a different purpose:

- **Method 1 (dnx)**: Quickest way to install - one command, no permanent tool installation
- **Method 2 (timewarp install)**: For users who want the `timewarp` command permanently for multiple operations
- **Method 3 (standalone installer)**: For users without .NET SDK at all (downloads pre-built binary)
- **Method 4 (manual)**: For advanced users or when other methods fail

### Method 1: Using dnx (One-Shot Execution)

`dnx` is a .NET 10 command that runs NuGet tools without permanent installation (similar to Node.js's `npx`).

**Single Command Installation:**

```bash
# For current beta versions (requires --prerelease)
dnx --prerelease TimeWarp.Ganda install

# For stable releases (when available)
dnx TimeWarp.Ganda install

# With specific version
dnx TimeWarp.Ganda@1.0.0-beta.13 install
```

**What happens:**
1. `dnx` prompts for confirmation to download TimeWarp.Ganda from NuGet
2. Downloads and runs the tool (without permanent installation)
3. The tool's `install` command downloads standalone executables from GitHub releases
4. Verifies attestation (if `gh` CLI is installed)
5. Installs to `~/.local/bin` (Linux/macOS) or `%USERPROFILE%\.tools` (Windows)

**Example output:**

```
$ dnx --prerelease TimeWarp.Ganda install
Tool package timewarp.ganda@1.0.0-beta.13 will be downloaded from source https://api.nuget.org/v3/index.json.
Proceed? [y/n] (y): y
Installing TimeWarp utilities v1.0.0-beta.13...

Downloading utilities from GitHub releases...
  URL: https://github.com/TimeWarpEngineering/timewarp-amuru/releases/download/v1.0.0-beta.13/timewarp-utilities-linux-x64.tar.gz
✓ Downloaded to /tmp/timewarp-utilities-linux-x64.tar.gz
Verifying attestation with GitHub CLI...
✓ Attestation verified successfully
Extracting utilities...
✓ Installed multiavatar to /home/user/.local/bin/multiavatar
✓ Installed generate-avatar to /home/user/.local/bin/generate-avatar
✓ Installed convert-timestamp to /home/user/.local/bin/convert-timestamp
✓ Installed generate-color to /home/user/.local/bin/generate-color
✓ Installed ssh-key-helper to /home/user/.local/bin/ssh-key-helper

Installation complete!
Make sure /home/user/.local/bin is in your PATH.
```

**Additional dnx options:**

```bash
# Auto-accept confirmation prompt
dnx -y --prerelease TimeWarp.Ganda install

# Install specific utilities only
dnx --prerelease TimeWarp.Ganda install multiavatar generate-avatar

# See all dnx options
dnx --help
```

**Note**: The `--prerelease` flag is required for beta versions. Once stable versions are released, you can omit this flag.

### Method 2: Using timewarp install (Permanent Tool)

If you want the `timewarp` command available permanently for multiple operations (not just installation), install TimeWarp.Ganda as a global tool:

**Step 1: Install TimeWarp.Ganda globally**

```bash
# For current beta versions
dotnet tool install --global TimeWarp.Ganda --prerelease

# For stable releases (when available)
dotnet tool install --global TimeWarp.Ganda
```

**Step 2: Run the install command**

```bash
# Install all standalone utilities
timewarp install

# Install specific utilities only
timewarp install multiavatar generate-avatar
```

This gives you:
- The `timewarp` command permanently in your PATH
- Access to all timewarp commands (convert-timestamp, generate-color, etc.)
- Ability to run `timewarp install` to update standalone executables

**Verify installation:**

```bash
timewarp --version
```

### Method 3: Using the Standalone Installer

For users without .NET SDK, download and run the pre-built installer executable. The installer:
- Downloads the correct platform-specific archive
- Verifies attestation with GitHub CLI (if available)
- Extracts and installs all executables
- Makes them executable (on Unix-like systems)

#### Step 1: Download the Installer

**Linux/macOS:**
```bash
# Download the installer from latest release
curl -L -o installer https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/installer-linux-x64

# Or for macOS
curl -L -o installer https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/installer-osx-x64

# Make it executable
chmod +x installer
```

**Windows (PowerShell):**
```powershell
# Download the installer from latest release
Invoke-WebRequest -Uri https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/installer-win-x64.exe -OutFile installer.exe
```

#### Step 2: Run the Installer

**Linux/macOS:**
```bash
# Install all utilities
./installer

# Install specific utilities only
./installer multiavatar generate-avatar
```

**Windows:**
```powershell
# Install all utilities
.\installer.exe

# Install specific utilities only
.\installer.exe multiavatar generate-avatar
```

The installer will:
1. Download the utilities archive for your platform
2. Verify attestation (if `gh` CLI is installed)
3. Extract executables to the installation directory
4. Report installation success

#### Installation Directories

The installer places executables in platform-specific directories:

- **Linux/macOS**: `~/.local/bin`
- **Windows**: `%USERPROFILE%\.tools`

**IMPORTANT**: Ensure this directory is in your PATH:

**Linux/macOS:**
```bash
# Add to ~/.bashrc or ~/.zshrc
export PATH="$HOME/.local/bin:$PATH"
```

**Windows (PowerShell):**
```powershell
# Add to PowerShell profile
$env:Path = "$env:USERPROFILE\.tools;$env:Path"

# Or permanently via System Properties
[Environment]::SetEnvironmentVariable("Path", "$env:USERPROFILE\.tools;$env:Path", "User")
```

### Method 4: Manual Download from GitHub

For advanced users or when other methods fail, manually download and extract the utilities archive:

#### Step 1: Download the Archive

Visit the [Releases page](https://github.com/TimeWarpEngineering/timewarp-amuru/releases) and download the appropriate archive:

- `timewarp-utilities-linux-x64.tar.gz` (Linux)
- `timewarp-utilities-win-x64.zip` (Windows)
- `timewarp-utilities-osx-x64.tar.gz` (macOS)

Or use the command line:

**Linux:**
```bash
wget https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/timewarp-utilities-linux-x64.tar.gz
```

**macOS:**
```bash
curl -L -O https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/timewarp-utilities-osx-x64.tar.gz
```

**Windows:**
```powershell
Invoke-WebRequest -Uri https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/timewarp-utilities-win-x64.zip -OutFile timewarp-utilities-win-x64.zip
```

#### Step 2: Extract the Archive

**Linux/macOS:**
```bash
# Extract to ~/.local/bin
mkdir -p ~/.local/bin
tar -xzf timewarp-utilities-*.tar.gz -C ~/.local/bin --strip-components=1

# Make executables
chmod +x ~/.local/bin/multiavatar
chmod +x ~/.local/bin/generate-avatar
chmod +x ~/.local/bin/convert-timestamp
chmod +x ~/.local/bin/generate-color
chmod +x ~/.local/bin/ssh-key-helper
```

**Windows:**
```powershell
# Extract to %USERPROFILE%\.tools
$installDir = "$env:USERPROFILE\.tools"
New-Item -ItemType Directory -Force -Path $installDir
Expand-Archive -Path timewarp-utilities-win-x64.zip -DestinationPath $installDir -Force
```

#### Step 3: Verify Attestation (Optional but Recommended)

If you have the GitHub CLI (`gh`) installed, verify the archive attestation:

```bash
# Verify the downloaded archive
gh attestation verify timewarp-utilities-*.tar.gz --repo TimeWarpEngineering/timewarp-amuru
```

This confirms the archive was built by TimeWarpEngineering's CI/CD pipeline and hasn't been tampered with.

---

## Verifying Installation

### Verify Library Installation

```bash
# Check installed NuGet packages
dotnet list package | grep TimeWarp.Amuru
```

### Verify CLI Tools

```bash
# Check timewarp tool
timewarp --version
```

### Verify Standalone Executables

```bash
# Check multiavatar
multiavatar --version

# Check generate-avatar
generate-avatar --version

# Check convert-timestamp
convert-timestamp --help

# Check generate-color
generate-color --help

# Check ssh-key-helper
ssh-key-helper --help
```

If executables aren't found, ensure the installation directory is in your PATH (see [Installation Directories](#installation-directories)).

---

## Updating

### Update Library

```bash
dotnet add package TimeWarp.Amuru
```

### Update CLI Tools

```bash
dotnet tool update --global TimeWarp.Ganda
```

### Update Standalone Executables

Choose one of the following methods to update:

**Method 1 (dnx - one command):**

```bash
# Re-run the install command (downloads latest from NuGet)
dnx --prerelease TimeWarp.Ganda install
```

**Method 2 (timewarp - if already installed globally):**

```bash
# Update the tool first, then re-run install
dotnet tool update --global TimeWarp.Ganda --prerelease
timewarp install
```

**Method 3 (standalone installer):**

```bash
# Download latest installer
curl -L -o installer https://github.com/TimeWarpEngineering/timewarp-amuru/releases/latest/download/installer-linux-x64

# Make executable and run
chmod +x installer
./installer
```

**Method 4 (manual):**

Manually download the latest utilities archive and extract as described in [Method 4](#method-4-manual-download-from-github).

---

## Uninstalling

### Uninstall Library

```bash
dotnet remove package TimeWarp.Amuru
```

### Uninstall CLI Tools

```bash
dotnet tool uninstall --global TimeWarp.Ganda
```

### Uninstall Standalone Executables

**Linux/macOS:**
```bash
# Remove executables from ~/.local/bin
rm ~/.local/bin/multiavatar
rm ~/.local/bin/generate-avatar
rm ~/.local/bin/convert-timestamp
rm ~/.local/bin/generate-color
rm ~/.local/bin/ssh-key-helper
```

**Windows:**
```powershell
# Remove executables from %USERPROFILE%\.tools
Remove-Item "$env:USERPROFILE\.tools\multiavatar.exe"
Remove-Item "$env:USERPROFILE\.tools\generate-avatar.exe"
Remove-Item "$env:USERPROFILE\.tools\convert-timestamp.exe"
Remove-Item "$env:USERPROFILE\.tools\generate-color.exe"
Remove-Item "$env:USERPROFILE\.tools\ssh-key-helper.exe"
```

---

## Troubleshooting

### Executables Not Found in PATH

If you get "command not found" errors, the installation directory is not in your PATH.

**Linux/macOS:**
```bash
# Add to ~/.bashrc or ~/.zshrc
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

**Windows:**
```powershell
# View current PATH
$env:Path

# Add to PATH permanently
[Environment]::SetEnvironmentVariable("Path", "$env:USERPROFILE\.tools;$env:Path", "User")

# Restart PowerShell after changing PATH
```

### Attestation Verification Failed

If attestation verification fails:

1. Ensure `gh` CLI is installed and authenticated:
   ```bash
   gh auth status
   ```

2. If you trust the source, you can skip verification by answering "y" when prompted by the installer.

3. Alternatively, manually verify using `gh attestation verify` as shown in [Method 4, Step 3](#step-3-verify-attestation-optional-but-recommended).

### Permission Denied (Linux/macOS)

If you get "Permission denied" when running executables:

```bash
# Make the file executable
chmod +x ~/.local/bin/multiavatar
```

### .NET SDK Version Mismatch

TimeWarp.Amuru requires .NET 10. Check your version:

```bash
dotnet --version
```

If you have an older version, download .NET 10 from [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0).

---

## CI/CD Pipeline

The CI/CD pipeline automatically:

1. **Builds all standalone executables** for Linux, Windows, and macOS
2. **Creates platform-specific archives** (`.tar.gz` for Unix, `.zip` for Windows)
3. **Generates attestations** for all archives using GitHub's attestation service
4. **Builds the installer executable** for each platform
5. **Publishes to GitHub Releases** when a release is created

All executables in the `exe/` directory (except `installer.cs` and test files) are automatically included in the build.

**Artifacts published to releases:**
- `timewarp-utilities-linux-x64.tar.gz`
- `timewarp-utilities-win-x64.zip`
- `timewarp-utilities-osx-x64.tar.gz`
- `installer-linux-x64`
- `installer-win-x64.exe`
- `installer-osx-x64`
- `TimeWarp.Amuru.*.nupkg`
- `TimeWarp.Ganda.*.nupkg`

---

## See Also

- [README.md](../../README.md) - Project overview and quick start
- [TimeWarp.Ganda README](../../Source/TimeWarp.Ganda/README.md) - CLI tool documentation
- [Architectural Layers](../Conceptual/ArchitecturalLayers.md) - Understanding the architecture
