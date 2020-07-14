// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using log4net;
using log4net.helpers;

using log4net.Ext.Serializable;

namespace log4net.Ext.Serializable.UnitTest
{
	/// <summary>
	/// Summary description for TestSerializableLog.
	/// </summary>
	class TestSerializableLog
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{

			// Note - a registry key must exist under hklm/software for this string below
			string domain = "log4net.Ext.Serializable.UnitTest1.2.10";
			SLog log = (SLog)SLogManager.GetLogger(domain,typeof(TestSerializableLog));
			log.RegistryConfigurator();

			PropertiesCollectionEx props = new PropertiesCollectionEx();
			props.Set("TESTPROP","myprop");
			
			log.InfoFormat(props,"Hello world with myprop for a prop {0}",log.GetHashCode());
			((ILog)log).DebugFormat("Shouldn't see this - its a debug.");

			SLog log2 = (SLog)SLogManager.GetLogger(domain,typeof(TestSerializableLog));
			log2.Info("Hello world again - different SLog reference, same SLog?  Look for myprop prop " +  + log2.GetHashCode());

			MemoryStream stream = new MemoryStream();
			SoapFormatter formatter = new SoapFormatter();
			formatter.Serialize(stream,log);
			formatter.Serialize(stream,props);
			System.Text.Encoding encoding = System.Text.Encoding.UTF8;
			string ser  = encoding.GetString(stream.ToArray());
			//log.Info("log looks like: " + ser);

			stream.Seek(0,System.IO.SeekOrigin.Begin);
			SoapFormatter formatter2 = new SoapFormatter();
			SLog log3 = (SLog)formatter2.Deserialize(stream);
			PropertiesCollectionEx props2 = (PropertiesCollectionEx)formatter2.Deserialize(stream);

			log3.Info(props2,"Hello world from deserialized stream.  Look for myprop prop. " +  + log3.GetHashCode());

			SLog log4 = (SLog)SLogManager.GetLogger(domain,typeof(TestSerializableLog));
			log4.Info("Hello world again - different SLog reference, same SLog?  Look for myprop prop. " + log4.GetHashCode());


			log4.Info(string.Format("Hash codes: {0} {1} {2} {3}",log.Logger.GetHashCode(),
				log2.Logger.GetHashCode(),
				log3.Logger.GetHashCode(),
					log4.Logger.GetHashCode()));

			Console.ReadLine();

		}
	}
}
