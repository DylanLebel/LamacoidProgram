@echo off
echo ==========================================
echo       PUSHING LAMACHOID PROGRAM TO GITHUB
echo ==========================================

:: 1. Initialize Git
git init

:: 2. Stage all files
git add .

:: 3. Commit changes
git commit -m "Update from batch script"

:: 4. Ensure branch is named 'main'
git branch -M main

:: 5. Add the repository link (or update it if it already exists)
git remote add origin https://github.com/DylanLebel/LamacoidProgram.git 2>nul
git remote set-url origin https://github.com/DylanLebel/LamacoidProgram.git

:: 6. Push to GitHub
echo.
echo Pushing files now... (You may be asked to sign in)
git push -u origin main

echo.
echo ==========================================
echo                 FINISHED
echo ==========================================
pause