# Sample App

The ESBToolkitSSOResolver sample demonstrates how to use the Deployment Framework for BizTalk with an ESB Toolkit itinerary that resolves adapter information at runtime from settings stored in SSO.

**NOTE:** This sample requires BizTalk 2013 R2 or newer with the ESB Toolkit installed and configured.

## What this Sample Does

The ESBToolkitSSOResolver sample picks up a file from a file system folder and runs it through the ESB Toolkit's ItinerarySelectReceiveXml pipeline in order to assign an itinerary named ESBToolkitSSOResolver.  The itinerary calls the POToInvoice map to transform the input message, then uses the Deployment Framework for BizTalk SSO Resolver to dynamically configure an off-ramp, which then writes the transformed file to another file system folder.

## How this Sample is Designed and Why

This sample demonstrates deployment of a single BizTalk assembly containing both schemas and a map, a custom ESB Toolkit itinerary and the Deployment Framework for BizTalk SSO Resolver.  It uses the Deployment Framework for BizTalk's environment-specific configuration features to dynamically transform a template bindings XML file into an environment-specific bindings file (see BasicMasterBindings sample).  Building on the same configuration features, it dynamically reads settings from SSO at runtime to configure the ESB off-ramp.

The Excel settings spreadsheet, SettingsFileGenerator.xml, contains a couple of settings used to configure the off-ramp: InvoiceOutputPath and InvoiceOutputTransport.  During deployment a particular environment's setting values are loaded into SSO, and the ESB Resolver reads them at runtime using the application's name (the value of the ProjectName property in the deployment project file).

One extensibility feature demonstrated in the sample's Deployment.btdfproj project file is how to package additional files into the MSI so that they are deployed to the server along with the application binaries and other project files.  The CustomRedist target is called during MSI packaging, and in the sample it copies the files in the TestFiles folder into the MSI staging folder (defined by the $(RedistDir) MSBuild property).

Another extensibility feature demonstrated in the project file is how to create a file system folder at deployment time -- in this case the output folder.  The CustomDeployTarget is called during application deployment, and in the sample it creates the output folder.  The folder path is automatically pulled from the settings spreadsheet (named InvoiceOutputPath) into an MSBuild property via the PropsFromEnvSettings ItemGroup.

## Where to Find this Sample

\<ESBResolverInstallFolder\>\\Samples\\ESBToolkitSSOResolver

**NOTE:** After upgrading the solution to BizTalk 2020, you must upgrade the ESBToolkitSSOResolver.Itineraries project to target .NET Framework 4.6.

## Building and Deploying this Sample

1. Open the solution file ESBToolkitSSOResolver.sln in Visual Studio.
2. Build the solution and ensure that the build succeeded
3. Deploy the application using the Deployment Framework's Deploy command
4. The Deployment Framework will create an input folder at C:\\DeploymentFrameworkForBizTalk\\Samples\\ESBToolkitSSOResolver\\In
5. Confirm that the deployment process succeeded and the application is now running in BizTalk (see the Visual Studio Output window and BizTalk Admin Console)
6. If desired, package the application into an MSI using the Deployment Framework's Build Server MSI command

## Running this Sample

1. Copy the file SamplePOInput.xml from TestFiles into C:\\DeploymentFrameworkForBizTalk\\Samples\\ESBToolkitSSOResolver\\In
2. Watch for an output XML file to appear in C:\\DeploymentFrameworkForBizTalk\\Samples\\ESBToolkitSSOResolver\\Out

## Removing this Sample

1. With the ESBToolkitSSOResolver.sln open in Visual Studio, use the Deployment Framework's Undeploy command
2. Delete the folder C:\\DeploymentFrameworkForBizTalk\\Samples\\ESBToolkitSSOResolver