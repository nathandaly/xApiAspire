# Cleanup script to remove build directories from git tracking

Write-Host "=== Git Build Directory Cleanup ===" -ForegroundColor Cyan
Write-Host ""

# Check if git is initialized
if (-not (Test-Path .git)) {
    Write-Host "Git repository not initialized. Initializing..." -ForegroundColor Yellow
    git init
    Write-Host ""
}

# Add .gitignore first if not already tracked
git add .gitignore 2>&1 | Out-Null

# Directories to remove from git tracking
$dirsToRemove = @(
    "xApiApp.ApiService/bin",
    "xApiApp.ApiService/obj",
    "xApiApp.AppHost/bin",
    "xApiApp.AppHost/obj",
    "xApiApp.ServiceDefaults/bin",
    "xApiApp.ServiceDefaults/obj",
    "xApiApp.Web/bin",
    "xApiApp.Web/obj"
)

Write-Host "Removing build directories from git tracking..." -ForegroundColor Yellow
Write-Host "(Local files will be kept, only removed from git)" -ForegroundColor Gray
Write-Host ""

foreach ($dir in $dirsToRemove) {
    if (Test-Path $dir) {
        Write-Host "  Removing: $dir" -ForegroundColor Green
        git rm -r --cached --ignore-unmatch $dir 2>&1 | Out-Null
    } else {
        Write-Host "  Skipping: $dir (not found)" -ForegroundColor DarkGray
    }
}

# Also remove any .vs, .idea, packages directories if they exist
$otherDirs = @(".vs", ".idea", "packages")
foreach ($dir in $otherDirs) {
    if (Test-Path $dir) {
        Write-Host "  Removing: $dir" -ForegroundColor Green
        git rm -r --cached --ignore-unmatch $dir 2>&1 | Out-Null
    }
}

Write-Host ""
Write-Host "=== Cleanup Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Current git status:" -ForegroundColor Yellow
git status --short

