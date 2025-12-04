# AI Screenshot Summary - Implementation Design

## Feature Context

This design provides implementation specifications for adding AI-powered screenshot analysis capabilities to the SETUNA screenshot tool. The feature enables users to select multiple captured screenshots and generate structured markdown summaries using multimodal AI models.

## Implementation Scope

### Core Components to Create

| Component | Type | Location | Purpose |
|-----------|------|----------|---------|
| AISummaryForm | Windows Form | SETUNA/Main/AI/AISummaryForm.cs | Main UI for AI summary feature |
| IMultimodalService | Interface | SETUNA/Main/AI/Services/IMultimodalService.cs | Service abstraction layer |
| MiniCPMService | Class | SETUNA/Main/AI/Services/MiniCPMService.cs | Local model implementation |
| QwenVLService | Class | SETUNA/Main/AI/Services/QwenVLService.cs | Cloud API implementation |
| MultimodalServiceFactory | Class | SETUNA/Main/AI/Services/MultimodalServiceFactory.cs | Service instantiation |
| AISummaryConfig | Class | SETUNA/Main/Option/AISummaryConfig.cs | Configuration model |
| MarkdownRenderer | Class | SETUNA/Main/AI/UI/MarkdownRenderer.cs | RichTextBox-based markdown display |
| MarkdownExporter | Class | SETUNA/Main/AI/Export/MarkdownExporter.cs | File export functionality |
| ExcelExporter | Class | SETUNA/Main/AI/Export/ExcelExporter.cs | Excel table export |

### Integration Points

**Mainform Context Menu Extension**

Add new menu item after "Scrap List" in the system tray menu construction:

- Menu item text: "AI Summary..."
- Enabled condition: scrapBook contains at least one scrap with valid image
- Click handler: Opens AISummaryForm as modal dialog
- Integration location: Mainform.SetSubMenu method
- No modifications to existing menu items

**SetunaOption Configuration Extension**

Add AISummary property to SetunaOption class:

- Property type: AISummaryConfig
- XML element name: "AISummary"
- Initialization: Create with default values in constructor
- Type registration: Add typeof(AISummaryConfig) to GetAllType method
- Serialization: Automatic via existing XmlSerializer infrastructure

**NuGet Package Dependencies**

| Package | Version | Purpose | Build Integration |
|---------|---------|---------|-------------------|
| EPPlus | 5.8.x or later | Excel file generation | Embed with ILMerge |
| Newtonsoft.Json | 13.0.x | JSON serialization for API calls | Already present in project |
| System.Net.Http | 4.3.x | HTTP client for AI service calls | Framework built-in |

## Data Models

### AISummaryConfig Class

Configuration data model for AI summary feature:

| Property | Type | Default Value | XML Element | Validation |
|----------|------|---------------|-------------|------------|
| Enabled | bool | false | Enabled | None |
| Engine | string | "minicpm-v4.5" | Engine | Enum: "minicpm-v4.5", "qwen3-vl-flash" |
| LocalEndpoint | string | "http://localhost:8080" | LocalEndpoint | Valid URI format |
| ApiKey | string | "" | ApiKey | Min 8 chars when cloud enabled |
| MaxImages | int | 5 | MaxImages | Range: 1-10 |
| PromptTemplate | string | (default) | PromptTemplate | Max 2000 chars |
| UseWebView2 | bool | false | UseWebView2 | None |
| TimeoutSeconds | int | 30 | TimeoutSeconds | Range: 10-120 |

Default prompt template:

```
Analyze the provided screenshots and generate a comprehensive summary in markdown format. Include: 1) Overview section describing the main content, 2) Detailed findings organized with headings and bullet points, 3) Tables for any structured data observed, 4) Key observations section. Use clear hierarchical structure with H1-H3 headings.
```

Constructor initializes all properties with defaults. Implements ICloneable for option dialog pattern consistency.

### MultimodalRequest Model

Request data structure for AI service calls:

| Field | Type | Purpose |
|-------|------|---------|
| Images | List&lt;Image&gt; | Screenshot bitmap collection |
| Prompt | string | Analysis instruction |
| MaxTokens | int | Response length limit (default: 2000) |
| Temperature | float | Randomness parameter (default: 0.7) |

### MultimodalResponse Model

Response data structure from AI services:

| Field | Type | Purpose |
|-------|------|---------|
| MarkdownContent | string | Generated summary text |
| Success | bool | Operation status |
| ErrorMessage | string | Failure description (null when Success=true) |
| ProcessingTimeMs | long | Latency measurement |

## Service Architecture

### IMultimodalService Interface

Contract definition for all AI service implementations:

**Methods**

| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| AnalyzeImagesAsync | Task&lt;MultimodalResponse&gt; | MultimodalRequest request | Executes AI analysis |
| IsAvailableAsync | Task&lt;bool&gt; | None | Health check for service |

**Properties**

| Property | Type | Purpose |
|----------|------|---------|
| ModelName | string | Identifies underlying model |
| MaxImageCount | int | Batch size limitation |

### MiniCPMService Implementation

Local model service for offline inference:

**Configuration**

- Endpoint URL: From AISummaryConfig.LocalEndpoint
- Communication protocol: HTTP POST to /analyze
- Image encoding: Base64 data URIs (data:image/jpeg;base64,...)
- Timeout: From AISummaryConfig.TimeoutSeconds
- Request format: JSON with "images" array and "prompt" string

**Image Preprocessing**

Before sending images to local endpoint:

1. Check image dimensions exceed 1920x1080
2. If exceeds, resize proportionally to fit within bounds
3. Encode as JPEG with 85% quality
4. Convert to base64 string
5. Wrap in data URI format
6. Dispose temporary bitmap

**Error Handling**

| Error Type | Detection | Recovery Action |
|-----------|-----------|-----------------|
| Connection refused | HttpRequestException | Return error response with "Local endpoint unreachable" |
| Timeout | TaskCanceledException | Return error response with "Request timeout" |
| Invalid JSON response | JsonException | Return error response with "Invalid server response" |
| HTTP error codes | StatusCode check | Return error response with status code description |

**IsAvailableAsync Implementation**

Send HTTP HEAD request to endpoint root. Return true if receives 200 or 404 status (server responding), false for connection errors.

### QwenVLService Implementation

Cloud API service for DashScope:

**Configuration**

- Base URL: https://dashscope.aliyuncs.com/compatible-mode/v1
- Model identifier: "qwen-vl-flash"
- Authentication: Bearer token from AISummaryConfig.ApiKey
- Request format: OpenAI-compatible vision chat completion

**Request Construction**

Chat completion format:

- messages array with single user message
- content array with multiple elements:
  - One text element with prompt
  - Multiple image_url elements (one per screenshot)
- max_tokens parameter from request
- temperature parameter from request

**Image Preprocessing**

Same as MiniCPMService (resize and JPEG encoding).

**Retry Logic**

Implement exponential backoff for transient failures:

1. First attempt: immediate
2. Second attempt: 2 second delay
3. Third attempt: 4 second delay
4. After three attempts, return error response

Retry on HTTP 429 (rate limit) and 5xx server errors. Do not retry on 4xx client errors except 429.

**IsAvailableAsync Implementation**

Check ApiKey property is not empty and length >= 8 characters. Optionally send test request to /models endpoint.

### MultimodalServiceFactory

Factory for service instantiation based on configuration:

**GetService Method**

Input: AISummaryConfig configuration
Output: IMultimodalService instance or throws ConfigurationException

Decision logic:

1. Read configuration.Engine property
2. If "minicpm-v4.5":
   - Instantiate MiniCPMService with LocalEndpoint
   - Call IsAvailableAsync to validate
   - If unavailable, throw exception with message
3. If "qwen3-vl-flash":
   - Validate ApiKey not empty
   - Instantiate QwenVLService with ApiKey
   - Return instance
4. If unrecognized engine, throw exception

**Exception Types**

Define custom exception: AIServiceConfigurationException with descriptive messages for user guidance.

## User Interface Design

### AISummaryForm Structure

Main form for AI summary feature:

**Inheritance**

- Base class: BaseForm
- Implements: IDisposable explicitly for proper cleanup

**Form Properties**

| Property | Value | Rationale |
|----------|-------|-----------|
| FormBorderStyle | Sizable | Allow user to resize window |
| StartPosition | CenterParent | Center on Mainform |
| MinimumSize | 800x600 | Ensure usability |
| Size | 1200x700 | Comfortable default |
| ShowInTaskbar | false | Modal dialog behavior |
| MaximizeBox | true | Allow fullscreen |
| MinimizeBox | false | Modal dialog pattern |
| Text | "AI Screenshot Summary" | Window title |

**Layout Structure**

SplitContainer with two panels:

- Orientation: Vertical
- SplitterDistance: 30% (left panel)
- FixedPanel: Panel1 (left panel fixed width)
- IsSplitterFixed: false (allow user adjustment)

Left Panel (Screenshot Selection):
- FlowLayoutPanel
- FlowDirection: TopDown
- AutoScroll: true
- BackColor: SystemColors.Control
- Padding: 10 pixels

Right Panel (Summary Display):
- Container with:
  - Top: Control bar (TableLayoutPanel, Dock.Top)
  - Center: Markdown display (RichTextBox, Dock.Fill)
  - Bottom: Status bar (StatusStrip, Dock.Bottom)

### Screenshot Selection Panel

**Thumbnail Generation**

For each screenshot from ScrapBook:

1. Create PictureBox control
2. Set SizeMode to Zoom
3. Set Size to 150x150 pixels
4. Load ScrapBase.Image property
5. Generate thumbnail using Image.GetThumbnailImage
6. Add badge overlay with sequence number

**Badge Overlay Drawing**

Draw badge on PictureBox.Paint event:

- Position: Bottom-right corner (X: Width-30, Y: Height-30)
- Shape: Circle with 25 pixel diameter
- Background: White with 85% opacity
- Border: 2 pixel gray stroke
- Text: Sequence number (1, 2, 3...)
- Font: Bold, 12pt
- Text color: Black

**Interaction Events**

| User Action | Event Handler | Response |
|-------------|---------------|----------|
| Single click | PictureBox.Click | Highlight thumbnail with blue border, scroll markdown to related section |
| Double click | PictureBox.DoubleClick | Open full-size image preview in new form |
| Right click | PictureBox.MouseClick | Show context menu (Remove, View Full Size, Copy Path) |
| Hover | PictureBox.MouseEnter | Show tooltip with DateTime and dimensions |

**Data Binding**

Retrieve screenshots on form load:

1. Access Mainform.Instance.scrapBook
2. Enumerate with foreach loop
3. Check ScrapBase.Image is not null
4. Sort by ScrapBase.DateTime descending
5. Take first MaxImages count
6. Store in List&lt;ScrapBase&gt; field
7. Generate thumbnails for each

### Control Bar Components

TableLayoutPanel with 1 row, 6 columns:

| Column | Control Type | Properties | Function |
|--------|--------------|------------|----------|
| 0 | Label | Text: "Model:" | Label for dropdown |
| 1 | ComboBox | DropDownStyle: DropDownList | Engine selection (MiniCPM / Qwen) |
| 2 | Button | Text: "Analyze" | Trigger AI analysis |
| 3 | Button | Text: "Export Markdown" | Save as .md file |
| 4 | Button | Text: "Export Excel" | Extract tables to .xlsx |
| 5 | ProgressBar | Style: Marquee | Processing indicator |

**Engine ComboBox**

Populate with two items:

- Display: "MiniCPM-V-4.5 (Local)", Value: "minicpm-v4.5"
- Display: "Qwen3-VL-Flash (Cloud)", Value: "qwen3-vl-flash"

Set SelectedValue to configuration.Engine on form load.

**Analyze Button Click Handler**

Async method that:

1. Validate at least one screenshot selected
2. Disable all controls in control bar
3. Show progress bar (Marquee style)
4. Update status label: "Analyzing screenshots..."
5. Create MultimodalRequest with images and prompt
6. Call serviceFactory.GetService(config)
7. Call service.AnalyzeImagesAsync(request) with await
8. On success: Update markdown display with response
9. On failure: Show error notification bar
10. Hide progress bar
11. Re-enable controls
12. Update status label with completion message

**Export Button Handlers**

Export Markdown:
- Open SaveFileDialog with .md filter
- Default filename: "Summary_yyyyMMdd_HHmmss.md"
- Write markdown content with UTF-8 encoding
- Add metadata header (timestamp, model, image count)
- Show success message

Export Excel:
- Parse markdown for table blocks
- If no tables found, show warning message
- Open SaveFileDialog with .xlsx filter
- Default filename: "Summary_Tables_yyyyMMdd_HHmmss.xlsx"
- Call ExcelExporter.Export method
- Show success message with file path

### Markdown Display Component

**MarkdownRenderer Class**

Responsibilities:
- Parse markdown syntax
- Apply formatting to RichTextBox
- Handle user selection for copy operations

**Rendering Strategy**

Parse markdown line by line:

| Syntax | Detection Pattern | RichTextBox Formatting |
|--------|-------------------|------------------------|
| H1 | Starts with "# " | Font: Bold, Size: 16pt, Color: Navy |
| H2 | Starts with "## " | Font: Bold, Size: 14pt, Color: DarkSlateGray |
| H3 | Starts with "### " | Font: Bold, Size: 12pt, Color: DarkSlateGray |
| Bold | Contains \*\*text\*\* | Font: Bold (preserve size) |
| List item | Starts with "- " or "* " | Indent 20 pixels, prepend bullet character |
| Table row | Contains " \| " | Monospace font (Consolas 10pt), preserve spacing |
| Code block | Between \`\`\` markers | Monospace font, gray background |
| Regular text | Default | Font: Regular, Size: 10pt |

**Implementation Approach**

Use RichTextBox methods:

- SelectionStart, SelectionLength for positioning
- SelectionFont for font changes
- SelectionColor for color changes
- SelectionIndent for list indentation
- SelectionBackColor for code block background

Iterate through lines, detect pattern, apply formatting, append text, move to next line.

**Fallback for Complex Markdown**

If markdown contains images, embedded HTML, or complex nested structures:
- Display raw markdown text
- Add note at top: "Complex formatting detected. Exported markdown file will preserve full content."

### Status Bar

StatusStrip with labels:

- Status text label (Spring: true, fills available space)
- Model name label (fixed width)
- Processing time label (fixed width)

Update labels after analysis completes with model name and latency.

### Error Notification Bar

Panel component:

- Dock: Top (above markdown display)
- Height: 40 pixels
- Visible: false (show only on error)
- BackColor: LightCoral (red) or LightGoldenrodYellow (warning)
- Contains: Label for message + Retry Button + Close Button

Display for errors:
- Configuration errors: Red background, show "Configure" button
- Service errors: Yellow background, show "Retry" button
- Timeout errors: Yellow background, show "Retry" button

Auto-dismiss after 10 seconds for non-critical warnings.

## Data Access

### Screenshot Retrieval

Access pattern for reading screenshots from ScrapBook:

**Method Signature**

```
private List<ScrapBase> GetRecentScreenshots(int maxCount)
```

**Implementation Steps**

1. Create empty List&lt;ScrapBase&gt;
2. Access Mainform.Instance.scrapBook
3. Check scrapBook is not null
4. Use foreach to enumerate scrapBook (implements IEnumerable&lt;ScrapBase&gt;)
5. For each ScrapBase:
   - Check Image property is not null
   - Add to list
6. Sort list by DateTime property descending (LINQ OrderByDescending)
7. Take first maxCount items (LINQ Take)
8. Return list

**Image Access**

For each ScrapBase in list:
- Read Image property directly (returns System.Drawing.Image)
- Do not modify original image
- Create copy if modifications needed
- Dispose copies after use

**Memory Management**

To prevent memory leaks:

1. Maintain references only while form is open
2. Implement IDisposable on form
3. Clear image list in Dispose method
4. Do not cache large images in fields
5. Dispose temporary bitmaps immediately after encoding
6. Use using statements for Image objects

### Configuration Access

**Reading Configuration**

Access from Mainform:

```
var config = Mainform.Instance.optSetuna.AISummary;
```

If AISummary is null (first run), initialize with defaults:

```
if (config == null)
{
    config = new AISummaryConfig();
    Mainform.Instance.optSetuna.AISummary = config;
}
```

**Saving Configuration**

Configuration persists automatically when user closes OptionForm with OK result. No manual save required in AISummaryForm.

If configuration changes within AISummaryForm (engine selection):
- Update Mainform.Instance.optSetuna.AISummary properties
- Call Mainform.Instance.SaveOption() to persist immediately

## Export Functionality

### MarkdownExporter Implementation

**Export Method Signature**

```
public static bool ExportToFile(string markdownContent, string filePath, AISummaryMetadata metadata)
```

**Metadata Structure**

| Field | Type | Source |
|-------|------|--------|
| GeneratedAt | DateTime | DateTime.Now |
| ModelName | string | From service response |
| ImageCount | int | Number of screenshots analyzed |
| ProcessingTime | TimeSpan | From response latency |

**File Format**

Structure of exported markdown file:

```
---
Generated: {GeneratedAt:yyyy-MM-dd HH:mm:ss}
Model: {ModelName}
Images: {ImageCount}
Processing Time: {ProcessingTime}
---

{markdownContent}
```

**Encoding**

- Write with UTF-8 encoding
- No BOM (Byte Order Mark)
- Use StreamWriter with explicit Encoding.UTF8
- Ensure newlines are consistent (Environment.NewLine)

**Error Handling**

Try-catch for:
- IOException: Show message "Unable to write file. Check permissions."
- UnauthorizedAccessException: Show message "Access denied. Choose different location."
- Return false on error, true on success

### ExcelExporter Implementation

**Table Detection**

Parse markdown to find table blocks:

1. Scan for lines containing pipe characters ("|")
2. Identify consecutive lines forming table
3. First line: Header row
4. Second line: Separator row (contains dashes)
5. Remaining lines: Data rows
6. Stop at empty line or non-table content

**Table Parsing**

For each detected table:

1. Split header row by pipe character
2. Trim whitespace from each column
3. Store as column names
4. Split each data row by pipe character
5. Store as string array
6. Validate column count matches header

**Excel Generation with EPPlus**

Using EPPlus library:

1. Create ExcelPackage object
2. For each table:
   - Add new ExcelWorksheet
   - Set worksheet name: "Table 1", "Table 2", etc.
   - Write header row to row 1
   - Apply formatting: Bold, gray fill (Color.LightGray)
   - Write data rows starting at row 2
   - Apply borders to all cells
   - Auto-fit column widths
   - Freeze top row
3. Save package to file path
4. Dispose package

**EPPlus License Compliance**

EPPlus 5.x requires commercial license for commercial use. For this implementation:
- Use EPPlus 5.x with NonCommercial license context
- Set license context in code: ExcelPackage.LicenseContext = LicenseContext.NonCommercial
- Alternative: Use EPPlus 4.5.x (LGPL license) if commercial use required

**Error Handling**

| Error | Message | Recovery |
|-------|---------|----------|
| No tables found | "No tables detected in summary." | Disable Excel export button |
| Parse error | "Unable to parse table structure." | Skip malformed tables |
| File write error | "Unable to create Excel file." | Show error dialog |

## Integration Implementation

### Mainform Menu Extension

**Location in Code**

Mainform.SetSubMenu method builds context menu dynamically. Add AI Summary menu item after scrap list style.

**Implementation Steps**

1. Locate SetSubMenu method in Mainform.cs
2. After scrap list menu item addition, insert:
   - Create new CStyle instance (or use direct ToolStripMenuItem)
   - Set Text: "AI Summary..."
   - Set Click handler: miAISummary_Click
   - Set Enabled: scrapBook.Count() > 0
   - Add to contextMenu.Items
   - Add separator after menu item

**Menu Item Click Handler**

```
private void miAISummary_Click(object sender, EventArgs e)
{
    if (IsCapture || IsOption)
    {
        return;
    }
    
    try
    {
        var summaryForm = new AISummaryForm();
        summaryForm.StartPosition = FormStartPosition.CenterParent;
        summaryForm.ShowDialog(this);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Unable to open AI Summary: " + ex.Message,
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
```

### SetunaOption Configuration Extension

**AISummary Property Addition**

Add to SetunaOption class:

```
public AISummaryConfig AISummary { get; set; }
```

Initialize in constructor:

```
AISummary = new AISummaryConfig();
```

**Type Registration**

Update GetAllType method:

Locate ArrayList initialization, add:
```
arrayList.Add(typeof(AISummaryConfig));
```

This ensures XmlSerializer recognizes the type during serialization/deserialization.

**Configuration File Compatibility**

Existing configuration files without AISummary element:
- XmlSerializer will leave property null
- Handle null case in AISummaryForm initialization
- Create default instance when null detected
- No migration required

### OptionForm Integration (Optional Future Enhancement)

For initial implementation, configuration can be managed through:
- Configuration file direct editing
- In-form settings panel within AISummaryForm

Future enhancement: Add "AI" tab to OptionForm with:
- Enable/disable toggle
- Engine selection radio buttons
- Local endpoint text box
- API key masked text box
- Max images numeric up/down
- Prompt template multi-line text box

## Async/Await Architecture

### UI Thread Management

All AI service calls must use async/await pattern to prevent UI freezing:

**Button Click Handler Pattern**

```
private async void btnAnalyze_Click(object sender, EventArgs e)
{
    // Disable UI
    SetControlsEnabled(false);
    progressBar.Visible = true;
    
    try
    {
        // Async operation
        var response = await PerformAnalysisAsync();
        
        // Update UI on UI thread
        UpdateMarkdownDisplay(response);
    }
    catch (Exception ex)
    {
        ShowError(ex.Message);
    }
    finally
    {
        // Re-enable UI
        SetControlsEnabled(true);
        progressBar.Visible = false;
    }
}
```

**Service Call Pattern**

```
private async Task<MultimodalResponse> PerformAnalysisAsync()
{
    // Runs on background thread
    var request = CreateRequest();
    var service = serviceFactory.GetService(config);
    return await service.AnalyzeImagesAsync(request);
}
```

**ConfigureAwait Usage**

For library code (services, exporters), use ConfigureAwait(false):

```
await httpClient.PostAsync(endpoint, content).ConfigureAwait(false);
```

For UI code (form event handlers), omit ConfigureAwait to return to UI thread:

```
await service.AnalyzeImagesAsync(request); // Returns to UI thread
```

### Cancellation Support

Implement cancellation token for long-running operations:

1. Add CancellationTokenSource field to form
2. Create token on analyze button click
3. Add "Cancel" button to control bar
4. Pass token to service methods
5. Handle OperationCanceledException
6. Reset UI state on cancellation

**Service Method Signature with Cancellation**

```
Task<MultimodalResponse> AnalyzeImagesAsync(
    MultimodalRequest request,
    CancellationToken cancellationToken = default
)
```

**HttpClient Timeout vs Cancellation**

- Use CancellationTokenSource.CancelAfter for timeout
- Pass token to HttpClient methods
- Catch TaskCanceledException
- Distinguish timeout from user cancellation

## Error Handling Strategy

### Exception Handling Hierarchy

Define custom exceptions:

| Exception Type | Base Class | Purpose |
|---------------|------------|---------|
| AIServiceException | Exception | Base for all AI service errors |
| AIServiceConfigurationException | AIServiceException | Configuration errors |
| AIServiceTimeoutException | AIServiceException | Request timeout |
| AIServiceAuthenticationException | AIServiceException | API key invalid |
| AIServiceNetworkException | AIServiceException | Network connectivity |

### User-Facing Error Messages

Map technical exceptions to user-friendly messages:

| Technical Error | User Message | Action Buttons |
|----------------|--------------|----------------|
| HttpRequestException (connection refused) | "Unable to connect to local AI service. Please start the server." | [Retry] [Configure] |
| TaskCanceledException (timeout) | "Request timed out. The AI service took too long to respond." | [Retry] [Increase Timeout] |
| AIServiceAuthenticationException | "Invalid API key. Please check your configuration." | [Configure] |
| JsonException | "Received invalid response from AI service." | [Retry] [View Details] |
| AIServiceConfigurationException | "AI service is not properly configured." | [Configure] |

### Logging Strategy

For debugging and support:

1. Log all exceptions to file: %AppData%/SETUNA/Logs/ai-summary.log
2. Include timestamp, exception type, message, stack trace
3. Log API request/response (without sensitive data)
4. Rotate log files daily, keep 7 days
5. Provide "View Logs" button in error dialogs

**Log Entry Format**

```
[2024-12-04 13:15:01] ERROR: AIServiceTimeoutException
Message: Request timed out after 30 seconds
Service: MiniCPMService
Endpoint: http://localhost:8080/analyze
Images: 3
Stack: [stack trace]
```

### Graceful Degradation

When features unavailable:

| Condition | Degradation Strategy |
|-----------|---------------------|
| No AI service configured | Show configuration prompt on form load |
| WebView2 not available | Use RichTextBox renderer automatically |
| EPPlus not available | Disable Excel export button |
| No internet (cloud mode) | Show offline warning, suggest local mode |

## Testing Considerations

### Unit Test Targets

Components requiring unit tests:

| Component | Test Focus | Mock Dependencies |
|-----------|-----------|-------------------|
| MarkdownRenderer | Parsing and formatting logic | None |
| MarkdownExporter | File writing and metadata | File system |
| ExcelExporter | Table detection and Excel generation | File system |
| MultimodalServiceFactory | Service instantiation logic | Configuration |
| MiniCPMService | Request construction and error handling | HttpClient |
| QwenVLService | Request construction and retry logic | HttpClient |

### Integration Test Scenarios

| Scenario | Setup | Expected Result |
|----------|-------|-----------------|
| End-to-end with mock service | Mock HTTP responses | Markdown displayed in UI |
| Screenshot retrieval | Create test scraps in ScrapBook | Thumbnails appear in panel |
| Configuration persistence | Modify config, restart app | Settings retained |
| Error recovery | Simulate service failure | Error message displayed, retry works |
| Memory leak test | Analyze 100 times | Memory usage stable |

### Manual Test Checklist

Pre-release verification:

- [ ] Form opens without errors
- [ ] Thumbnails display correctly for various image sizes
- [ ] Local service integration works with MiniCPM server
- [ ] Cloud service integration works with valid API key
- [ ] Markdown rendering handles various formatting
- [ ] Export markdown creates valid UTF-8 file
- [ ] Export Excel creates valid .xlsx with proper formatting
- [ ] Configuration persists across application restarts
- [ ] Error messages are user-friendly
- [ ] Cancel button stops processing
- [ ] Memory usage remains under 150 MB
- [ ] No crashes with empty ScrapBook
- [ ] No crashes with malformed AI responses
- [ ] UI remains responsive during processing
- [ ] High-DPI displays render correctly

## Performance Optimization

### Image Compression Strategy

Reduce memory and transmission overhead:

**Compression Parameters**

| Parameter | Value | Rationale |
|-----------|-------|-----------|
| Max dimensions | 1920x1080 | Balance quality and size |
| JPEG quality | 85% | Minimal visual loss |
| Format | JPEG | Smaller than PNG for photos |
| Color depth | 24-bit | Standard RGB |

**Implementation**

```
private static byte[] CompressImage(Image original)
{
    // Calculate target dimensions
    var scale = Math.Min(
        1920.0 / original.Width,
        1080.0 / original.Height
    );
    
    if (scale >= 1.0)
    {
        // No resize needed
        return EncodeAsJpeg(original, 85);
    }
    
    // Resize proportionally
    var newWidth = (int)(original.Width * scale);
    var newHeight = (int)(original.Height * scale);
    
    using (var resized = new Bitmap(newWidth, newHeight))
    {
        using (var g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(original, 0, 0, newWidth, newHeight);
        }
        
        return EncodeAsJpeg(resized, 85);
    }
}
```

### Thumbnail Caching

Avoid regenerating thumbnails:

1. Create thumbnails once on form load
2. Store in Dictionary&lt;ScrapBase, Image&gt;
3. Reuse for redraw operations
4. Dispose all thumbnails in form Dispose method

**Memory Budget**

- 150x150 thumbnail: ~67 KB uncompressed
- 10 thumbnails: ~670 KB total
- Acceptable overhead for improved performance

### Async Best Practices

Maximize responsiveness:

1. Never block UI thread with .Result or .Wait()
2. Use Task.WhenAll for parallel operations when possible
3. Avoid async void except for event handlers
4. Always handle exceptions in async methods
5. Use IProgress&lt;T&gt; for progress reporting

## Deployment Considerations

### Build Configuration

**Project File Changes**

Add to SETUNA.csproj:

```xml
<ItemGroup>
  <PackageReference Include="EPPlus" Version="5.8.14" />
</ItemGroup>
```

Ensure Newtonsoft.Json is already referenced (check existing packages.config or PackageReference).

**ILMerge Configuration**

Update ILMerge settings to embed EPPlus:

- Add EPPlus.dll to merge list
- Exclude EPPlus.Interfaces.dll (if present)
- Set internalize to false for EPPlus
- Verify single executable output includes EPPlus

**Build Verification**

Test single-executable deployment:

1. Build in Release configuration
2. Run ILMerge post-build
3. Copy SETUNA.exe to clean test folder
4. Run without installing dependencies
5. Verify AI Summary feature functions
6. Check EPPlus operations work

### User Documentation

Create user guide sections:

**Getting Started with AI Summary**

1. Capture screenshots using normal workflow
2. Right-click system tray icon
3. Select "AI Summary..."
4. Choose AI model (local or cloud)
5. Click "Analyze"
6. Review and export results

**Local Model Setup (MiniCPM-V-4.5)**

1. Download MiniCPM-V-4.5 model
2. Install Python inference server
3. Start server: `python serve.py --port 8080`
4. Configure SETUNA endpoint (default works)
5. No API key required

**Cloud Service Setup (Qwen3-VL-Flash)**

1. Register at DashScope: https://dashscope.aliyun.com/
2. Generate API key
3. Open SetunaConfig.xml
4. Add API key to AISummary section
5. Select "Qwen3-VL-Flash (Cloud)" in dropdown

**Troubleshooting Guide**

Common issues and solutions:

| Issue | Cause | Solution |
|-------|-------|----------|
| "Unable to connect" | Local server not running | Start MiniCPM server |
| "Invalid API key" | Wrong or expired key | Check DashScope dashboard |
| "Request timeout" | Large images or slow service | Increase timeout in config |
| "No tables found" | Summary has no tables | Only use Excel export when tables present |
| High memory usage | Too many large images | Reduce MaxImages setting |

### Release Checklist

Before feature release:

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Manual test checklist complete
- [ ] User documentation written
- [ ] API key security reviewed
- [ ] Memory profiling conducted
- [ ] Error messages reviewed for clarity
- [ ] Logging implemented and tested
- [ ] Single-executable deployment verified
- [ ] Backward compatibility tested (old config files)
- [ ] License compliance verified (EPPlus)
- [ ] Code comments and XML documentation added
- [ ] Performance benchmarks recorded

## Security Considerations

### API Key Protection

**Storage**

- Store in SetunaConfig.xml as plain text
- Rely on file system permissions for security
- Do not transmit except to configured endpoint
- Never log complete API key

**Display Masking**

In configuration UI, mask API key:
- Show only first 6 and last 4 characters
- Replace middle with asterisks: "sk-abc***xyz123"
- Unmask with "Show" button (eye icon)

**Transmission**

- Cloud API: HTTPS only (enforce TLS 1.2+)
- Local endpoint: Support both HTTP and HTTPS
- Validate SSL certificates (no override option)
- Use Authorization header (never URL parameters)

### Data Privacy

**User Disclosure**

First-time feature usage shows consent dialog:

```
AI Screenshot Summary

This feature transmits your screenshots to an AI service for analysis.

- Local Model: Data stays on your computer
- Cloud Service: Data sent to third-party provider

Screenshots may contain sensitive information. Review your data 
before analysis.

[ ] Don't show this again
[Cancel] [Continue]
```

Store consent flag in configuration: AISummary.ConsentGiven

**Data Retention**

- No persistent storage of API requests/responses
- Images disposed from memory after processing
- Export files saved only when user explicitly requests
- No telemetry or usage tracking

### Input Validation

Validate all user inputs:

| Input | Validation | Error Handling |
|-------|-----------|----------------|
| LocalEndpoint | Valid URI format, HTTP/HTTPS scheme | Show error, revert to default |
| ApiKey | Minimum 8 characters, no whitespace | Show error, prevent save |
| MaxImages | Integer between 1 and 10 | Clamp to valid range |
| TimeoutSeconds | Integer between 10 and 120 | Clamp to valid range |
| PromptTemplate | Maximum 2000 characters | Truncate with warning |

### Dependency Security

**NuGet Package Verification**

Before deployment:
- Check EPPlus version for known vulnerabilities
- Review EPPlus license terms
- Verify Newtonsoft.Json version is up-to-date
- Check .NET Framework security patches

**Third-Party Service Trust**

For cloud services:
- Document data transmission in user agreement
- Provide link to DashScope privacy policy
- Allow users to opt-out (use local mode only)
- No automatic service updates without user consent

## Configuration Reference

### Complete XML Example

```xml
<SetunaOption>
  <!-- Existing configuration sections... -->
  
  <AISummary>
    <Enabled>true</Enabled>
    <Engine>qwen3-vl-flash</Engine>
    <LocalEndpoint>http://localhost:8080</LocalEndpoint>
    <ApiKey>sk-abc123def456789...</ApiKey>
    <MaxImages>5</MaxImages>
    <PromptTemplate>Analyze the provided screenshots and generate a comprehensive summary in markdown format. Include: 1) Overview section describing the main content, 2) Detailed findings organized with headings and bullet points, 3) Tables for any structured data observed, 4) Key observations section. Use clear hierarchical structure with H1-H3 headings.</PromptTemplate>
    <UseWebView2>false</UseWebView2>
    <TimeoutSeconds>30</TimeoutSeconds>
    <ConsentGiven>true</ConsentGiven>
  </AISummary>
</SetunaOption>
```

### Configuration File Location

- Path: %AppData%\SETUNA\SetunaConfig.xml
- Alternative: Application startup directory\SetunaConfig.xml
- Encoding: UTF-8
- Format: XML with proper indentation
- Backup: Automatic backup before each save (SetunaConfig.xml.bak)

### Default Configuration

When AISummary section missing or corrupt:

```csharp
public AISummaryConfig()
{
    Enabled = false;
    Engine = "minicpm-v4.5";
    LocalEndpoint = "http://localhost:8080";
    ApiKey = string.Empty;
    MaxImages = 5;
    PromptTemplate = GetDefaultPrompt();
    UseWebView2 = false;
    TimeoutSeconds = 30;
    ConsentGiven = false;
}
```

## Implementation Checklist

### Phase 1: Core Infrastructure

- [ ] Create project folder structure (Main/AI/)
- [ ] Define data models (AISummaryConfig, request/response)
- [ ] Implement IMultimodalService interface
- [ ] Create MultimodalServiceFactory
- [ ] Add AISummary property to SetunaOption
- [ ] Update GetAllType method
- [ ] Test configuration serialization

### Phase 2: Service Implementations

- [ ] Implement MiniCPMService
- [ ] Implement QwenVLService
- [ ] Add image compression utility
- [ ] Add base64 encoding utility
- [ ] Implement retry logic for cloud service
- [ ] Test services with mock endpoints
- [ ] Add comprehensive error handling

### Phase 3: User Interface

- [ ] Create AISummaryForm class
- [ ] Design form layout (SplitContainer)
- [ ] Implement screenshot selection panel
- [ ] Implement thumbnail generation
- [ ] Add control bar components
- [ ] Create MarkdownRenderer class
- [ ] Implement status bar
- [ ] Add error notification bar

### Phase 4: Export Functionality

- [ ] Implement MarkdownExporter
- [ ] Implement table detection logic
- [ ] Implement ExcelExporter with EPPlus
- [ ] Add export button handlers
- [ ] Test export with various markdown formats
- [ ] Handle edge cases (no tables, malformed markdown)

### Phase 5: Integration

- [ ] Add menu item to Mainform
- [ ] Implement menu click handler
- [ ] Test form opening from menu
- [ ] Verify ScrapBook enumeration
- [ ] Test configuration persistence
- [ ] Verify no breaking changes to existing features

### Phase 6: Testing and Polish

- [ ] Write unit tests for core components
- [ ] Perform integration testing
- [ ] Complete manual test checklist
- [ ] Fix memory leaks if found
- [ ] Optimize performance bottlenecks
- [ ] Add user documentation
- [ ] Create troubleshooting guide
- [ ] Final deployment verification

---

**Confidence Assessment**: High

**Confidence Basis**:
- Clear integration points identified in existing codebase
- Proven patterns (BaseForm, SetunaOption, async/await) already in use
- Service abstraction allows independent development and testing
- No breaking changes to existing functionality
- Comprehensive error handling strategy defined
- Realistic performance and memory targets set
