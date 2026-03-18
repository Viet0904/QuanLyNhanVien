$files = Get-ChildItem -Path "Database" -Filter "*.sql"
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    $utf8Bom = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllText($f.FullName, $content, $utf8Bom)
    Write-Host "Fixed: $($f.Name)"
}
Write-Host "`nDone! All SQL files now have UTF-8 BOM."
