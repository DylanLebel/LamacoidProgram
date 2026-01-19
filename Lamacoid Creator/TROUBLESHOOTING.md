# LamCreator - Troubleshooting Guide

## Current Build Errors and Solutions

### 1. Missing DraftSight Interop Assemblies

**Error Messages:**
```
The type or namespace name 'Document' could not be found
The type or namespace name 'DraftSight' could not be found
Cannot get the file path for type library "e64169b3-3592-47d2-816e-602c5c13f328" version 1.1
The referenced component 'DraftSight.Interop.dsAutomation' could not be found
The referenced component 'DraftSight.Interop.dsAddin' could not be found
```

**Root Cause:**
These errors occur because the DraftSight COM Interop assemblies are not available on your build machine. These assemblies are created from the DraftSight COM type libraries and must be present for the application to compile.

**Solution Options:**

#### Option A: Install DraftSight (Recommended)
1. Download and install DraftSight from Dassault Systèmes
2. During installation, ensure COM Interop is enabled
3. The installation will register the necessary COM type libraries
4. Rebuild the project

#### Option B: Add Interop Assemblies to Project
1. If you have another machine with DraftSight installed, locate these DLL files:
   - `DraftSight.Interop.dsAutomation.dll`
   - `DraftSight.Interop.dsAddin.dll`
2. Copy them to your project's `References` folder
3. Update the project references to point to these local copies
4. Commit the DLL files to source control

#### Option C: Generate Interop Assemblies Manually
If you have access to DraftSight's type library files:
1. Open Visual Studio Developer Command Prompt
2. Navigate to the directory containing the type library (.tlb or .olb file)
3. Run: `tlbimp.exe DraftSight.tlb /out:DraftSight.Interop.dsAutomation.dll`
4. Add the generated DLL as a project reference

**Files Affected:**
- `LamacoidForm.cs:7` - Uses `DraftSight.Interop.dsAutomation`
- `DraftSightHelper.cs:7` - Uses `DraftSight.Interop.dsAutomation`

---

### 2. Missing Acrobat Reference

**Error Message:**
```
The referenced component 'Acrobat' could not be found
```

**Root Cause:**
The project references Adobe Acrobat's COM library, but it's not installed or registered on your system.

**Solution:**

#### Check if Reference is Actually Needed
1. Search the codebase for any usage of Acrobat libraries
2. If not used, remove the reference from the project file

#### If Acrobat is Needed
1. Install Adobe Acrobat Pro (not just Reader)
2. Ensure the Acrobat COM library is registered
3. Re-add the reference in Visual Studio

**Verification:**
Use the included logging system to check if PDF operations are working with iTextSharp (current implementation) instead of Acrobat.

---

### 3. BouncyCastle Security Vulnerabilities

**Error Messages:**
```
Package 'BouncyCastle' 1.8.9 has a known moderate severity vulnerability:
- https://github.com/advisories/GHSA-m44j-cfrm-g8qc
- https://github.com/advisories/GHSA-8xfc-gm6g-vgpv
- https://github.com/advisories/GHSA-v435-xc8x-wvr9
```

**Root Cause:**
The project originally used BouncyCastle 1.8.9, which has known security vulnerabilities. BouncyCastle is a dependency of iTextSharp.

**Status: ✅ FIXED**

The project has been updated to use **BouncyCastle.Cryptography 2.4.0**, which is the modern, secure version.

**What Was Changed:**
- `packages.config`: Updated to `BouncyCastle.Cryptography version="2.4.0"`
- `Lamacoid Creator.csproj`: Updated reference to point to BouncyCastle.Cryptography 2.4.0

**To Apply This Fix:**
1. Open NuGet Package Manager or use Package Manager Console
2. Run: `Update-Package` or restore packages
3. The new BouncyCastle.Cryptography 2.4.0 will be downloaded
4. Rebuild the project

**Verification:**
After restoring packages, you should see:
- `packages\BouncyCastle.Cryptography.2.4.0\` folder
- No more vulnerability warnings for BouncyCastle

**Compatibility:**
BouncyCastle.Cryptography 2.4.0 is fully compatible with iTextSharp 5.5.13.3 and does not require code changes.

---

## New Logging System

The application now includes a comprehensive logging system that will help diagnose issues:

### Log File Location
```
[Application Directory]\Logs\LamCreator_YYYYMMDD.log
```

### Log Features
- **Multiple log levels**: DEBUG, INFO, WARNING, ERROR, FATAL
- **Automatic rotation**: When logs exceed 10 MB
- **Detailed stack traces**: Full exception information
- **Performance metrics**: Timing for operations
- **Source tracking**: File, method, and line number for each log entry

### Viewing Logs
1. Run the application
2. Navigate to the `Logs` folder in the application directory
3. Open the current date's log file
4. Look for ERROR or FATAL entries to identify issues

### Log Entry Format
```
[2026-01-19 14:30:45.123] [INFO   ] Application started
    Source: LamacoidForm.cs -> LamacoidForm() [Line 34]

[2026-01-19 14:30:45.456] [ERROR  ] Failed to connect to DraftSight
    Source: DraftSightHelper.cs -> InitializeDraftSight() [Line 70]
    Exception Type: System.Runtime.InteropServices.COMException
    Exception Message: Operation unavailable (Exception from HRESULT: 0x800401E3)
    Stack Trace:
        at System.Runtime.InteropServices.Marshal.GetActiveObject(Guid& rclsid, IntPtr reserved, Object& ppunk)
        at DraftSight_Helper.InitializeDraftSight() in DraftSightHelper.cs:line 66
```

---

## Build Instructions

### Prerequisites
1. Visual Studio 2019 or later
2. .NET Framework 4.7.2 SDK
3. DraftSight installed (for COM Interop assemblies)
4. NuGet Package Manager

### Build Steps
1. Open `Lamacoid Creator.sln` in Visual Studio
2. Restore NuGet packages: `Tools` → `NuGet Package Manager` → `Manage NuGet Packages for Solution` → Click `Restore`
3. Resolve the DraftSight reference issues (see Section 1)
4. Build the solution: `Build` → `Build Solution` (Ctrl+Shift+B)
5. Check the Logs folder for any runtime issues

### Clean Build
If you encounter persistent issues:
```powershell
# In Visual Studio Package Manager Console
dotnet clean
Remove-Item -Recurse -Force bin,obj
dotnet restore
dotnet build
```

---

## Runtime Requirements

### Required Software
- **Windows OS**: Windows 10 or later
- **DraftSight**: Must be installed and running for the application to work
- **.NET Framework 4.7.2**: Runtime must be installed

### Network Access
The application expects access to:
- `Y:\Autocad\Template\~LAMACOIDS\Lams\` - Template and output directory
- `Y:\Autocad\Template\~LAMACOIDS\Lams\Templates\` - Template source files

If these paths are not accessible, you'll see errors in the logs and MessageBoxes.

---

## Getting Help

1. **Check the Logs**: Always check the log files first
2. **Review Error Messages**: The new logging system captures full stack traces
3. **Verify Prerequisites**: Ensure DraftSight is installed and running
4. **Network Paths**: Confirm access to Y:\ drive paths
5. **Rebuild References**: Try removing and re-adding the DraftSight references

---

## Version Information

- **Application**: LamCreator v1.0
- **Framework**: .NET Framework 4.7.2
- **Dependencies**:
  - DraftSight Interop (COM)
  - iTextSharp 5.5.13.3
  - PdfiumViewer 2.13.0.0
  - BouncyCastle 1.8.9 (⚠️ needs update)

Last Updated: 2026-01-19
