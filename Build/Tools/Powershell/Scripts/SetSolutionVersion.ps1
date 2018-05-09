Param([parameter(Mandatory=$true)]$folder,[parameter(Mandatory=$true)]$version)

try
{
	$solutionPattern = "[0-9]+(\.([0-9]+|\*)){1,3}"

    $foundFiles = get-childitem $folder -recurse -include *SolutionInfo.cs

    foreach( $file in $foundFiles )
    {
        attrib -R $file
        (Get-Content $file) | ForEach-Object {
                % {$_ -replace $solutionPattern, $version }
            } | Set-Content $file
    }
}
catch
{
    Write-Output "Error: Failed updating SolutionInfo"
    Write-Output $error[0]
    Throw
}
