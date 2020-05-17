<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html"/>
    <xsl:template match="/*">
        <xsl:apply-templates select="root" />
    </xsl:template>
    
    <xsl:template match="root">
        <xsl:apply-templates select="node" mode="root" />
    </xsl:template>
    
    <xsl:template match="node" mode="root">
        <div class="dnnActionMenu">
          <div class="dnnActionMenuTag">
            <xsl:choose>
                <xsl:when test="@enabled = 1">
                    <a href="{@url}">
                        <xsl:if test="@icon">
                            <img src="{@icon}" />
                        </xsl:if>
                        <xsl:value-of select="@text" />
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
          </div>
        <xsl:if test="node">
            <ul class="dnnActionMenuBody">
                <xsl:apply-templates select="node" />
            </ul>
        </xsl:if>
        </div>

    </xsl:template>

    <xsl:template match="node">
        <li>
            <xsl:attribute name="class">
                <xsl:if test="@first = 1">first</xsl:if>
                <xsl:if test="@last = 1"><xsl:text>&#32;</xsl:text>last</xsl:if>
                <xsl:if test="@selected = 1"><xsl:text>&#32;</xsl:text>selected</xsl:if>
                <xsl:if test="position() mod 4 = 0"><xsl:text>&#32;</xsl:text>rowBreak</xsl:if>
            </xsl:attribute>
            <xsl:choose>
                <xsl:when test="@enabled = 1">
                    <a href="{@url}">
                        <xsl:if test="@icon">
                            <img src="{@icon}" />
                        </xsl:if>
                        <xsl:value-of select="@text" />
                    </a>
                </xsl:when>
                <xsl:otherwise>
                    <span>
                        <xsl:if test="@icon">
                            <img src="{@icon}" />
                        </xsl:if>
                        <xsl:value-of select="@text" />
                    </span>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="node">
                <ul>
                    <xsl:attribute name="class">
                        <xsl:if test="@first = 1">first</xsl:if>
                        <xsl:if test="@last = 1"><xsl:text>&#32;</xsl:text>last</xsl:if>
                    </xsl:attribute>
                    <xsl:apply-templates select="node" />
                </ul>
            </xsl:if>
        </li>
    </xsl:template>
</xsl:stylesheet>
