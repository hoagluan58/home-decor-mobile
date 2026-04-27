# deploy-webgl.ps1
# ─────────────────────────────────────────────────────────────
# Run this script AFTER doing a WebGL build in Unity.
# It copies the build output into /docs and pushes to GitHub.
#
# Usage:
#   .\deploy-webgl.ps1
# Or with a custom build output path:
#   .\deploy-webgl.ps1 -BuildPath "C:\MyBuilds\WebGL"
# ─────────────────────────────────────────────────────────────

param(
    [string]$BuildPath = "$PSScriptRoot\LocalWebGLBuild"
)

$DocsPath = "$PSScriptRoot\docs"
$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Home Decor — WebGL Deploy to GitHub" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ── 1. Validate build output exists ──────────────────────────
if (-not (Test-Path "$BuildPath\index.html")) {
    Write-Host "ERROR: No WebGL build found at: $BuildPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please build for WebGL in Unity first:" -ForegroundColor Yellow
    Write-Host "  File -> Build Settings -> WebGL -> Build"  -ForegroundColor Yellow
    Write-Host "  Output folder: $BuildPath"                 -ForegroundColor Yellow
    exit 1
}

Write-Host "Found WebGL build at: $BuildPath" -ForegroundColor Green

# ── 2. Clear old docs/ and copy new build ────────────────────
Write-Host "Copying build to /docs ..." -ForegroundColor Yellow

if (Test-Path $DocsPath) {
    Remove-Item -Recurse -Force $DocsPath
}
Copy-Item -Recurse $BuildPath $DocsPath

Write-Host "Done copying." -ForegroundColor Green

# ── 3. Commit and push ───────────────────────────────────────
Write-Host ""
Write-Host "Committing and pushing to GitHub ..." -ForegroundColor Yellow

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"

git add docs/
git commit -m "deploy: WebGL build ($timestamp)"
git push origin main

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Deployed! Your game is live at:"      -ForegroundColor Green
Write-Host ""
Write-Host "  https://hoagluan58.github.io/home-decor-mobile/" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
