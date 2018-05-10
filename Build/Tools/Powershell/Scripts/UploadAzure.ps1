<#
.SYNOPSIS
   Publishes a file to Azure Blob Storage.
.DESCRIPTION
   Publishes a file to Azure Blob Storage
.PARAMETER filename
   The full filename to be uploaded to Azure Storage
.PARAMETER accountname
   The name of the storage account where the file will be stored.
.PARAMETER accountkey
   The key used to access the specified storage account.
.PARAMETER container
   The name of the container where the blog will be stored.
.EXAMPLE
   Publish-AzureBlob -filename c:\somefile.pdf -accountname myaccount -accountkey somekey -container uploads
#>

Param(
    [Parameter(Mandatory = $True, ValueFromPipeline = $True, ValueFromPipelinebyPropertyName = $True)]
    [string]$filename,
    [Parameter(Mandatory = $false)]
    [string]$foldername,
    [Parameter(Mandatory = $True)]
    [string]$accountname,
    [Parameter(Mandatory = $True)]
    [string]$accountKey,
    [string]$container
)

Begin {
    $azureContext = New-AzureStorageContext -StorageAccountName $accountname -StorageAccountKey $accountkey
}

Process {
    $file = Get-ChildItem $filename

    if ([string]::IsNullOrEmpty($foldername)) {
        $blobname = $file.name
    }
    else {
        $blobname = "{0}\{1}" -f $foldername, $file.name
    }

    Set-AzureStorageBlobContent -File $file -Blob $blobname -Container $container -Context $azureContext -Force
}
