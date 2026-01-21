// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Configuration;
using System.Collections;


using log4net.Repository;

namespace log4net.Ext.Config
{
	/// <summary>
	/// Summary description for AppConfigConfigurator.
	/// </summary>
	public class AppConfigConfigurator
	{
		private static Hashtable _configuredDomains = new Hashtable();

		public AppConfigConfigurator()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static void ConfigureAndWatch(string domainName, string keyValue)
		{
			// See if we have already configured this domain...
			ILoggerRepository repository = (ILoggerRepository)_configuredDomains[domainName];
			if(repository == null)
			{
				lock(typeof(AppConfigConfigurator))
				{
					try 
					{
						repository = log4net.LogManager.GetRepository(domainName);
					} 
					catch (log4net.Core.LogException)
					{
						// Exception is expected if the domain has not been created.  This case shouldn't really happen.
						repository = log4net.LogManager.CreateRepository(domainName);
					}

					string location = Getlog4netConfigLocation(keyValue);
					log4net.Config.XmlConfigurator.ConfigureAndWatch(repository,new System.IO.FileInfo(location));
					_configuredDomains[domainName] = repository;
				}
			}
			else
			{
			}
		}

		/// <summary>
		/// Retrives the log4net config location from the applications app.config file.
		/// </summary>
		/// <param name="keyValue">The key name in the app.config file</param>
		/// <returns>The location of the log4net config as prescribed by the app.config file.</returns>
		private static string Getlog4netConfigLocation(string keyValue)
		{
			string location = string.Empty;
			try
			{
            location = ConfigurationManager.AppSettings[keyValue].ToString();
			}
			catch
			{
				throw(new Exception("log4netConfigHelper: Unable to read App.Config key for log4net configuration location. " + keyValue));
			}

			return location;
		}
	}
}
