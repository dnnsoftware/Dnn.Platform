<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml" omit-xml-declaration="yes" />
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>
    <xsl:template match="connectionStrings">
        <connectionStrings>
            <add name="SiteSqlServer" connectionString="{ConnectionString}" providerName="System.Data.SqlClient" />
        </connectionStrings>
    </xsl:template>
    <xsl:template match="add[@name='SqlDataProvider']">
        <add name="SqlDataProvider" type="DotNetNuke.Data.SqlDataProvider, DotNetNuke" connectionStringName="SiteSqlServer" upgradeConnectionString="" providerPath="~\Providers\DataProviders\SqlDataProvider\" objectQualifier="{ObjectQualifier}" databaseOwner="{DbOwner}" />
    </xsl:template>
</xsl:stylesheet>
