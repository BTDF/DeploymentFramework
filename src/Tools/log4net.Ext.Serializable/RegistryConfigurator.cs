// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;

using log4net.Repository;

namespace log4net.Ext.Config
{
	/// <summary>
	/// RegistryConfigHelper is a class designed to assist with loading log4net configuration
	/// for the case where we don't have an appdomain config file handy (e.g. BizTalk or COM+) and
	/// assembly level attributes are inappropriate given appdomain's base directory.
	/// The location of a log4net config file will be read from HKLM\Software\YourPath, using the
	/// "log4netConfig" string value. 
	/// log4net will be initialized once regardless of now many calls are made to ConfigureAndWatch.
	/// </summary>
	/// 

	public class RegistryConfigurator
	{
		private static Hashtable _configuredDomains = new Hashtable();
		public readonly static string RegValue = "log4netConfig";

		private RegistryConfigurator()
		{
		}

		/// <summary>
		/// Initialize a log4net domain based on the configuration file located at
		/// HKLM\LocalMachine\Software\registryPath, using the "log4netConfig" string value.
		/// </summary>
		/// <param name="applicationName"></param>
		public static void ConfigureAndWatch(string domainName, string registryPath)
		{
			// See if we have already configured this domain...
			ILoggerRepository repository = (ILoggerRepository)_configuredDomains[domainName];
			if(repository == null)
			{
				lock(typeof(RegistryConfigurator))
				{
					try 
					{
						repository = log4net.LogManager.GetRepository(domainName);
						//Trace.WriteLine("RegistryConfigurator::ConfigureAndWatch retrieved existing domain: " + domainName);
					} 
					catch (log4net.Core.LogException)
					{
						// Exception is expected if the domain has not been created.  This case shouldn't really happen.
						repository =
							log4net.LogManager.CreateRepository(domainName);
						//Trace.WriteLine("RegistryConfigurator::ConfigureAndWatch had to create domain: " + domainName);
					}

					string location = Getlog4netConfigLocation(registryPath);
					log4net.Config.XmlConfigurator.ConfigureAndWatch(repository,new System.IO.FileInfo(location));
					_configuredDomains[domainName] = repository;
				}
			}
			else
			{
				//Trace.WriteLine("RegistryConfigurator::ConfigureAndWatch called with an already configured domain: " + domainName);
			}
		}


		private static string Getlog4netConfigLocation(string registryPath)
		{
			RegistryKey rk = Registry.LocalMachine;
			string location = string.Empty;
			try
			{
				location = (string)rk.OpenSubKey(@"SOFTWARE\"+registryPath).GetValue(RegValue);
			}
			catch
			{
				throw(new Exception("log4netConfigHelper: Unable to read registry key for log4net configuration location."));
			}

			return location;
		}

	}
}
