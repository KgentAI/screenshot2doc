# User-Defined Styles

<cite>
**Referenced Files in This Document**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs)
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs)
- [CStyle.cs](file://SETUNA/Main/Style/CStyle.cs)
- [CStyleItem.cs](file://SETUNA/Main/StyleItems/CStyleItem.cs)
- [KeyItem.cs](file://SETUNA/Main/KeyItems/KeyItem.cs)
- [COpacityStyleItem.cs](file://SETUNA/Main/StyleItems/COpacityStyleItem.cs)
- [CScaleStyleItem.cs](file://SETUNA/Main/StyleItems/CScaleStyleItem.cs)
- [CMarginStyleItem.cs](file://SETUNA/Main/StyleItems/CMarginStyleItem.cs)
- [CRotateStyleItem.cs](file://SETUNA/Main/StyleItems/CRotateStyleItem.cs)
- [OptionForm.cs](file://SETUNA/Main/Option/OptionForm.cs)
</cite>

## Table of Contents
1. [Introduction](#introduction)
2. [System Architecture](#system-architecture)
3. [Core Components](#core-components)
4. [Styles Collection Management](#styles-collection-management)
5. [Style Creation and Editing](#style-creation-and-editing)
6. [Style Item Operations](#style-item-operations)
7. [Hotkey Assignment System](#hotkey-assignment-system)
8. [Persistence and Configuration](#persistence-and-configuration)
9. [Validation and Error Handling](#validation-and-error-handling)
10. [Advanced Style Examples](#advanced-style-examples)
11. [Troubleshooting Guide](#troubleshooting-guide)
12. [Conclusion](#conclusion)

## Introduction

The Setuna application provides a sophisticated user-defined style management system that allows users to create, modify, and organize custom automation operations. This system enables users to define complex sequences of image manipulation operations with associated keyboard shortcuts, creating powerful productivity workflows for screenshot capture and editing tasks.

The style management system is built around three primary components: the `SetunaOption` class that manages the overall configuration, the `StyleEditForm` that provides the user interface for creating and modifying styles, and the `CStyle` class that represents individual user-defined automation operations.

## System Architecture

The user-defined style management system follows a layered architecture with clear separation of concerns:

```mermaid
graph TB
subgraph "User Interface Layer"
SEF[StyleEditForm]
OF[OptionForm]
end
subgraph "Business Logic Layer"
SO[SetunaOption]
CS[CStyle]
CSI[CStyleItem]
end
subgraph "Data Management Layer"
SC[Styles Collection]
KI[KeyItem System]
PC[Persistence Configuration]
end
subgraph "Operation Layer"
OP[Opacity Operations]
SC_OP[Scale Operations]
MR_OP[Margin Operations]
RT_OP[Rotate Operations]
end
SEF --> SO
OF --> SO
SO --> SC
SO --> KI
CS --> CSI
CSI --> OP
CSI --> SC_OP
CSI --> MR_OP
CSI --> RT_OP
SC --> PC
```

**Diagram sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L14-L1156)
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L11-L293)
- [CStyle.cs](file://SETUNA/Main/Style/CStyle.cs#L10-L277)

## Core Components

### SetunaOption Class

The `SetunaOption` class serves as the central configuration manager for the entire application, including the styles collection. It implements the `ICloneable` interface to support configuration copying and restoration.

```mermaid
classDiagram
class SetunaOption {
+CStyle[] Styles
+bool Difficult
+Keys[] ScrapHotKeyDatas
+bool ScrapHotKeyEnable
+SetunaOptionData Setuna
+ScrapOptionData Scrap
+GetDefaultOption() SetunaOption
+FindStyle(styleId) CStyle
+GetKeyItemBook() KeyItemBook
+Clone() object
+RegistHotKey(handle, keyID) bool
+UnregistHotKey(handle, keyID) void
}
class CStyle {
+string StyleName
+int StyleID
+CStyleItem[] Items
+KeyItem[] KeyItems
+AddStyle(newCi) void
+ClearStyle() void
+RemoveStyle(removeCi) void
+AddKeyItem(newKey) void
+ClearKey() void
+Apply(ref scrap) void
+DeepCopy() CStyle
}
class CStyleItem {
<<abstract>>
+bool IsSetting
+string StateText
+bool IsTerminate
+bool IsInitApply
+StyleItemSetting() void
+Apply(ref scrap, clickpoint) void
+GetName() string
+GetDisplayName() string
+GetDescription() string
+GetIcon() Bitmap
}
SetunaOption --> CStyle : "manages"
CStyle --> CStyleItem : "contains"
```

**Diagram sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L14-L1156)
- [CStyle.cs](file://SETUNA/Main/Style/CStyle.cs#L10-L277)
- [CStyleItem.cs](file://SETUNA/Main/StyleItems/CStyleItem.cs#L8-L101)

**Section sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L14-L1156)
- [CStyle.cs](file://SETUNA/Main/Style/CStyle.cs#L10-L277)

### StyleEditForm Class

The `StyleEditForm` provides the graphical interface for creating and modifying user-defined styles. It integrates with the `SetunaOption` system to manage style collections and handles the binding between UI controls and underlying style objects.

```mermaid
sequenceDiagram
participant User as User
participant SEF as StyleEditForm
participant CS as CStyle
participant KI as KeyItem
participant KB as KeyItemBook
User->>SEF : Open Style Editor
SEF->>CS : Initialize with Target Style
SEF->>SEF : RefreshStyleItemList()
SEF->>SEF : RefreshAllStyleItemList()
SEF->>SEF : RefreshKeyItemList()
User->>SEF : Add Style Item
SEF->>SEF : InsertStyleItem()
SEF->>CS : AddStyle(newItem)
User->>SEF : Assign Hotkey
SEF->>KI : Create KeyItem
SEF->>KB : AddKeyItem(keyItem)
SEF->>CS : AddKeyItem(key)
User->>SEF : Save Changes
SEF->>SEF : WriteValue()
SEF->>CS : Update Style Properties
SEF->>SEF : Close Dialog
```

**Diagram sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L13-L293)

**Section sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L11-L293)

## Styles Collection Management

### Styles Collection Structure

The `Styles` collection in `SetunaOption` is a `List<CStyle>` that stores all user-defined styles. Each style contains multiple operations (`CStyleItem` instances) and associated hotkeys.

### FindStyle Method

The `FindStyle` method provides efficient retrieval of user-defined styles by their unique identifier:

```mermaid
flowchart TD
Start([FindStyle Called]) --> CheckID{styleId == 0?}
CheckID --> |Yes| ReturnNull[Return null]
CheckID --> |No| LoopStyles[Loop Through Styles]
LoopStyles --> CompareID{StyleID == target?}
CompareID --> |Yes| ReturnStyle[Return Style Object]
CompareID --> |No| NextStyle[Next Style]
NextStyle --> MoreStyles{More Styles?}
MoreStyles --> |Yes| LoopStyles
MoreStyles --> |No| ReturnNull
ReturnStyle --> End([End])
ReturnNull --> End
```

**Diagram sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L687-L703)

**Section sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L687-L703)

## Style Creation and Editing

### Creating New Styles

New styles are created through the `StyleEditForm` constructor, which initializes with either a new empty style or an existing style object:

```mermaid
flowchart TD
CreateForm[Create StyleEditForm] --> CheckTrgStyle{trgStyle == null?}
CheckTrgStyle --> |Yes| NewStyle[Create New CStyle]
CheckTrgStyle --> |No| UseExisting[Use Existing Style]
NewStyle --> InitProperties[Initialize Properties]
UseExisting --> LoadProperties[Load Style Properties]
InitProperties --> SetDefaults[Set Default Values]
LoadProperties --> RefreshUI[Refresh UI Controls]
SetDefaults --> RefreshUI
RefreshUI --> Ready[Ready for Editing]
```

**Diagram sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L14-L30)

### Style Property Binding

The `Style` property in `StyleEditForm` provides read-only access to the underlying `CStyle` object, enabling seamless integration between the form and the style data model.

**Section sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L33-L35)

## Style Item Operations

### CStyleItem Architecture

The `CStyleItem` abstract class defines the interface for all style operations. Each style item represents a specific image manipulation operation with configurable parameters.

### Available Style Item Types

The system supports various types of style items:

| Style Item Type | Purpose | Key Operations |
|----------------|---------|----------------|
| `COpacityStyleItem` | Adjust transparency | Absolute/Relative opacity values |
| `CScaleStyleItem` | Resize images | Fixed percentage/Incremental scaling |
| `CMarginStyleItem` | Border styling | Border types, colors, sizes |
| `CRotateStyleItem` | Rotation/flipping | Rotation angles, reflection modes |
| `CMoveStyleItem` | Position adjustment | Movement coordinates |

### Adding Style Items

The `InsertStyleItem` method handles the creation and insertion of new style items:

```mermaid
flowchart TD
SelectItem[listAllStyleItem.SelectedItem] --> CheckSelection{Item Selected?}
CheckSelection --> |No| Exit[Exit Method]
CheckSelection --> |Yes| CloneItem[Clone Selected Item]
CloneItem --> CheckSettings{IsSetting Enabled?}
CheckSettings --> |Yes| ShowSettings[Show Setting Dialog]
CheckSettings --> |No| CheckPosition{Selected Index?}
ShowSettings --> CheckPosition
CheckPosition --> |=-1| AppendToEnd[Add to End]
CheckPosition --> |Other| InsertAfter[Insert After Current]
AppendToEnd --> UpdateUI[Update UI]
InsertAfter --> UpdateUI
UpdateUI --> Complete[Complete]
```

**Diagram sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L147-L164)

**Section sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L147-L164)
- [CStyleItem.cs](file://SETUNA/Main/StyleItems/CStyleItem.cs#L8-L101)

## Hotkey Assignment System

### KeyItem Integration

The hotkey assignment system uses the `KeyItem` class to manage keyboard shortcuts for styles. Each `KeyItem` is associated with a specific style and can be managed through the `KeyItemBook` system.

### Hotkey Validation

The system provides real-time validation of hotkey conflicts and displays user-friendly warnings:

```mermaid
sequenceDiagram
participant User as User
participant HF as HotkeyControl
participant KB as KeyItemBook
participant TI as ToolTip
User->>HF : Press Key Combination
HF->>HF : Validate Key Combination
HF->>KB : FindKeyItem(key)
KB-->>HF : Return KeyItem or null
HF->>TI : Show Conflict Warning
TI-->>User : Display Conflict Message
HF->>HF : Enable/Disable Entry Button
```

**Diagram sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L255-L274)

**Section sources**
- [KeyItem.cs](file://SETUNA/Main/KeyItems/KeyItem.cs#L8-L99)

## Persistence and Configuration

### WriteValue Method

The `WriteValue` method in `StyleEditForm` handles the persistence of style configuration changes:

```mermaid
flowchart TD
Start([WriteValue Called]) --> UpdateName[Update Style Name]
UpdateName --> ClearStyles[Clear Existing Styles]
ClearStyles --> LoopStyles[Loop Through UI Items]
LoopStyles --> AddStyle[Add Style Item to Style]
AddStyle --> MoreStyles{More Items?}
MoreStyles --> |Yes| LoopStyles
MoreStyles --> |No| ClearKeys[Clear Existing Keys]
ClearKeys --> LoopKeys[Loop Through Key Items]
LoopKeys --> FindKey[Find KeyItem in Book]
FindKey --> Deparent{Key Found?}
Deparent --> |Yes| RemoveOld[Remove Old Assignment]
Deparent --> |No| AddKey[Add New Key]
RemoveOld --> AddKey
AddKey --> MoreKeys{More Keys?}
MoreKeys --> |Yes| LoopKeys
MoreKeys --> |No| Complete[Complete]
```

**Diagram sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L125-L144)

### Configuration Storage

Style configurations are persisted through the `SetunaOption` class, which manages the XML serialization of the entire configuration including styles, hotkeys, and application settings.

**Section sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L125-L144)

## Validation and Error Handling

### Style Name Validation

The system enforces basic validation rules for style names:

```mermaid
flowchart TD
CheckName[Check Style Name] --> Empty{Empty String?}
Empty --> |Yes| ShowError[Show Error Message]
Empty --> |No| EnableSave[Enable OK Button]
ShowError --> FocusName[Focus Style Name Field]
EnableSave --> AllowSave[Allow Style Creation]
FocusName --> WaitInput[Wait for Input]
WaitInput --> CheckName
```

**Diagram sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L106-L111)

### Hotkey Conflict Detection

The system automatically detects and warns users about hotkey conflicts:

| Conflict Type | Behavior | User Action Required |
|--------------|----------|---------------------|
| Duplicate Assignment | Warning displayed | Choose different key combination |
| Reserved Keys | Automatic prevention | System prevents assignment |
| Modifier Conflicts | Real-time validation | Immediate feedback |

**Section sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L106-L111)
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L255-L274)

## Advanced Style Examples

### Complex Multi-Operation Style

A sophisticated style combining multiple operations demonstrates the power of the system:

```mermaid
graph LR
subgraph "Complex Style Example"
S1[Scale Operation<br/>50% Fixed]
S2[Opacity Operation<br/>Absolute 80%]
S3[Margin Operation<br/>Single Color Border]
S4[Rotate Operation<br/>90° Rotation]
end
S1 --> S2
S2 --> S3
S3 --> S4
subgraph "Hotkey Assignment"
HK[Ctrl+Alt+S]
end
HK -.-> S1
```

**Diagram sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L509-L522)

### Basic Automation Style

A simple style for basic automation showcases fundamental concepts:

```mermaid
graph LR
subgraph "Basic Automation Style"
MS[Margin Operation<br/>Solid Border]
OP[Opacity Operation<br/>95% Absolute]
end
MS --> OP
subgraph "Hotkey Assignment"
BH[Ctrl+B]
end
BH -.-> MS
```

**Diagram sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L509-L522)

**Section sources**
- [SetunaOption.cs](file://SETUNA/Main/Option/SetunaOption.cs#L509-L522)

## Troubleshooting Guide

### Common Issues and Solutions

| Issue | Symptoms | Solution |
|-------|----------|----------|
| Style Not Saving | Changes lost on restart | Verify XML file permissions |
| Hotkey Conflicts | Keys not responding | Check for conflicting assignments |
| Style Items Missing | Operations not applied | Re-add missing style items |
| Memory Issues | Slow performance | Reduce number of style items |

### Debugging Style Problems

1. **Validate Style Names**: Ensure all styles have non-empty names
2. **Check Hotkey Assignments**: Verify no duplicate key combinations
3. **Review Style Item Order**: Confirm logical operation sequence
4. **Test Individual Operations**: Isolate problematic style items

**Section sources**
- [StyleEditForm.cs](file://SETUNA/Main/Option/StyleEditForm.cs#L106-L111)

## Conclusion

The user-defined style management system in Setuna provides a comprehensive framework for creating sophisticated automation workflows. Through the integration of the `SetunaOption`, `StyleEditForm`, and `CStyle` classes, users can build complex sequences of image manipulation operations with intuitive hotkey assignments.

The system's modular architecture, robust validation mechanisms, and flexible configuration options make it suitable for both casual users and power users who need advanced automation capabilities. The persistent storage system ensures that user configurations are reliably maintained across application sessions.

Future enhancements could include style templates, batch operations, and more sophisticated conflict resolution mechanisms to further improve the user experience while maintaining the system's extensible architecture.