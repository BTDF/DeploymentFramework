// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// This MSBuild task uses the BizTalk 2006 R2 WCF publishing libraries to export schemas
    /// and definition files for a WCF Web service exposed from BizTalk.
    /// 
    /// Published services contain copies of all of the schemas related to the service in a
    /// set of XSD files on disk.  This means that every time one of those schemas changes,
    /// the service must be re-published.  That's where this task comes in.  It removes the
    /// need to store copies of the published schema files because it can re-publish them
    /// on demand.
    /// 
    /// This task requires that the service be manually exported once with the WCF Service
    /// Publishing Wizard.  Among other files, the export process creates a definition file called
    /// WcfServiceDescription.xml, which captures the settings used in the wizard.  The file
    /// is saved to wwwroot\[service]\App_Data\Temp.  The task requires this definition
    /// file to reproduce the original export.
    /// 
    /// There are only four files that should be preserved in source control from the published
    /// service: [service].svc and web.config in wwwroot\[service] and the service description
    /// and binding XML files in wwwroot\[service]\App_Data\Temp.  Everything in App_Data can
    /// be recreated with this task.
    /// 
    /// This task does not directly reference the BizTalk assemblies.  The WcfPublishingAssemblyName
    /// property can override the .NET assembly name, which defaults to that of BizTalk 2006 R2.
    /// </summary>
    public class PublishWcfServiceArtifacts : Task
    {
        private const string BizTalkWcfPublishingAssembly =
            "Microsoft.BizTalk.Adapter.Wcf.Publishing, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        private string _outputPath;
        private string _serviceDescriptionPath;
        private string _wcfPublishingAssembly = BizTalkWcfPublishingAssembly;

        /// <summary>
        /// Path to the service home directory. App_Data will be created below this folder.
        /// </summary>
        [Required]
        public string OutputPath
        {
            get { return _outputPath; }
            set { _outputPath = value; }
        }

        /// <summary>
        /// Path to the WcfServiceDescription.xml file saved from a manual publication of the service.
        /// </summary>
        [Required]
        public string ServiceDescriptionPath
        {
            get { return _serviceDescriptionPath; }
            set { _serviceDescriptionPath = value; }
        }

        /// <summary>
        /// The complete assembly name (name, version, culture, public key) of the BizTalk
        /// WCF Publishing assembly. Default value is correct for BizTalk 2006 R2.
        /// </summary>
        public string WcfPublishingAssemblyName
        {
            get { return _wcfPublishingAssembly; }
            set { _wcfPublishingAssembly = value; }
        }

        public override bool Execute()
        {
            try
            {
                //
                // This code is based on Microsoft.BizTalk.Adapter.Wcf.Publishing.Publisher.Export().
                // Since most of the classes and methods are private, this code has to use reflection
                // to do most of the work.  It was developed and tested against the original release
                // of BizTalk Server 2006 R2.
                //

                // First, read in the service description file saved from a manual run of the
                // WCF Service Publishing Wizard.
                Type wsdType = Type.GetType(
                    "Microsoft.BizTalk.Adapter.Wcf.Publishing.Description.WcfServiceDescription, " + _wcfPublishingAssembly, true);
                object svcDescription =
                    wsdType.InvokeMember("LoadXml", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, new object[] { _serviceDescriptionPath });

                Type wmtType = Type.GetType(
                    "Microsoft.BizTalk.Adapter.Wcf.Publishing.Description.WcfMessageType, " + _wcfPublishingAssembly, true);

                IDictionary uniqueMessageTypes = null;

                // Build a list of the message types defined in the service description file.
                Type publisherType = Type.GetType(
                    "Microsoft.BizTalk.Adapter.Wcf.Publishing.Publisher, " + _wcfPublishingAssembly, true);
                uniqueMessageTypes =
                    publisherType.InvokeMember("GetUniqueMessageTypes", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[] { svcDescription }) as IDictionary;

                // Create a new instance of the WebServiceImplementation class, which will carry
                // out the save to disk later on.
                Type wsiType = Type.GetType(
                    "Microsoft.BizTalk.Adapter.Wcf.Publishing.Implementation.WebServiceImplementation, " + _wcfPublishingAssembly, true);
                object wsi = wsiType.InvokeMember("", BindingFlags.CreateInstance, null, null, null);

                // Create a BtsServiceDescription object based on the service description.
                Type bsdeType = Type.GetType(
                    "Microsoft.BizTalk.Adapter.Wcf.Publishing.Exporters.BtsServiceDescriptionExporter, " + _wcfPublishingAssembly, true);
                object bsd =
                    bsdeType.InvokeMember("Export", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, new object[] { svcDescription });

                // Set the BtsServiceDescription object into the WebServiceImplementation object's
                // BtsServiceDescription property.
                wsiType.InvokeMember(
                    "BtsServiceDescription", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, wsi, new object[] { bsd });

                // Add the WCF-default DataContractSerializer schema to the WebServiceImplementation object.
                MethodInfo asstiMethod =
                    publisherType.GetMethod("AddSerializationSchemaToImplementation", BindingFlags.Static | BindingFlags.NonPublic);
                asstiMethod.Invoke(null, new object[] { wsi });

                MethodInfo pwmtMethod =
                    publisherType.GetMethod("ProcessWcfMessageType", BindingFlags.Static | BindingFlags.NonPublic);

                Log.LogMessage(MessageImportance.Normal, "Exporting WCF service artifacts from {0}...", _serviceDescriptionPath);

                // For each unique message type defined in the service description, extract the schemas
                // into the WebServiceImplementation object.
                foreach (object type in uniqueMessageTypes.Values)
                {
                    pwmtMethod.Invoke(null, new object[] { wsi, type });
                }

                // Now that we have everything we need in the WebServiceImplementation object,
                // it can save everything out to disk.
                MethodInfo stfMethod =
                    wsiType.GetMethod("SaveToFolder", BindingFlags.Instance | BindingFlags.Public);
                stfMethod.Invoke(wsi, new object[] { _outputPath });

            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            return true;
        }
    }
}
