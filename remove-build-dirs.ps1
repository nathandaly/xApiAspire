# Script to remove build and cache directories from git tracking

Write-Host "Checking git status..."
$status = git status 2>&1
if ($LASTEXITCODE -ne 0 -and $status -match "not a git repository") {
    Write-Host "Git repository not initialized. Initializing..."
    git init
}

Write-Host "`nFinding tracked build directories..."
$trackedFiles = git ls-files 2>&1
if ($trackedFiles) {
    $buildDirs = @()
    
    # Check for bin/ and obj/ directories
    $binDirs = $trackedFiles | Where-Object { $_ -match "\\bin\\" -or $_ -match "/bin/" } | ForEach-Object { 
        $dir = if ($_ -match "(.+[/\\])bin[/\\]") { $matches[1] + "bin" } else { $null }
        if ($dir) { $dir }
    } | Select-Object -Unique
    
    $objDirs = $trackedFiles | Where-Object { $_ -match "\\obj\\" -or $_ -match "/obj/" } | ForEach-Object { 
        $dir = if ($_ -match "(.+[/\\])obj[/\\]") { $matches[1] + "obj" } else { $null }
        if ($dir) { $dir }
    } | Select-Object -Unique
    
    Write-Host "`nRemoving from git tracking (keeping local files)..."
    
    # Remove bin directories
    foreach ($dir in $binDirs) {
        if (Test-Path $dir) {
            Write-Host "Removing: $dir"
            git rm -r --cached --ignore-unmatch $dir 2>&1 | Out-Null
        }
    }
    
    # Remove obj directories
    foreach ($dir in $objDirs) {
        if (Test-Path $dir) {
            Write-Host "Removing: $dir"
            git rm -r --cached --ignore-unmatch $dir 2>&1 | Out-Null
        }
    }
    
    # Remove specific known directories
    $knownDirs = @(
        "xApiApp.ApiService/bin",
        "xApiApp.ApiService/obj",
        "xApiApp.AppHost/bin",
        "xApiApp.AppHost/obj",
        "xApiApp.ServiceDefaults/bin",
        "xApiApp.ServiceDefaults/obj",
        "xApiApp.Web/bin",
        "xApiApp.Web/obj"
    )
    
    foreach ($dir in $knownDirs) {
        if (Test-Path $dir) {
            Write-Host "Removing: $dir"
            git rm -r --cached --ignore-unmatch $dir 2>&1 | Out-Null
        }
    }
    
    Write-Host "`nDone! Build directories removed from git tracking."
    Write-Host "`nCurrent git status:"
    git status --short | Select-Object -First 20
} else {
    Write-Host "No files tracked in git yet, or git repository not initialized."
}

