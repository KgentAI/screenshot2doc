# Installation and Setup

<cite>
**Referenced Files in This Document**   
- [README.md](file://README.md)
- [初始配置.txt](file://初始配置.txt)
- [SETUNA.csproj](file://SETUNA/SETUNA.csproj)
- [Program.cs](file://SETUNA/Program.cs)
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs)
- [AutoStartup.cs](file://SETUNA/Main/Startup/AutoStartup.cs)
- [WindowsAPI.cs](file://SETUNA/Main/Common/WindowsAPI.cs)
- [app.manifest](file://SETUNA/app.manifest)
</cite>

## Table of Contents
1. [System Requirements](#system-requirements)
2. [Installation Procedures](#installation-procedures)
3. [Initial Configuration](#initial-configuration)
4. [Troubleshooting Common Setup Issues](#troubleshooting-common-setup-issues)
5. [Configuration File Reference](#configuration-file-reference)

## System Requirements

The SETUNA application has specific system requirements to ensure proper functionality, particularly for high-DPI display support and compatibility with modern Windows systems.

### Operating System
- **Windows 10 version 14393 or higher** (Windows 10 Anniversary Update or later)
- For systems below this version, the application will attempt to set DPI awareness programmatically
- Older 2.x versions may be required for systems that don't meet these requirements but may experience screen scaling issues

### Framework Requirements
- **.NET Framework 4.7** is required for the 3.x version of SETUNA
- The application will not run without this framework version installed
- .NET Framework 2.0 is required for the older 2.x version (no longer maintained)

### Platform Configuration
- **x86 platform configuration** is required for development builds
- The application targets x86 architecture specifically, as indicated in the project configuration
- This ensures compatibility across different system architectures

**Section sources**
- [README.md](file://README.md#L21-L36)
- [SETUNA.csproj](file://SETUNA/SETUNA.csproj#L12)
- [Program.cs](file://SETUNA/Program.cs#L15-L19)

## Installation Procedures

### Binary Distribution Installation
1. Download the latest release from the [GitHub releases page](https://github.com/tylearymf/SETUNA2/releases)
2. Extract the downloaded ZIP file to a preferred directory
3. Run SETUNA.exe directly from the extracted folder
4. The application will create configuration files on first run
5. No administrative privileges are required for standard operation

### Development Build from Source
1. Clone the repository or download the source code
2. Open SETUNA.sln in Visual Studio
3. **Switch the platform target to x86** in the build configuration (required step)
4. Restore NuGet packages if prompted
5. Build the solution (F5 or Ctrl+Shift+B)
6. The compiled executable will be in the bin\x86\Debug or bin\x86\Release directory
7. Run the application directly from the output directory

The initial configuration file (初始配置.txt) specifically instructs developers to "switch platform to x86, then start the project once with F5," confirming the x86 platform requirement for development.

**Section sources**
- [初始配置.txt](file://初始配置.txt#L1)
- [SETUNA.csproj](file://SETUNA/SETUNA.csproj#L79-L85)
- [README.md](file://README.md#L21-L36)

## Initial Configuration

### High-DPI Support Configuration
The application handles high-DPI displays through a combination of manifest settings and runtime configuration:

1. **Automatic DPI Awareness**: The application checks the Windows version at startup
2. **Runtime DPI Setting**: In Program.cs, if the OS version is below Windows 10 build 14393, it calls `SetProcessDPIAware()` to enable DPI awareness
3. **Manifest Configuration**: The app.manifest file includes DPI awareness settings to ensure proper scaling on high-DPI displays

This multi-layered approach ensures compatibility across different Windows versions and display configurations.

### Auto-Start Configuration
The application supports configuration of auto-start functionality through the settings interface:

1. Navigate to **Options → General → Other**
2. Check the option to enable auto-start
3. The setting modifies the Windows Registry entry at:
   `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run\SETUNA_AutoStartup`

The AutoStartup.cs file implements this functionality by creating or removing a registry entry that points to the application executable.

### Display Settings Configuration
Key display settings can be configured through the application interface:

- **Fullscreen Crosshair Cursor**: Options → Miscellaneous Settings → Fullscreen Crosshair Style
- **Mouse Cursor in Screenshots**: Options → Miscellaneous Settings → Other → Keep mouse in screenshot
- **Magnifier Display**: Options → Miscellaneous Settings → Other → Show magnifier
- **Multi-monitor DPI Support**: For 2.x versions, manually set DPI in Options → Monitor DPI Settings

**Section sources**
- [Program.cs](file://SETUNA/Program.cs#L15-L19)
- [AutoStartup.cs](file://SETUNA/Main/Startup/AutoStartup.cs#L77)
- [WindowsAPI.cs](file://SETUNA/Main/Common/WindowsAPI.cs#L20)
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L44-L52)
- [README.md](file://README.md#L47-L67)

## Troubleshooting Common Setup Issues

### Blurry Captures (Penetration Capture Setting)
If screenshots appear blurry, enable the "penetration capture" feature:

1. Open the application settings
2. Navigate to **Miscellaneous Settings → Screenshot Background**
3. Enable the **Penetration Screenshot** option

This setting, specifically mentioned in the README.md, addresses capture quality issues that may occur on certain display configurations.

### Screen Scaling Problems
For users experiencing screen scaling issues:

1. **Windows Version Check**: Verify you're running Windows 10 build 14393 or higher
   - Go to Settings → System → About → Windows specifications for version details
2. **DPI Awareness**: Ensure the application has proper DPI awareness
   - The application automatically sets this for older Windows versions
   - For Windows 10 Anniversary Update and later, the system handles DPI scaling
3. **Fallback Option**: If scaling issues persist, consider using the 2.x version of the application, though this is no longer maintained

### High-DPI Display Issues
For multi-monitor setups with different DPI settings:

1. **3.x Version**: Supports multiple displays with different DPI settings automatically
2. **2.x Version**: Requires manual DPI configuration in Options → Monitor DPI Settings
3. **Application Behavior**: The application captures screenshots at the native resolution of each monitor

### Common Error Scenarios
- **Missing .NET Framework**: Install .NET Framework 4.7 if the application fails to start
- **Permission Issues**: Run as administrator if registry modifications for auto-start fail
- **Antivirus Interference**: Add SETUNA.exe to antivirus exclusions if startup is blocked
- **Multiple Instances**: The application uses singleton pattern to prevent multiple instances

**Section sources**
- [README.md](file://README.md#L34-L36)
- [Program.cs](file://SETUNA/Program.cs#L15-L19)
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L27-L36)
- [WindowsAPI.cs](file://SETUNA/Main/Common/WindowsAPI.cs#L20)

## Configuration File Reference

### Runtime Configuration Paths
The application stores user configuration in an XML file named "SetunaConfig.xml" located in the application directory. The path is determined by:

```csharp
public static string ConfigFile
{
    get
    {
        var text = Application.StartupPath;
        text = Path.GetFullPath(Path.Combine(text, ""));
        if (!Directory.Exists(text))
        {
            Directory.CreateDirectory(text);
        }
        return Path.Combine(text, "SetunaConfig.xml");
    }
}
```

This ensures the configuration file is always created in the same directory as the executable.

### Build-Time Setup
The initial configuration file (初始配置.txt) contains the essential build instruction:
- Platform must be set to x86 before building
- First run should be executed with F5 (debug mode) to initialize the project

This is critical for development builds to ensure proper compilation and execution.

### Registry Configuration
Auto-start functionality uses the Windows Registry:
- Key location: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- Value name: "SETUNA_AutoStartup"
- Value data: Full path to the application executable

The AutoStartup class handles all registry operations with proper error handling and resource cleanup.

**Section sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L574-L585)
- [AutoStartup.cs](file://SETUNA/Main/Startup/AutoStartup.cs#L77)
- [初始配置.txt](file://初始配置.txt#L1)