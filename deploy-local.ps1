<#
.SYNOPSIS
    Local deployment script for SupermarketProject.
    Builds Angular frontend and prepares .NET backend for Azure deployment.

.DESCRIPTION
    This script:
    1. Builds the Angular app to supermarket-app/dist/
    2. Copies ONLY SPA files (JS, CSS, HTML) to wwwroot — NEVER images
    3. Publishes the .NET project
    4. Creates a deploy.zip ready for Azure App Service

    IMPORTANT: Product images (UUID-named) in wwwroot/images/products/ are
    managed at runtime by FileUploadService and are NOT affected by this script.
#>

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$angularDir = Join-Path $root "supermarket-app"
$angularDist = Join-Path $angularDir "dist\supermarket-app\browser"
$wwwroot = Join-Path $root "SupermarketMock\SupermarketMock\wwwroot"
$publishDir = Join-Path $root "SupermarketMock\publish"
$deployZip = Join-Path $root "SupermarketMock\deploy.zip"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SupermarketProject Deploy Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# Step 1: Build Angular
# ============================================================
Write-Host "[1/4] Building Angular app..." -ForegroundColor Yellow
Push-Location $angularDir
try {
    & ng build --configuration production
    if ($LASTEXITCODE -ne 0) { throw "Angular build failed" }
} finally {
    Pop-Location
}
Write-Host "  Angular build complete." -ForegroundColor Green

# ============================================================
# Step 2: Copy ONLY SPA files to wwwroot (NO images!)
# ============================================================
Write-Host "[2/4] Copying SPA files to wwwroot..." -ForegroundColor Yellow

# Ensure dist directory exists
if (-not (Test-Path $angularDist)) {
    throw "Angular dist not found at $angularDist"
}

# Copy SPA core files only (JS, CSS, HTML, favicon)
$spaExtensions = @("*.js", "*.css", "*.html", "*.ico")
foreach ($ext in $spaExtensions) {
    Get-ChildItem -Path $angularDist -Filter $ext | ForEach-Object {
        Copy-Item $_.FullName -Destination $wwwroot -Force
        Write-Host "  Copied: $($_.Name)" -ForegroundColor Gray
    }
}

# Copy the Angular app's banner images (needed by the Angular app)
$bannerSrc = Join-Path $angularDist "images\banner"
$bannerDest = Join-Path $wwwroot "images\banner"
if (Test-Path $bannerSrc) {
    if (-not (Test-Path $bannerDest)) {
        New-Item -ItemType Directory -Path $bannerDest -Force | Out-Null
    }
    Copy-Item "$bannerSrc\*" -Destination $bannerDest -Force
    Write-Host "  Copied: images/banner/*" -ForegroundColor Gray
}

# CRITICAL: DO NOT copy images/products/ from Angular dist to wwwroot!
# Product images in wwwroot/images/products/ are uploaded via admin panel
# and use UUID filenames managed by FileUploadService.

Write-Host "  SPA files copied (images/products/ intentionally skipped)." -ForegroundColor Green

# ============================================================
# Step 3: Publish .NET project + Generate EF migration script
# ============================================================
Write-Host "[3/4] Publishing .NET project + Generating EF migration script ($Configuration)..." -ForegroundColor Yellow
$csprojPath = Join-Path $root "SupermarketMock\SupermarketMock\SupermarketMock.csproj"

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

# --- Ensure dotnet-ef global tool is installed ---
Write-Host "  Checking dotnet-ef tool..." -ForegroundColor Gray
$efInstalled = dotnet tool list -g 2>$null | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Host "  dotnet-ef not found. Installing..." -ForegroundColor Yellow
    & dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) { throw "Failed to install dotnet-ef" }
}

# Resolve the full path to dotnet-ef.exe to avoid PATH refresh issues
# in the current PowerShell session after a fresh install.
$dotnetEfPath = Join-Path $env:USERPROFILE ".dotnet\tools\dotnet-ef.exe"
if (-not (Test-Path $dotnetEfPath)) {
    # Fallback: rely on dotnet resolving the tool from its known paths
    $dotnetEfArgs = @("ef", "migrations", "script", "--idempotent",
                       "-o", "$publishDir\migrate.sql",
                       "--project", $csprojPath,
                       "--startup-project", $csprojPath)
    & dotnet @dotnetEfArgs
} else {
    & $dotnetEfPath migrations script --idempotent `
        -o "$publishDir\migrate.sql" `
        --project $csprojPath `
        --startup-project $csprojPath
}
if ($LASTEXITCODE -ne 0) { throw "EF Core migrations script failed" }

$migrateSqlPath = Join-Path $publishDir "migrate.sql"
if (-not (Test-Path $migrateSqlPath)) { throw "migrate.sql was not generated" }
Write-Host "  migrate.sql generated: $migrateSqlPath" -ForegroundColor Gray

# --- Publish .NET project ---
& dotnet publish $csprojPath -c $Configuration -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }
Write-Host "  Publish complete: $publishDir" -ForegroundColor Green

# ============================================================
# Step 4: Create deploy.zip
# ============================================================
Write-Host "[4/4] Creating deploy.zip..." -ForegroundColor Yellow

if (Test-Path $deployZip) {
    Remove-Item $deployZip -Force
}

# Compress the published output
Compress-Archive -Path "$publishDir\*" -DestinationPath $deployZip -Force
$zipSize = [math]::Round((Get-Item $deployZip).Length / 1MB, 1)
Write-Host "  deploy.zip created ($zipSize MB)" -ForegroundColor Green

# ============================================================
# Cleanup
# ============================================================
Write-Host ""
Write-Host "Cleaning up publish directory..." -ForegroundColor Yellow
Remove-Item $publishDir -Recurse -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Done! deploy.zip ready at:" -ForegroundColor Cyan
Write-Host "  $deployZip" -ForegroundColor White
Write-Host "" -ForegroundColor Cyan
Write-Host "  Next: Deploy via Azure MCP or Azure CLI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
