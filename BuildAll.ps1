Push-Location
try {
	get-childitem -Directory | foreach {
		Write-Host "============ Building Component *** $_ *** ============"
		cd $_
		yarn install
		yarn run prepublishOnly
		cd ..
	}
} finally {
	Pop-Location
}
