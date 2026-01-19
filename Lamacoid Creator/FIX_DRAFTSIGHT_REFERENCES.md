# How to Fix DraftSight Interop References

## The Problem

You're seeing these errors:
```
The type or namespace name 'DraftSight' could not be found
The referenced component 'DraftSight.Interop.dsAutomation' could not be found
The referenced component 'DraftSight.Interop.dsAddin' could not be found
```

**Root Cause**: The DraftSight COM Interop DLLs are not available on your machine. These are required to compile the application.

## Solution Options

### ✅ Option 1: Install DraftSight (Recommended)

This is the easiest and most reliable solution.

1. **Download DraftSight**
   - Go to: https://www.3ds.com/products/draftsight
   - Download the installer for Windows
   - Choose the appropriate version (Standard or Professional)

2. **Install DraftSight**
   - Run the installer
   - Complete the installation
   - Launch DraftSight at least once to complete the setup

3. **Verify Interop Files Exist**
   - Navigate to: `C:\Program Files\Dassault Systemes\DraftSight\APISDK\tlb\`
   - You should see:
     - `DraftSight.Interop.dsAutomation.dll`
     - `DraftSight.Interop.dsAddin.dll`

4. **Rebuild the Project**
   - Open Visual Studio
   - Right-click solution → Clean Solution
   - Right-click solution → Restore NuGet Packages
   - Right-click solution → Rebuild Solution

---

### ✅ Option 2: Copy Interop DLLs from Another Machine

If you have access to another machine with DraftSight installed:

1. **On Machine WITH DraftSight**:
   - Navigate to: `C:\Program Files\Dassault Systemes\DraftSight\APISDK\tlb\`
   - Copy these files:
     - `DraftSight.Interop.dsAutomation.dll`
     - `DraftSight.Interop.dsAddin.dll`

2. **On Your Development Machine**:
   - Create directory: `C:\Program Files\Dassault Systemes\DraftSight\APISDK\tlb\`
   - Paste the DLL files there

3. **Alternative: Add to Project Directory**
   - Create folder in your project: `Lamacoid Creator\Lamacoid Creator\lib\`
   - Copy the DLL files there
   - Update the `.csproj` file:

   ```xml
   <Reference Include="DraftSight.Interop.dsAddin">
     <HintPath>lib\DraftSight.Interop.dsAddin.dll</HintPath>
     <EmbedInteropTypes>True</EmbedInteropTypes>
   </Reference>
   <Reference Include="DraftSight.Interop.dsAutomation">
     <HintPath>lib\DraftSight.Interop.dsAutomation.dll</HintPath>
     <EmbedInteropTypes>True</EmbedInteropTypes>
   </Reference>
   ```

4. **Rebuild the Project**

---

### ✅ Option 3: Generate Interop Assemblies (Advanced)

If you have access to DraftSight's type library files but not the Interop DLLs:

1. **Locate Type Library Files**
   - Look for `.tlb` or `.olb` files in DraftSight installation
   - Typically in: `C:\Program Files\Dassault Systemes\DraftSight\`

2. **Open Developer Command Prompt**
   - Start → Visual Studio 2019 → Developer Command Prompt for VS 2019
   - Run as Administrator

3. **Generate Interop DLL**
   ```cmd
   cd "C:\Program Files\Dassault Systemes\DraftSight\APISDK\tlb"
   tlbimp.exe dsAutomation.tlb /out:DraftSight.Interop.dsAutomation.dll
   tlbimp.exe dsAddin.tlb /out:DraftSight.Interop.dsAddin.dll
   ```

4. **Add to Project**
   - Follow Option 2 steps to add the generated DLLs

---

## Verifying the Fix

After implementing one of the solutions above:

1. **Open Visual Studio**
2. **Clean the Solution**: `Build` → `Clean Solution`
3. **Restore NuGet Packages**: Right-click solution → `Restore NuGet Packages`
4. **Rebuild**: `Build` → `Rebuild Solution` (Ctrl+Shift+B)

You should see:
- ✅ No more "DraftSight not found" errors
- ✅ All DraftSight types recognized (Document, Application, etc.)
- ✅ Build succeeds

---

## Remaining Issues After Fix

### BouncyCastle Package Issue

**Current Status**: Fixed! Updated to `BouncyCastle.Cryptography 2.4.0`

The project now uses the modern BouncyCastle.Cryptography package which is compatible with iTextSharp and doesn't have the vulnerabilities of the old 1.8.9 version.

### Acrobat COM Reference

**Error**:
```
Cannot get the file path for type library "e64169b3-3592-47d2-816e-602c5c13f328" version 1.1
The referenced component 'Acrobat' could not be found
```

**Quick Fix**: Remove the Acrobat reference (it's not being used)

1. Open `Lamacoid Creator.csproj` in a text editor
2. Find and **DELETE** these lines (around line 109-118):
   ```xml
   <COMReference Include="Acrobat">
     <Guid>{E64169B3-3592-47D2-816E-602C5C13F328}</Guid>
     <VersionMajor>1</VersionMajor>
     <VersionMinor>1</VersionMinor>
     <Lcid>0</Lcid>
     <WrapperTool>tlbimp</WrapperTool>
     <Isolated>False</Isolated>
     <EmbedInteropTypes>True</EmbedInteropTypes>
   </COMReference>
   ```
3. Save and rebuild

The application uses **iTextSharp** and **PdfiumViewer** for PDF operations, not Adobe Acrobat, so this reference is unnecessary.

---

## Runtime Requirements

Even after the project compiles successfully, **DraftSight must be running** for the application to work at runtime:

1. **Launch DraftSight** before running the application
2. The application connects to DraftSight via COM: `Marshal.GetActiveObject("DraftSight.Application")`
3. If DraftSight is not running, you'll see this error:
   ```
   Error: DraftSight application not found. Please ensure DraftSight is running.
   ```

The new logging system will capture this in the log files with full details.

---

## Testing After Fix

1. **Build Test**
   ```
   Build → Rebuild Solution
   ```
   Expected: Build succeeds with 0 errors

2. **Run Test** (with DraftSight running)
   - Launch DraftSight
   - Press F5 in Visual Studio
   - Application should start
   - Check `Logs\LamCreator_YYYYMMDD.log` for:
     ```
     [INFO] === Lamacoid Creator Application Started ===
     [INFO] Successfully connected to DraftSight application
     ```

3. **Check Log Files**
   - Navigate to: `[Project]\bin\Debug\Logs\`
   - Open today's log file
   - Verify logging is working

---

## Quick Reference: File Paths

**DraftSight Interop DLLs**:
```
C:\Program Files\Dassault Systemes\DraftSight\APISDK\tlb\
├── DraftSight.Interop.dsAutomation.dll
└── DraftSight.Interop.dsAddin.dll
```

**Project Files**:
```
Lamacoid Creator\Lamacoid Creator\
├── Lamacoid Creator.csproj  (Update reference paths here)
├── packages.config            (NuGet packages)
└── Logs\                      (Created at runtime)
```

**NuGet Packages**:
```
Lamacoid Creator\packages\
├── BouncyCastle.Cryptography.2.4.0\
├── iTextSharp.5.5.13.3\
└── PdfiumViewer.2.13.0.0\
```

---

## Still Having Issues?

1. **Check the logs**: `[Project]\bin\Debug\Logs\LamCreator_YYYYMMDD.log`
2. **Verify DraftSight is installed**: Launch it manually
3. **Check file paths**: Ensure the `.csproj` HintPath matches actual file locations
4. **Clean and Rebuild**: Sometimes Visual Studio caches need clearing
5. **Restart Visual Studio**: After adding DLLs, restart VS

---

## Summary

| Issue | Solution | Priority |
|-------|----------|----------|
| DraftSight Interop Missing | Install DraftSight OR copy DLLs | 🔴 Critical |
| BouncyCastle Version | ✅ Fixed - Updated to 2.4.0 | ✅ Done |
| Acrobat Reference | Remove from .csproj | 🟡 Optional |
| DraftSight Not Running | Launch before running app | 🔵 Runtime |

**Most Important**: Get the DraftSight Interop DLLs using Option 1 or Option 2 above. Without these, the project cannot compile.
