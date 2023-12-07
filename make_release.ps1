$mod_manifest = Get-Content -Raw manifest.json | ConvertFrom-Json
$mod_version = $mod_manifest.version_number
$version_exists = Test-Path ("./releases/JetpackWarning" + $mod_version + ".zip")

if ( !$version_exists ) {
    $mod_compress = @{
        Path             = "./bin/Release/netstandard2.1/JetpackWarning.dll", "./assets/icon.png", "./manifest.json", "./README.md", "./CHANGELOG.md"
        CompressionLevel = "Fastest"
        DestinationPath  = "./releases/JetpackWarning" + $mod_version + ".zip"
    }

    Compress-Archive @mod_compress
    Write-Output ("File JetpackWarning" + $mod_version + ".zip created")
}
else {
    Write-Output ("File JetpackWarning" + $mod_version + ".zip already exists")
}

cmd /c pause