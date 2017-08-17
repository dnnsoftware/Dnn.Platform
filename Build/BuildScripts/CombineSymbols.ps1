Param(
    [parameter(Mandatory=$true)][string]$checkoutDir,
    [parameter(Mandatory=$false)][string]$symbolsDir = "Dnn.Symbols"
	)

$baseFolder   = ([System.IO.FileInfo]"$checkoutDir\$symbolsDir").FullName
$binFolder    = ([System.IO.FileInfo]"$baseFolder\bin").FullName
$resourcePath = ([System.IO.FileInfo]"$baseFolder\Resources.zip").FullName

if (Test-Path "$binFolder")
{
    Remove-Item "$binFolder" -Recurse
}
    
if (Test-Path "$resourcePath")
{
    Remove-Item "$resourcePath"
}
    
$artifactsPath = ([System.IO.FileInfo]"$checkoutDir\Artifacts\").FullName
if (!(Test-Path $artifactsPath -PathType Container))
{
    md $artifactsPath
}
    
# extract all resources files to bin folder
Get-ChildItem Resources.zip -Recurse | Foreach-Object {
    Expand-Archive -Force -LiteralPath "$($_.FullName)" -DestinationPath "$baseFolder"
}

# compress all files in bin folder
Compress-Archive -Force -Path "$binFolder" -CompressionLevel Optimal -DestinationPath "$resourcePath"

Get-ChildItem *Symbols.zip |  Foreach-Object {
    # add teh resources file back to the symbols
    Compress-Archive -Force -LiteralPath "$resourcePath" -Update -CompressionLevel Optimal -DestinationPath "$($_.FullName)"
    Copy-Item "$($_.FullName)" "$artifactsPath"
}
