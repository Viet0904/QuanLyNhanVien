$files = Get-ChildItem -Path "Database" -Filter "*.sql"
foreach ($f in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $bom = "UTF-8 BOM"
    } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        $bom = "UTF-16 LE"
    } else {
        $bom = "No BOM (raw UTF-8 or ANSI)"
    }
    Write-Host "$($f.Name): $bom"
}
