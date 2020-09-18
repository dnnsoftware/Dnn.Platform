<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html"/>
	<xsl:param name="ControlID" />
	<xsl:param name="Options" />
	<xsl:template match="/*">
		<xsl:apply-templates select="root" />
	</xsl:template>
	<xsl:template match="root">
		<script type="text/javascript">
			DDR.Menu.registerMenu('<xsl:value-of select="$ControlID"/>', <xsl:value-of select="$Options"/>);
		</script>
		<div id="{$ControlID}">
			<ul>
				<xsl:apply-templates select="node" />
			</ul>
		</div>
	</xsl:template>
	<xsl:template match="node">
		<li nid="{@id}">
			<xsl:choose>
				<xsl:when test="@separator = 1">
					<xsl:attribute name="class">separator</xsl:attribute>
				</xsl:when>
				<xsl:when test="@selected = 1 and @breadcrumb = 1">
					<xsl:attribute name="class">selected breadcrumb</xsl:attribute>
				</xsl:when>
				<xsl:when test="@selected = 1">
					<xsl:attribute name="class">selected</xsl:attribute>
				</xsl:when>
				<xsl:when test="@breadcrumb = 1">
					<xsl:attribute name="class">breadcrumb</xsl:attribute>
				</xsl:when>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="@enabled = 1">
					<a href="{@url}">
						<xsl:if test="@icon">
							<img src="{@icon}" />
						</xsl:if>
						<span>
							<xsl:value-of select="@text" />
						</span>
					</a>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="@icon">
						<img src="{@icon}" />
					</xsl:if>
					<span>
						<xsl:value-of select="@text" />
					</span>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="node">
				<ul nid="{@id}">
					<xsl:apply-templates select="node" />
				</ul>
			</xsl:if>
		</li>
	</xsl:template>
</xsl:stylesheet>
