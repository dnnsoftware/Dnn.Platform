Param(
    [parameter(Mandatory=$true)][string]$checkoutDir,
    [parameter(Mandatory=$false)][string]$symbolsDir = "Dnn.Symbols",
    [parameter(Mandatory=$false)][string]$symbolsName
	)

$baseFolder   = ([System.IO.FileInfo]"$checkoutDir\$symbolsDir").FullName
$binFolder    = ([System.IO.FileInfo]"$baseFolder\bin").FullName
$resourcePath = ([System.IO.FileInfo]"$baseFolder\Resources.zip").FullName

Write-Host "This script will Combine symbols under $baseFolder"

Push-Location
Set-Location $baseFolder
 
try
{
    if (Test-Path "$binFolder")
    {
        Write-Host "Deleting $binFolder"
        Remove-Item "$binFolder" -Recurse
    }

    if (Test-Path "$resourcePath")
    {
        Write-Host "Deleting $resourcePath"
        Remove-Item "$resourcePath"
    }

    $artifactsPath = ([System.IO.FileInfo]"$checkoutDir\Artifacts\").FullName
    if (!(Test-Path $artifactsPath -PathType Container))
    {
        Write-Host "Creating $artifactsPath"
        md $artifactsPath
    }

    # extract all resources files to bin folder
    Get-ChildItem "Resources.zip" -Recurse | Foreach-Object {
        Write-Host "Extrachting $($_.FullName)"
        Expand-Archive -Force -LiteralPath "$($_.FullName)" -DestinationPath "$baseFolder"
    }

    if (Test-Path "$binFolder")
    {
        # compress all files in bin folder
        Write-Host "Compressing content of $binFolder"
        Compress-Archive -Force -Path "$binFolder" -CompressionLevel Optimal -DestinationPath "$resourcePath"

        $symbolsZip = (Get-ChildItem "*Symbols.zip").FullName
        Write-Host "Adding $resourcePath to $symbolsZip"
        Compress-Archive -LiteralPath "$resourcePath" -Update -CompressionLevel Optimal -DestinationPath "$symbolsZip"

        if ($symbolsName)
        {
            Write-Host "Renaming $symbolsZip to $symbolsName"
            Rename-Item "$symbolsZip" "$symbolsName"
            $symbolsZip = $symbolsName
        }

        Write-Host "Copying $symbolsZip to $artifactsPath"
        Copy-Item "$symbolsZip" "$artifactsPath"
    }
    else
    {
       Write-Host "BIN folder doesn't exist: $binFolder"
    }
}
finally
{
    Pop-Location
}
