
    [cmdletbinding()]
    param(
        $StorageAccountName = "dnnbuildstorage",
        $StorageAccountKey,
        $ContainerName = "dnn",
        $Source,
        $DestPrefix,
        $Force
    )

    $ctx = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey
    $container = Get-AzureStorageContainer -Name $ContainerName -Context $ctx

    $container.CloudBlobContainer.Uri.AbsoluteUri
    if ($container) {
        $filesToUpload = Get-ChildItem $Source -Recurse -File

        foreach ($x in $filesToUpload) {
            $targetPath = $($DestPrefix.Replace("\", "/") + "/" + $x)
            Write-Verbose "Uploading $($x.fullname) to $($container.CloudBlobContainer.Uri.AbsoluteUri + "/" + $targetPath)"
            Set-AzureStorageBlobContent -File $x.fullname -Container $container.Name -Blob $targetPath -Context $ctx -Force:$Force | Out-Null
        }
    }



