# Overview

The Deployment Framework includes a custom ESB resolver (BTDF-SSO) that can be selected in the Itinerary Designer.  It lets you dynamically resolve settings via SSO at runtime that originate from your Excel settings spreadsheet (SettingsFileGenerator.xml).

> **NOTE**: If you are using the BTDF-SSO resolver, then you must do a Custom install of the Deployment Framework on your BizTalk servers.  This is due to the fact that the resolver DLL must be installed on the servers and registered in the Esb.config file.

To use the BTDF ESB Resolver:

1) **Complete the steps in Deploy Configuration Settings into SSO**
The steps in Deploy Configuration Settings into SSO are all prerequisites to this topic.

2) **Create or edit an itinerary in the Itinerary Designer and, on an itinerary service object, add a resolver**
See the BizTalk ESB Toolkit documentation for details on how to configure an itinerary service with a resolver.  An example is shown in the following image:

![Itinerary](_static\Itinerary.png)

3) **Choose "Deployment Framework for BizTalk SSO Resolver Extension" as the Resolver Implementation**
This will add the AffiliateAppName property to the properties list.

4) **Fill in the AffiliateAppName, Transport Location and Transport Type fields**
The AffiliateAppName value is the name of the SSO affiliate application into which your project's settings are loaded.  This is typically the value of the ProjectName element in your deployment project file.

The values of Transport Location and Transport Name each correspond to the names of settings rows in your settings spreadsheet.  The associated values in the spreadsheet should define the destination for the resolver (e.g. C:\\MyApp\\Output and FILE).

The following image shows a typical resolver configuration:

![ItineraryProperties](_static\ItineraryProperties.png)

And the associated configuration in the settings spreadsheet:

![ESBResolverSettings](_static\\ESBResolverSettings.png)