# Test Mode - Running Without DraftSight

This document explains how to run and test Lamacoid Creator without having DraftSight installed.

## Overview

The application now supports a **Test Mode** that allows you to run and test the application without requiring DraftSight to be installed. This is useful for:
- Development and testing on machines without DraftSight
- Automated testing and CI/CD pipelines
- Quick prototyping and UI testing
- Learning how the application works

## How It Works

The application uses an abstraction layer (interface) for all DraftSight operations:
- **IDraftSightHelper** - Interface defining all DraftSight operations
- **DraftSightHelper** - Real implementation using DraftSight COM automation
- **MockDraftSightHelper** - Mock implementation that simulates DraftSight operations
- **DraftSightHelperFactory** - Factory that creates the appropriate implementation based on configuration

## Enabling/Disabling Test Mode

Test mode is controlled by a single constant in `Constants.cs`:

```csharp
public const bool UseTestMode = true;  // Enable test mode (no DraftSight required)
// or
public const bool UseTestMode = false; // Use real DraftSight (requires DraftSight installed)
```

### To Enable Test Mode:
1. Open `Lamacoid Creator/Lamacoid Creator/Constants.cs`
2. Set `UseTestMode = true;`
3. Rebuild the application

### To Disable Test Mode (Use Real DraftSight):
1. Open `Lamacoid Creator/Lamacoid Creator/Constants.cs`
2. Set `UseTestMode = false;`
3. Ensure DraftSight is installed and running
4. Rebuild the application

## What Works in Test Mode

When test mode is enabled, the mock implementation:

✅ **Works:**
- Generates correct file names (E-LAM-001, E-LAM-002, etc.)
- Creates file system structures
- Copies template files
- Creates dummy PDF files (valid PDF structure with placeholder content)
- Logs all operations with `[MOCK]` prefix for easy identification
- Simulates property updates and document operations

⚠️ **Limitations:**
- PDFs contain placeholder text ("Mock Lamacoid") instead of actual lamacoid designs
- DWG files are template copies (properties are not actually embedded)
- Visual Library browser will show placeholder thumbnails
- No actual CAD operations are performed

## Testing Scenarios

### Scenario 1: Test UI and Workflow
```
1. Enable test mode (UseTestMode = true)
2. Run the application
3. Add names to the list
4. Select a template
5. Click Continue
6. Watch the progress bar and logs
7. Verify files are created in the output directory
```

### Scenario 2: Test Visual Library Browser
```
1. Enable test mode
2. Create some test lamacoids using the batch creation interface
3. Open Visual Library form
4. Browse and search the created lamacoids
5. Verify thumbnails and search functionality work
```

### Scenario 3: Switch to Production
```
1. Test with mock mode first
2. Verify all workflows work correctly
3. Switch to UseTestMode = false
4. Ensure DraftSight is installed and running
5. Run application with real DraftSight automation
```

## Logging

All mock operations are logged with a `[MOCK]` prefix for easy identification:

```
[INFO] [MOCK] Simulating opening template: TEMPLATE.dwg
[INFO] [MOCK] Template file exists, simulating successful open
[INFO] [MOCK] Simulating property update: CONTENTS = TEST-01
[INFO] [MOCK] Property 'CONTENTS' updated successfully to 'TEST-01'
[INFO] [MOCK] Simulating PDF export: E-LAM-001.dwg -> E-LAM-001.pdf
[INFO] [MOCK] Dummy PDF created successfully: E-LAM-001.pdf
```

Check the log files in `bin/Debug/Logs/` to see all operations.

## Files Created

The new test mode implementation includes these files:

1. **IDraftSightHelper.cs** - Interface defining DraftSight operations
2. **DraftSightHelper.cs** - Modified to implement the interface (now instance-based)
3. **MockDraftSightHelper.cs** - Mock implementation for testing
4. **DraftSightHelperFactory.cs** - Factory to create appropriate implementation
5. **Constants.cs** - Modified to include `UseTestMode` flag

## Architecture

```
┌─────────────────────────────────────┐
│      Application Code               │
│  (LamacoidForm, LamacoidIndex)      │
└─────────────┬───────────────────────┘
              │
              ↓
      IDraftSightHelper (interface)
              │
        ┌─────┴─────┐
        │           │
        ↓           ↓
DraftSightHelper  MockDraftSightHelper
(Real COM)        (Test/Mock)
        │           │
        ↓           │
   DraftSight       │
   Application      ↓
                File System
```

## Development Tips

1. **Start with Mock Mode**: Always develop and test new features with mock mode first
2. **Check Logs**: Use the `[MOCK]` prefix in logs to verify mock operations
3. **Test Both Modes**: Before deploying, test with both mock and real DraftSight
4. **Network Paths**: Mock mode still uses network paths (Y:\) - ensure they exist or modify Constants.cs for local testing

## Troubleshooting

### Issue: Application crashes on startup
- Check if `UseTestMode` is set correctly
- Verify all new files are included in the project
- Rebuild the solution

### Issue: Files are not created
- Check directory permissions
- Verify network paths exist (even in mock mode, paths must be accessible)
- Review logs for error messages

### Issue: "DraftSight Not Found" error in test mode
- This should NOT happen in test mode
- Verify `UseTestMode = true` in Constants.cs
- Rebuild the application completely

## Next Steps

Consider future enhancements:
- Command-line argument to toggle test mode without recompiling
- Configuration file for test mode setting
- More sophisticated mock PDFs with actual rendered content
- Unit tests using the mock implementation
