<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
	<xsl:template match="/">
	    <LogCollection>
		    <logs>
			    <xsl:for-each select="/LogCollection/logs/log">
				    <xsl:sort select="@LogCreateDateNum" data-type="number" order="descending"/>
				    <xsl:copy-of select="."/>
			    </xsl:for-each>
		    </logs>
		</LogCollection>
	</xsl:template>
</xsl:stylesheet>
