// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using Microsoft.XLANGs.BaseTypes;
using log4net;

namespace BizTalkSample.Components
{
	/// <summary>
	/// Summary description for MyClass, which will be used by orchestrations and therefore
	/// must be in the GAC.  Note: consider using static methods where possible, to avoid 
	/// having to instantiate an instance within your orchestration.
	/// </summary>
	public class MyClass
	{
		public MyClass()
		{
			//
			// TODO: Add constructor logic here
			//
		}

      public static void Execute(XLANGMessage msg, ILog log)
      {
         // The XLANGMessage class has a RetrieveAs method that allows you to specify
         // a type that you would like to use to operate on the message here within your .net
         // code.  This can be an XmlReader, or a Stream, or an XmlDocument, or a strong .net type, etc.

         //XmlReader reader = (XmlReader) msg[0].RetrieveAs(typeof(XmlReader));
         XmlDocument doc = (XmlDocument) msg[0].RetrieveAs(typeof(XmlDocument));
			log.Info(doc.OuterXml);

			// Demonstrate retrieving an sso item.  In a component, we can get entire hash table.
			// In an orchestration, we will likely use the ReadValue override and get a single value.
            Hashtable ht = SSOSettingsFileManager.SSOSettingsFileReader.Read("DeploymentFramework.Samples.Advanced");
			string someValue = (string)ht["SomeAppConfigItem"];
			log.InfoFormat("SSO value read in component for SomeAppConfigItem: {0}",someValue);
			log.InfoFormat("SSO value read in component for AnotherAppConfigItem: {0}",ht["AnotherAppConfigItem"]);

			log.InfoFormat("SSO value read in component for NestedName.One: {0}",ht["NestedName.One"]);
			log.InfoFormat("SSO value read in component for NestedName.Two: {0}",ht["NestedName.Two"]);

			StackTrace stack = new StackTrace(true);
			StackFrame frame = stack.GetFrame(0);
			log.Info("Stack with line numbers if PDB was deployed: " + frame.ToString());
      }

	}
}
