<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE stylesheet [
  <!ENTITY space "<xsl:text> </xsl:text>">
  <!ENTITY cr "<xsl:text>
</xsl:text>">
]>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ddr="urn:ddrmenu">
	<xsl:output method="html"/>
	<xsl:param name="ControlID" />
	<xsl:param name="Options" />
  
  <xsl:template match="/*">
    <xsl:apply-templates select="root" />
  </xsl:template>
  
	<xsl:template match="root">
		<div id="{$ControlID}">
      <ul class="dnnadminmega">
        <xsl:apply-templates select="node">
          <xsl:with-param name="nodeType">root</xsl:with-param>
        </xsl:apply-templates>
        <li id="dnnCommonTasks" class="root">
          <a href="#">
            <xsl:value-of select ="ddr:GetString('//Modules','~/App_GlobalResources/GlobalResources.resx')"/>
          </a>
          <div class="megaborder"></div>
        </li>
        <li id="dnnCurrentPage" class="root">
          <a href="#">
            <xsl:value-of select ="ddr:GetString('//Pages','~/App_GlobalResources/GlobalResources.resx')"/>
          </a>
          <div class="megaborder"></div>
        </li>
        <li id="dnnOtherTools" class="root">
          <a href="#">
            <xsl:value-of select ="ddr:GetString('//Tools','~/App_GlobalResources/GlobalResources.resx')"/>
          </a>
          <div class="megaborder"></div>
        </li>			
			</ul>
		</div>
	</xsl:template>

  <xsl:template match="node">
    <xsl:param name="nodeType" />
    <xsl:param name="parent" />    
		<li>
      <xsl:choose>
        <xsl:when test="$nodeType='leaf'">
          <xsl:variable name="columns" select="3" />
          <xsl:variable name="nodeClass">
            <xsl:value-of select="$nodeType"/> 
            <xsl:if test="@separator = 1"> separator</xsl:if>
            <xsl:if test="@selected = 1"> selected</xsl:if>
            <xsl:if test="@breadcrumb = 1"> breadcrumb</xsl:if>
            <xsl:text> child-</xsl:text><xsl:value-of select="position()"/>
            <xsl:text> col</xsl:text><xsl:value-of select="(position() -1)  mod $columns"/>
          </xsl:variable>
          <xsl:attribute name="class">
            <xsl:value-of select="$nodeClass"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">
            <xsl:value-of select="$nodeType"/>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:call-template name="anchor" />

      <xsl:if test="node">
        <xsl:choose>
          <xsl:when test="$nodeType = 'root'">
            <div class="megaborder">
              <ul>
                <li class="category">
                  <span>
                    <xsl:value-of select="@text"/>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select ="ddr:GetString('MenuTitleSuffix.Text','~/admin/ControlPanel/App_LocalResources/RibbonBar.ascx.resx')"/>
                  </span>
                  <UL>
                    <xsl:apply-templates select="node[@enabled=1]">
                      <xsl:with-param name="nodeType" >leaf</xsl:with-param>
                    </xsl:apply-templates>                        
                  </UL>
                </li>
                <xsl:apply-templates select="node[@enabled=0]">
                  <xsl:with-param name="nodeType" >category</xsl:with-param>
                </xsl:apply-templates>
              </ul>
            </div>            
          </xsl:when>
          <xsl:otherwise>
            <ul>
              <xsl:apply-templates select="node">
                <xsl:with-param name="nodeType" >leaf</xsl:with-param>
              </xsl:apply-templates>
            </ul>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>      
    </li>
	</xsl:template>

  <xsl:template name="anchor">
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
        <span>
          <xsl:value-of select="@text" />
        </span>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
