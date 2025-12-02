# Introduction

<cite>
**Referenced Files in This Document**
- [README.md](file://README.md)
- [SETUNA.csproj](file://SETUNA/SETUNA.csproj)
- [Program.cs](file://SETUNA/Program.cs)
- [Mainform.cs](file://SETUNA/Mainform.cs)
- [ScrapBase.cs](file://SETUNA/Main/ScrapBase.cs)
- [ScrapBook.cs](file://SETUNA/Main/ScrapBook.cs)
- [CaptureForm.cs](file://SETUNA/Main/CaptureForm.cs)
- [CacheManager.cs](file://SETUNA/Main/Cache/CacheManager.cs)
- [HotkeyControl.cs](file://SETUNA/Main/HotkeyControl.cs)
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs)
- [CStyle.cs](file://SETUNA/Main/Style/CStyle.cs)
- [AssemblyInfo.cs](file://SETUNA/Properties/AssemblyInfo.cs)
</cite>

## Table of Contents
1. [Project Overview](#project-overview)
2. [Purpose and Core Functionality](#purpose-and-core-functionality)
3. [Key Features and Capabilities](#key-features-and-capabilities)
4. [Target Audience](#target-audience)
5. [Project Evolution and History](#project-evolution-and-history)
6. [Technical Requirements](#technical-requirements)
7. [Practical Use Cases](#practical-use-cases)
8. [Architecture Foundation](#architecture-foundation)

## Project Overview

SETUNA is a sophisticated, lightweight screenshot capture application designed specifically for Windows desktop environments. Built with .NET Framework 4.7, this tool serves as a comprehensive solution for users who require persistent, high-quality image captures with advanced management capabilities. The application operates as both a standalone executable and a system-tray resident program, providing seamless integration with Windows desktop workflows.

The application's primary mission is to offer power users and professionals with a reliable, efficient tool for capturing, managing, and persistently storing screen images. Unlike traditional screenshot tools that simply capture and discard images, SETUNA maintains captured screenshots as persistent "scraps" that remain accessible across system restarts and can be manipulated with various styling and editing options.

**Section sources**
- [README.md](file://README.md#L1-L89)
- [SETUNA.csproj](file://SETUNA/SETUNA.csproj#L1-L50)

## Purpose and Core Functionality

SETUNA addresses the fundamental need for persistent screenshot management in professional and creative workflows. The application transforms the traditional screenshot process by maintaining captured images as interactive "scraps" that can be resized, repositioned, styled, and accessed long-term. This persistent approach eliminates the need for constant file management and provides immediate access to previously captured content.

The core functionality revolves around three primary operations:
- **Screenshot Capture**: High-DPI-aware capture with multi-monitor support
- **Persistent Storage**: Automatic caching and restoration of captured images
- **Interactive Management**: Styling, editing, and organizational capabilities

This approach fundamentally differs from conventional screenshot tools by treating captured images as permanent workspace elements rather than temporary captures, enabling users to build comprehensive visual libraries and reference materials.

**Section sources**
- [Mainform.cs](file://SETUNA/Mainform.cs#L1-L100)
- [ScrapBook.cs](file://SETUNA/Main/ScrapBook.cs#L1-L100)

## Key Features and Capabilities

### Multi-Monitor and High-DPI Support
SETUNA excels in modern display environments with native support for multiple monitors with varying DPI settings. The application automatically detects and adapts to different display configurations, ensuring accurate capture regardless of whether users work with high-resolution displays, mixed DPI setups, or multiple monitors.

### Persistent Scrap Restoration
One of SETUNA's most distinctive features is its ability to maintain screenshot persistence across system restarts. Through an integrated caching system, the application stores captured images along with their positioning, styling, and organizational metadata. Upon restart, all previously captured scraps are automatically restored to their original positions and configurations.

### System Tray Integration
The application provides seamless system tray integration, allowing users to minimize the interface while maintaining access to all captured content. The system tray icon serves as the primary activation point, enabling users to quickly bring up all screenshots with a single click and manage them through contextual menus.

### Global Hotkey System
SETUNA implements a comprehensive global hotkey system that allows users to capture screenshots without interrupting their workflow. The application registers system-wide keyboard shortcuts that can be customized according to user preferences, enabling rapid capture operations during presentations, documentation creation, or design reviews.

### Clipboard Enhancement
Beyond basic screenshot functionality, SETUNA enhances the Windows clipboard system by providing intelligent image pasting capabilities. Users can easily paste images from the clipboard directly into existing scraps, creating layered compositions and reference materials with minimal effort.

### Advanced Image Formats Support
The application supports an extensive range of image formats including JPEG, PNG, PSD, GIF, BMP, ICO, TIFF, WEBP, SVG, TGA, and others. This comprehensive format support ensures compatibility with various design tools and documentation systems.

### Drag-and-Drop Functionality
SETUNA incorporates intuitive drag-and-drop capabilities, allowing users to create scraps from website images by dragging them directly onto the application interface. This feature simplifies the process of incorporating online content into personal documentation and design projects.

**Section sources**
- [README.md](file://README.md#L40-L76)
- [HotkeyControl.cs](file://SETUNA/Main/HotkeyControl.cs#L1-L82)
- [CacheManager.cs](file://SETUNA/Main/Cache/CacheManager.cs#L1-L132)

## Target Audience

SETUNA is specifically designed for power users and professionals who require sophisticated screenshot management capabilities. The primary target audience includes:

### Software Developers and Engineers
Developers benefit from SETUNA's persistent screenshot storage and clipboard integration, enabling them to maintain comprehensive documentation of code reviews, bug reports, and development workflows. The ability to create layered compositions helps in illustrating complex system interactions and design decisions.

### Design Professionals
Graphic designers, UI/UX specialists, and visual artists utilize SETUNA for building visual reference libraries, maintaining design inspiration boards, and creating annotated design reviews. The multi-format support and drag-and-drop functionality streamline the incorporation of external design resources.

### Technical Writers and Documentation Specialists
Professionals creating technical documentation leverage SETUNA's persistent storage and organizational features to maintain comprehensive visual archives. The clipboard enhancement capabilities facilitate the inclusion of screenshots in documentation workflows.

### Quality Assurance Teams
QA testers use SETUNA for maintaining visual records of testing processes, bug reporting, and regression testing. The persistent nature of scraps enables teams to track changes over time and maintain historical documentation of testing phases.

### Educators and Presenters
Academic instructors and presentation specialists benefit from SETUNA's global hotkey system and clipboard integration for creating educational materials and presentation aids. The multi-monitor support accommodates classroom and conference presentation scenarios.

**Section sources**
- [README.md](file://README.md#L1-L30)
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L31-L1099)

## Project Evolution and History

SETUNA represents the evolution of the original Setuna2 project, which was officially discontinued by its original developers. The current maintainer identified limitations in the original Setuna2, particularly concerning high-DPI display support, and initiated the development of SETUNA 3.x as a modernized, maintained alternative.

### Legacy Context
The original Setuna2 project served as the foundation for this modern iteration, establishing core concepts of persistent screenshot management and multi-format support. However, the project had reached a maintenance hiatus, leaving users with compatibility issues on modern Windows systems and high-DPI displays.

### Modernization Efforts
SETUNA 3.x introduces significant improvements over its predecessor:
- **Updated Technology Stack**: Migration to .NET Framework 4.7 for improved compatibility and performance
- **Enhanced DPI Support**: Native high-DPI awareness for modern display environments
- **Modern Architecture**: Refined codebase with improved maintainability and extensibility
- **Expanded Feature Set**: Additional styling options, clipboard enhancements, and drag-and-drop functionality

### Maintenance Status
As of the current release, SETUNA operates under active maintenance with ongoing development efforts. The project maintains backward compatibility with Setuna2 configurations while introducing new features and improvements based on user feedback and technological advancements.

**Section sources**
- [README.md](file://README.md#L1-L10)
- [AssemblyInfo.cs](file://SETUNA/Properties/AssemblyInfo.cs#L1-L16)

## Technical Requirements

### System Requirements
SETUNA requires specific system configurations to ensure optimal functionality:

#### Operating System Compatibility
- **Windows 10 Build 14393 or later**: The application requires Windows 10 Anniversary Update or newer to leverage modern Windows APIs and high-DPI support
- **Windows 11**: Fully compatible with Windows 11 operating system environments

#### Framework Dependencies
- **.NET Framework 4.7**: Essential runtime requirement for application functionality and modern Windows integration
- **.NET Framework 2.0**: Legacy compatibility for users maintaining older Setuna2 installations

### Hardware Considerations
While SETUNA is designed as a lightweight application, certain hardware configurations enhance the user experience:
- **Multi-Monitor Setup**: Recommended for leveraging full multi-DPI support capabilities
- **High-DPI Displays**: Optimized rendering ensures crisp visuals on modern high-resolution screens
- **Sufficient RAM**: Adequate memory allocation for smooth operation with multiple open scraps

### Network Requirements
Certain features require network connectivity:
- **Drag-and-Drop from Websites**: Internet connection for downloading remote images
- **Cloud Integration**: Optional connectivity for cloud-based screenshot sharing (when enabled)

**Section sources**
- [README.md](file://README.md#L20-L37)
- [Program.cs](file://SETUNA/Program.cs#L1-L34)
- [SETUNA.csproj](file://SETUNA/SETUNA.csproj#L12-L15)

## Practical Use Cases

### Documentation and Knowledge Management
SETUNA excels in documentation workflows by providing persistent storage of visual references. Users can capture interface elements, code snippets, and design assets that remain accessible throughout project lifecycles. The clipboard integration streamlines the inclusion of screenshots in documentation systems, reducing manual file management overhead.

### Design Review and Feedback
Creative professionals utilize SETUNA for collaborative design review processes. Screenshots can be layered with annotations, comments, and reference materials, creating comprehensive visual feedback documents. The persistent nature of scraps enables teams to maintain historical design iterations and track evolution over time.

### Bug Reporting and Issue Tracking
Software development teams employ SETUNA for systematic bug reporting. Captured screenshots serve as visual evidence of issues, with the ability to annotate and organize them systematically. The clipboard enhancement facilitates the inclusion of visual bug reports in issue tracking systems.

### Educational Content Creation
Educators and instructional designers use SETUNA for creating comprehensive learning materials. The drag-and-drop functionality simplifies the incorporation of external visual content, while the persistent storage ensures organized access to teaching resources across multiple sessions.

### Presentation and Demonstration Support
Professionals preparing presentations benefit from SETUNA's global hotkey system and clipboard integration. Rapid screenshot capture during demonstrations ensures immediate access to visual content without interrupting presentation flow.

**Section sources**
- [README.md](file://README.md#L1-L30)
- [ScrapBase.cs](file://SETUNA/Main/ScrapBase.cs#L1-L200)

## Architecture Foundation

SETUNA's architecture is built upon several foundational principles that enable its sophisticated functionality:

### Modular Design Approach
The application employs a modular architecture with distinct separation of concerns:
- **Capture Module**: Handles screenshot acquisition and initial processing
- **Storage Module**: Manages persistent caching and restoration of scraps
- **Interface Module**: Provides user interaction and visual management
- **Integration Module**: Handles system integration and external functionality

### Event-Driven Communication
SETUNA utilizes an event-driven architecture that enables loose coupling between components. This design facilitates extensibility and maintainability while ensuring responsive user interactions across all functional areas.

### Plugin Architecture
The application supports extensible functionality through a plugin-like architecture, particularly evident in the WebP format support and potential for additional format handlers. This approach enables future enhancements without requiring core application modifications.

### Memory Management Strategy
SETUNA implements sophisticated memory management to handle multiple large image files efficiently. The caching system balances performance with resource utilization, ensuring smooth operation even with numerous open scraps.

**Section sources**
- [Mainform.cs](file://SETUNA/Mainform.cs#L1-L100)
- [ScrapBook.cs](file://SETUNA/Main/ScrapBook.cs#L1-L219)
- [CStyle.cs](file://SETUNA/Main/Style/CStyle.cs#L1-L200)