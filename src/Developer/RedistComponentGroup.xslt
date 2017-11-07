<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
    xmlns="http://schemas.microsoft.com/wix/2006/wi"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:helpers="urn:schemas-btdf:xslt"
    exclude-result-prefixes="msxsl wix helpers">

  <xsl:output method="xml" indent="yes"/>

  <xsl:variable name="EnvironmentSettingsDirectoryId" select="/wix:Wix/wix:Fragment/wix:DirectoryRef/wix:Directory[@Name='EnvironmentSettings']/@Id" />
  <xsl:variable name="DeployResultsDirectoryId" select="/wix:Wix/wix:Fragment/wix:DirectoryRef/wix:Directory[@Name='DeployResults']/@Id" />
  <xsl:variable name="DeploymentDirectoryId" select="/wix:Wix/wix:Fragment/wix:DirectoryRef/wix:Directory[@Name='Deployment']/@Id" />

  <xsl:template match="@* | node()">
      <xsl:copy>
          <xsl:apply-templates select="@* | node()"/>
      </xsl:copy>
  </xsl:template>
  
  <xsl:template match="wix:ComponentGroup[@Id='RedistComponentGroup']">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
        <xsl:if test="$EnvironmentSettingsDirectoryId">
        <xsl:element name="Component">
          <xsl:attribute name="Id">cmp<xsl:value-of select="helpers:GetId()"/></xsl:attribute>
          <xsl:attribute name="Directory"><xsl:value-of select="$EnvironmentSettingsDirectoryId"/></xsl:attribute>
          <xsl:attribute name="Guid"><xsl:value-of select="helpers:GetGuid()"/></xsl:attribute>
          <xsl:attribute name="KeyPath">yes</xsl:attribute>
          <xsl:element name="RemoveFile">
            <xsl:attribute name="Name">*.xml</xsl:attribute>
            <xsl:attribute name="On">uninstall</xsl:attribute>
            <xsl:attribute name="Id"><xsl:value-of select="$EnvironmentSettingsDirectoryId"/></xsl:attribute>
          </xsl:element>
        </xsl:element>
        </xsl:if>
        <xsl:element name="Component">
          <xsl:attribute name="Id">cmp<xsl:value-of select="helpers:GetId()"/></xsl:attribute>
          <xsl:attribute name="Directory"><xsl:value-of select="$DeploymentDirectoryId"/></xsl:attribute>
          <xsl:attribute name="Guid"><xsl:value-of select="helpers:GetGuid()"/></xsl:attribute>
          <xsl:attribute name="KeyPath">yes</xsl:attribute>
          <xsl:element name="RemoveFile">
            <xsl:attribute name="Name">PortBindings.xml</xsl:attribute>
            <xsl:attribute name="On">uninstall</xsl:attribute>
            <xsl:attribute name="Id"><xsl:value-of select="$DeploymentDirectoryId"/></xsl:attribute>
          </xsl:element>
        </xsl:element>
      </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:File[contains(@Source,'SSOSettingsEditor.exe')]">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
        <xsl:processing-instruction name="if">$(var.CreateStartMenuShortcuts) ~= True And $(var.IncludeSSOEditorShortcut) ~= True</xsl:processing-instruction>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">SSOSettingsEditorShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Edit SSO Settings</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;$(var.BizTalkAppName)&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">INSTALLDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:processing-instruction name="endif" />
      </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:File[substring(@Source, (string-length(@Source) - string-length('nunit.exe')) + 1) = 'nunit.exe']">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
        <xsl:processing-instruction name="if">$(var.CreateStartMenuShortcuts) ~= True</xsl:processing-instruction>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">NUnitGuiShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Verify Deployment</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[INSTALLDIR]\$(var.DeploymentTest)&quot; /run</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">INSTALLDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:processing-instruction name="endif" />
      </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:File[substring(@Source, (string-length(@Source) - string-length('.chm')) + 1) = '.chm']">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
        <xsl:processing-instruction name="if">$(var.CreateStartMenuShortcuts) ~= True</xsl:processing-instruction>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">chm<xsl:value-of select="helpers:GetId()"/></xsl:attribute>
          <xsl:attribute name="Directory">BizShortCuts</xsl:attribute>
          <xsl:attribute name="Name"><xsl:value-of select="helpers:GetFileNameNoExt(self::node()/@Source)"/></xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:processing-instruction name="endif" />
      </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:File[contains(@Source,'UacElevate.exe')]">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
        <xsl:processing-instruction name="if">$(var.CreateStartMenuShortcuts) ~= True</xsl:processing-instruction>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">DeployShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCuts</xsl:attribute>
          <xsl:attribute name="Name">Deploy $(var.PackageDescription)</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server /t:LaunchServerDeployWizard $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">ReDeployShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCuts</xsl:attribute>
          <xsl:attribute name="Name">Redeploy $(var.PackageDescription)</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server /t:LaunchServerRedeployWizard $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">UnDeployShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCuts</xsl:attribute>
          <xsl:attribute name="Name">Undeploy $(var.PackageDescription)</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server /t:LaunchServerUndeployWizard $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">QuickDeployShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Quick Deploy</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server /t:LaunchServerQuickDeploy $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">BounceShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Bounce BizTalk</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server;Interactive=true /t:BounceBizTalk $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">TerminateInstancesShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Terminate Service Instances</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server;Interactive=true /t:TerminateServiceInstances $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">ImportBindingsShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Import Bindings</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server;Interactive=true /t:ImportBindings $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">PreprocessAndImportBindingsShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Preprocess and Import Bindings</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server;Interactive=true /t:PreprocessAndImportBindings $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:processing-instruction name="endif" />

        <xsl:processing-instruction name="if">$(var.CreateStartMenuShortcuts) ~= True And $(var.IncludeDeployRulesShortcut) ~= True</xsl:processing-instruction>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">RedeployRulesShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Redeploy Rules Policies and Vocabularies</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server;ExplicitlyDeployRulePoliciesOnDeploy=true;Interactive=true /t:DeployVocabAndRules $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:processing-instruction name="endif" />
        <xsl:processing-instruction name="if">$(var.CreateStartMenuShortcuts) ~= True And $(var.IncludeSSOEditorShortcut) ~= True</xsl:processing-instruction>
        <xsl:element name="Shortcut">
          <xsl:attribute name="Id">ImportSSOShortcut</xsl:attribute>
          <xsl:attribute name="Directory">BizShortCutsTools</xsl:attribute>
          <xsl:attribute name="Name">Update SSO from Settings Spreadsheet</xsl:attribute>
          <xsl:attribute name="Arguments">&quot;[MSBUILDPATH]&quot; &quot;/p:Configuration=Server;Interactive=true /t:DeploySSO $(var.ProjectFilename) [MSBUILDTOOLSVER]&quot;</xsl:attribute>
          <xsl:attribute name="WorkingDirectory">DEPLOYMENTDIR</xsl:attribute>
          <xsl:attribute name="Advertise">yes</xsl:attribute>
        </xsl:element>
        <xsl:processing-instruction name="endif" />
      </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:Component">
    <xsl:variable name="CurrentComponentDirectory" select="self::node()/@Directory"/>

    <xsl:choose>
      <xsl:when test="$CurrentComponentDirectory=$DeployResultsDirectoryId">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
        <xsl:element name="RemoveFile">
            <xsl:attribute name="Name">*.txt</xsl:attribute>
            <xsl:attribute name="On">uninstall</xsl:attribute>
            <xsl:attribute name="Id"><xsl:value-of select="$DeployResultsDirectoryId"/></xsl:attribute>
        </xsl:element>
      </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
      </xsl:copy>
      </xsl:otherwise>
      </xsl:choose>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="helpers">
    public System.String GetId()
    {
      return System.Guid.NewGuid().ToString("N");
    }
    public System.String GetGuid()
    {
      return System.Guid.NewGuid().ToString("B");
    }
    public System.String GetFileNameNoExt(string filename)
    {
      return System.IO.Path.GetFileNameWithoutExtension(filename);
    }
  </msxsl:script>

</xsl:stylesheet>
