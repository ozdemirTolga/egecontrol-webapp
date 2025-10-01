# EgeControl Web App - Deployment Script
# Usage: .\deploy.ps1

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "..\publish"
)

Write-Host "=== EgeControl Deployment Starting ===" -ForegroundColor Green
Write-Host ""

# 1. Directory check
$projectPath = ".\EgeControlWebApp"
$publishPath = ".\publish"

if (-not (Test-Path $projectPath)) {
    Write-Host "ERROR: EgeControlWebApp folder not found!" -ForegroundColor Red
    exit 1
}

Set-Location $projectPath

# 2. Clean old publish folder
Write-Host "Cleaning old publish folder..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  [OK] Old files removed" -ForegroundColor Gray
}

# 3. Build
Write-Host ""
Write-Host "Building ($Configuration)..." -ForegroundColor Yellow
dotnet build -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    Set-Location ..
    exit 1
}
Write-Host "  [OK] Build successful" -ForegroundColor Green

# 4. Publish
Write-Host ""
Write-Host "Publishing..." -ForegroundColor Yellow
dotnet publish -c $Configuration -o $OutputPath --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publish failed!" -ForegroundColor Red
    Set-Location ..
    exit 1
}
Write-Host "  [OK] Publish completed" -ForegroundColor Green

# 5. Copy wwwroot manually (StaticWebAssets workaround)
Write-Host ""
Write-Host "Copying wwwroot..." -ForegroundColor Yellow

$wwwrootSource = ".\wwwroot"
$wwwrootDest = "$OutputPath\wwwroot"

if (Test-Path $wwwrootSource) {
    # Remove existing wwwroot first
    if (Test-Path $wwwrootDest) {
        Remove-Item $wwwrootDest -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Copy
    Copy-Item -Path $wwwrootSource -Destination $wwwrootDest -Recurse -Force -ErrorAction Continue
    
    # Check for nested wwwroot (sometimes becomes wwwroot\wwwroot)
    $nestedWwwroot = "$wwwrootDest\wwwroot"
    if (Test-Path $nestedWwwroot) {
        Write-Host "  [WARNING] Nested wwwroot detected, fixing..." -ForegroundColor Yellow
        Move-Item "$nestedWwwroot\*" $wwwrootDest -Force -ErrorAction Continue
        Remove-Item $nestedWwwroot -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    Write-Host "  [OK] wwwroot copied" -ForegroundColor Green
} else {
    Write-Host "  [WARNING] wwwroot folder not found!" -ForegroundColor Yellow
}

# 6. Calculate size
Write-Host ""
Write-Host "=== Publish Statistics ===" -ForegroundColor Cyan
$publishFullPath = Resolve-Path $OutputPath
$totalSize = (Get-ChildItem $publishFullPath -Recurse -ErrorAction SilentlyContinue | 
              Measure-Object -Property Length -Sum).Sum / 1MB
$fileCount = (Get-ChildItem $publishFullPath -Recurse -File -ErrorAction SilentlyContinue | 
              Measure-Object).Count

Write-Host "  Size: $([math]::Round($totalSize, 2)) MB" -ForegroundColor White
Write-Host "  Files: $fileCount" -ForegroundColor White
Write-Host "  Location: $publishFullPath" -ForegroundColor White

# 7. Critical file checks
Write-Host ""
Write-Host "=== Critical File Checks ===" -ForegroundColor Cyan

$criticalFiles = @(
    "EgeControlWebApp.dll",
    "web.config",
    "app.db",
    "appsettings.json",
    "appsettings.Production.json",
    "wwwroot\lib\bootstrap\dist\css\bootstrap.min.css",
    "wwwroot\css\site.css",
    "wwwroot\js\site.js"
)

$allOk = $true
foreach ($file in $criticalFiles) {
    $filePath = Join-Path $publishFullPath $file
    if (Test-Path $filePath) {
        Write-Host "  [OK] $file" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $file" -ForegroundColor Red
        $allOk = $false
    }
}

# 8. Result
Write-Host ""
if ($allOk) {
    Write-Host "=== DEPLOYMENT SUCCESSFUL ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next step: Upload publish folder to server" -ForegroundColor Yellow
    Write-Host "  Location: $publishFullPath" -ForegroundColor Gray
    Write-Host ""
    Write-Host "=== Server Setup Required ===" -ForegroundColor Cyan
    Write-Host "  1. IIS Application Pool: Enable 32-Bit = False" -ForegroundColor White
    Write-Host "  2. File permissions: IIS_IUSRS = Full Control" -ForegroundColor White
    Write-Host "  3. Run: iisreset" -ForegroundColor White
    Write-Host ""
    Write-Host "Documentation: UPLOAD_INSTRUCTIONS.md" -ForegroundColor Gray
} else {
    Write-Host "=== DEPLOYMENT COMPLETED WITH WARNINGS ===" -ForegroundColor Yellow
    Write-Host "  Some files are missing. Check the list above." -ForegroundColor Gray
}

Set-Location ..
