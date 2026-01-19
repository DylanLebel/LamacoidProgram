# LamCreator - Quick Start Guide

## 🚀 Get Building in 5 Minutes

Follow these steps to fix the build errors and get the application running.

---

## Step 1: Install DraftSight (Required)

**This is the #1 critical step** - without DraftSight installed, the project cannot build.

1. Download DraftSight from: https://www.3ds.com/products/draftsight
2. Install it (full installation)
3. Launch DraftSight at least once
4. Verify the files exist at: `C:\Program Files\Dassault Systemes\DraftSight\APISDK\tlb\`
   - `DraftSight.Interop.dsAutomation.dll`
   - `DraftSight.Interop.dsAddin.dll`

**Don't have admin access to install?** See `FIX_DRAFTSIGHT_REFERENCES.md` for alternative options (copying DLLs from another machine).

---

## Step 2: Restore NuGet Packages

Open the solution in Visual Studio and restore packages:

### Method A: Automatic Restore (Easiest)
1. Open `Lamacoid Creator.sln` in Visual Studio
2. Visual Studio should automatically prompt to restore packages
3. Click "Restore"

### Method B: Manual Restore
1. In Visual Studio: `Tools` → `NuGet Package Manager` → `Package Manager Console`
2. Run:
   ```powershell
   Update-Package -reinstall
   ```

### Method C: Command Line
```bash
cd "Lamacoid Creator"
nuget restore
```

---

## Step 3: Remove Acrobat Reference (Optional but Recommended)

The Acrobat COM reference is not needed and causes errors.

1. Open `Lamacoid Creator.csproj` in a text editor (or unload project in VS and edit)
2. Find and **delete** these lines (around line 109-118):
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
3. Save the file
4. Reload the project in Visual Studio

---

## Step 4: Clean and Rebuild

1. In Visual Studio: `Build` → `Clean Solution`
2. Then: `Build` → `Rebuild Solution` (or press Ctrl+Shift+B)

**Expected Result**: Build succeeds with 0 errors! ✅

---

## Step 5: Test Run

1. **Launch DraftSight first** (must be running!)
2. Press F5 in Visual Studio to run the application
3. The application should start
4. Check the logs at: `bin\Debug\Logs\LamCreator_[date].log`

You should see:
```
[INFO] === Lamacoid Creator Application Started ===
[INFO] Successfully connected to DraftSight application
[INFO] Loading templates from: Y:\Autocad\Template\~LAMACOIDS\Lams\Templates
```

---

## ✅ Success Checklist

- [ ] DraftSight installed and Interop DLLs present
- [ ] NuGet packages restored (BouncyCastle.Cryptography 2.4.0, iTextSharp, PdfiumViewer)
- [ ] Acrobat COM reference removed
- [ ] Project builds with 0 errors
- [ ] Application runs and connects to DraftSight
- [ ] Log files are being created in `bin\Debug\Logs\`

---

## 🔥 Troubleshooting

### Build Error: "DraftSight not found"
**Solution**: See `FIX_DRAFTSIGHT_REFERENCES.md` for detailed instructions

### Build Error: "BouncyCastle not found"
**Solution**: Restore NuGet packages (Step 2)

### Build Error: "Acrobat library not registered"
**Solution**: Remove the Acrobat reference (Step 3)

### Runtime Error: "DraftSight application not found"
**Solution**: Launch DraftSight before running the application

### More Help
See the complete troubleshooting guide: `TROUBLESHOOTING.md`

---

## 📁 Important Files

| File | Purpose |
|------|---------|
| `FIX_DRAFTSIGHT_REFERENCES.md` | Detailed guide for fixing DraftSight Interop issues |
| `TROUBLESHOOTING.md` | Complete troubleshooting guide |
| `CHANGELOG.md` | List of all changes made |
| `Logs\` | Application log files (created at runtime) |

---

## 🎯 What You Get

After completing these steps, you'll have:
- ✅ A building application with no errors
- ✅ Comprehensive debug logging system
- ✅ Updated secure dependencies (BouncyCastle.Cryptography 2.4.0)
- ✅ Detailed logs showing every operation
- ✅ Performance metrics for troubleshooting
- ✅ Full stack traces for all errors

---

## 💡 Quick Tips

1. **Always check the logs first** - They contain detailed information about every operation
2. **Keep DraftSight running** - The application needs it to work
3. **Log files rotate automatically** - They won't fill up your disk
4. **Performance metrics included** - See how long each operation takes

---

## 🆘 Need More Help?

1. Check `FIX_DRAFTSIGHT_REFERENCES.md` for DraftSight-specific issues
2. Check `TROUBLESHOOTING.md` for all other issues
3. Look at the log files in `bin\Debug\Logs\`
4. All errors now include full stack traces and context

---

**That's it!** You should now have a fully functional application with enterprise-grade logging. Happy debugging! 🎉
