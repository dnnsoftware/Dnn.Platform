<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ddr="urn:ddrmenu">
  <xsl:output method="html" />

  <xsl:template match="*">
    <xsl:apply-templates select="." mode="echo" />
  </xsl:template>
  <xsl:template match="*" mode="echo">
    <div style="margin-left:1em">
      &lt;<xsl:value-of select="name()" /><xsl:apply-templates select="@*" mode="attribute" /><xsl:choose>
        <xsl:when test="* or text()">&gt;<xsl:value-of select="ddr:EscapeXML(text())" /><xsl:apply-templates select="*" mode="echo" />&lt;/<xsl:value-of select="name()" />&gt;</xsl:when>
        <xsl:otherwise> /&gt;</xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  <xsl:template match="@*" mode="attribute"><xsl:text>&#32;</xsl:text><xsl:value-of select="name()" />="<xsl:value-of select="ddr:EscapeXML(.)" />"</xsl:template>
</xsl:stylesheet>
