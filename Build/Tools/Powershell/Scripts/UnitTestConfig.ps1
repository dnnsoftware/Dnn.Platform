Param(
[parameter(Mandatory=$true)][String]$appConfigPath,
[parameter(Mandatory=$true)][String]$siteUrl,
[parameter(Mandatory=$true)][String]$sitePath,
[parameter(Mandatory=$true)][String]$testPath,
[parameter(Mandatory=$true)][String]$dbName,
[parameter(Mandatory=$true)][String]$objQual
)

$connectionString = "Server=(local);Database=" + $dbName + ";Integrated Security=True;Application Name=" + $dbName;

attrib -R $appConfigPath
$xml = [xml](get-content $appConfigPath)
$root = $xml.get_DocumentElement()

foreach($n in $xml.selectnodes("/configuration/connectionStrings/add"))
{
	switch($n.name)
	{
		"SiteSqlServer" { $n.connectionString = $connectionString }
	}
}

foreach($n in $xml.selectnodes("/configuration/appSettings/add"))
{
	switch($n.key)
	{
		"SiteSqlServer" { $n.value = $connectionString }
	}
}

foreach($n in $xml.selectnodes("/configuration/appSettings/add"))
{
	switch($n.key)
	{
		"SiteURL" { $n.value = $siteUrl }
	}
}

foreach($n in $xml.selectnodes("/configuration/appSettings/add"))
{
	switch($n.key)
	{
		"DefaultPhysicalAppPath" { $n.value = $sitePath }
	}
}

foreach($n in $xml.selectnodes("/configuration/appSettings/add"))
{
	switch($n.key)
	{
		"UrlTestFilesPath" { $n.value = $testPath }
	}
}

if ($objQual -ne "")
{
	foreach($n in $xml.selectnodes("/configuration/appSettings/add"))
	{
		switch($n.key)
		{
			"objectQualifier" { $n.value = $objQual }
		}
	}

    foreach($n in $xml.selectnodes("/configuration/dotnetnuke/data/providers/add"))
    {
    	$n.objectQualifier = $objQual
    }
}

$xml.Save($appConfigPath)
