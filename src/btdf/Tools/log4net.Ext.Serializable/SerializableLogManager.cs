// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

/*
 * Custom Logging Classes to support serializable loggers
 */
namespace log4net.Ext.Serializable
{
	public class SLogManager
	{
		#region Static Member Variables

		/// <summary>
		/// The wrapper map to use to hold the <see cref="EventIDLogImpl"/> objects
		/// </summary>
		private static InjectableWrapperMap s_wrapperMap = new InjectableWrapperMap(new WrapperCreationHandler(WrapperCreationHandler));
		private static ArrayList _createdDomains = new ArrayList();

		#endregion

		#region Constructor

		/// <summary>
		/// Private constructor to prevent object creation
		/// </summary>
		private SLogManager() { }

		#endregion

		#region Type Specific Manager Methods

		/// <summary>
		/// Returns the named logger if it exists
		/// </summary>
		/// <remarks>
		/// <para>If the named logger exists (in the default hierarchy) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.</para>
		/// </remarks>
		/// <param name="name">The fully qualified logger name to look for</param>
		/// <returns>The logger found, or null</returns>
		public static SLog Exists(string name) 
		{
			return Exists(Assembly.GetCallingAssembly(), name);
		}

		/// <summary>
		/// Returns the named logger if it exists
		/// </summary>
		/// <remarks>
		/// <para>If the named logger exists (in the specified domain) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.</para>
		/// </remarks>
		/// <param name="domain">the domain to lookup in</param>
		/// <param name="name">The fully qualified logger name to look for</param>
		/// <returns>The logger found, or null</returns>
		public static SLog Exists(string domain, string name) 
		{
			return WrapLogger(LoggerManager.Exists(domain, name));
		}

		/// <summary>
		/// Returns the named logger if it exists
		/// </summary>
		/// <remarks>
		/// <para>If the named logger exists (in the specified assembly's domain) then it
		/// returns a reference to the logger, otherwise it returns
		/// <c>null</c>.</para>
		/// </remarks>
		/// <param name="assembly">the assembly to use to lookup the domain</param>
		/// <param name="name">The fully qualified logger name to look for</param>
		/// <returns>The logger found, or null</returns>
		public static SLog Exists(Assembly assembly, string name) 
		{
			return WrapLogger(LoggerManager.Exists(assembly, name));
		}

		/// <summary>
		/// Returns all the currently defined loggers in the default domain.
		/// </summary>
		/// <remarks>
		/// <para>The root logger is <b>not</b> included in the returned array.</para>
		/// </remarks>
		/// <returns>All the defined loggers</returns>
		public static SLog[] GetCurrentLoggers()
		{
			return GetCurrentLoggers(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified domain.
		/// </summary>
		/// <param name="domain">the domain to lookup in</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers</returns>
		public static SLog[] GetCurrentLoggers(string domain)
		{
			return WrapLoggers(LoggerManager.GetCurrentLoggers(domain));
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified assembly's domain.
		/// </summary>
		/// <param name="assembly">the assembly to use to lookup the domain</param>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <returns>All the defined loggers</returns>
		public static SLog[] GetCurrentLoggers(Assembly assembly)
		{
			return WrapLoggers(LoggerManager.GetCurrentLoggers(assembly));
		}

		/// <summary>
		/// Retrieve or create a named logger.
		/// </summary>
		/// <remarks>
		/// <para>Retrieve a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.</para>
		/// 
		/// <para>By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.</para>
		/// </remarks>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>the logger with the name specified</returns>
		public static SLog GetLogger(string name)
		{
			return GetLogger(Assembly.GetCallingAssembly(), name);
		}

		/// <summary>
		/// Retrieve or create a named logger.
		/// </summary>
		/// <remarks>
		/// <para>Retrieve a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.</para>
		/// 
		/// <para>By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.</para>
		/// </remarks>
		/// <param name="domain">the domain to lookup in</param>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>the logger with the name specified</returns>
		public static SLog GetLogger(string domain, string name)
		{
			CreateDomainIfNeeded(domain);
			return WrapLogger(LoggerManager.GetLogger(domain, name));
		}

		/// <summary>
		/// Retrieve or create a named logger.
		/// </summary>
		/// <remarks>
		/// <para>Retrieve a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.</para>
		/// 
		/// <para>By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.</para>
		/// </remarks>
		/// <param name="assembly">the assembly to use to lookup the domain</param>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>the logger with the name specified</returns>
		public static SLog GetLogger(Assembly assembly, string name)
		{
			return WrapLogger(LoggerManager.GetLogger(assembly, name));
		}	

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Get the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="type">The full name of <paramref name="type"/> will 
		/// be used as the name of the logger to retrieve.</param>
		/// <returns>the logger with the name specified</returns>
		public static SLog GetLogger(Type type) 
		{
			return GetLogger(Assembly.GetCallingAssembly(), type.FullName);
		}

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Get the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="domain">the domain to lookup in</param>
		/// <param name="type">The full name of <paramref name="type"/> will 
		/// be used as the name of the logger to retrieve.</param>
		/// <returns>the logger with the name specified</returns>
		public static SLog GetLogger(string domain, Type type) 
		{
			CreateDomainIfNeeded(domain);
			return WrapLogger(LoggerManager.GetLogger(domain, type));
		}

		/// <summary>
		/// Shorthand for <see cref="LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Get the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="assembly">the assembly to use to lookup the domain</param>
		/// <param name="type">The full name of <paramref name="type"/> will 
		/// be used as the name of the logger to retrieve.</param>
		/// <returns>the logger with the name specified</returns>
		public static SLog GetLogger(Assembly assembly, Type type) 
		{
			return WrapLogger(LoggerManager.GetLogger(assembly, type));
		}

		#endregion

		#region Extension Handlers

		/// <summary>
		/// Because LogManager does not offer us a away to determine if a domain has already been created,
		/// we have this helper method to accomplish that.  To avoid relying on an exception being thrown/caught
		/// as the only mechanism for seeing if a logging domain is created, we keep our own list.
		/// </summary>
		/// <param name="domainName"></param>
		public static void CreateDomainIfNeeded(string domainName)
		{
			try 
			{
				if(!_createdDomains.Contains(domainName))
				{
					log4net.LogManager.CreateRepository(domainName);
					_createdDomains.Add(domainName);
				}

			} 
			catch (log4net.Core.LogException)
			{
				// Exception is expected if the domain has already been created.
			}
		}

		/// <summary>
		/// Lookup the wrapper object for the logger specified
		/// </summary>
		/// <param name="logger">the logger to get the wrapper for</param>
		/// <returns>the wrapper for the logger specified</returns>
		public static SLog WrapLogger(ILogger logger)
		{
			return (SLog)s_wrapperMap[logger];
		}

		/// <summary>
		/// Lookup the wrapper objects for the loggers specified
		/// </summary>
		/// <param name="loggers">the loggers to get the wrappers for</param>
		/// <returns>Lookup the wrapper objects for the loggers specified</returns>
		public static SLog[] WrapLoggers(ILogger[] loggers)
		{
			SLog[] results = new SLog[loggers.Length];
			for(int i=0; i<loggers.Length; i++)
			{
				results[i] = WrapLogger(loggers[i]);
			}
			return results;
		}

		/// <summary>
		/// Method to create the <see cref="ILoggerWrapper"/> objects used by
		/// this manager.
		/// </summary>
		/// <param name="logger">The logger to wrap</param>
		/// <returns>The wrapper for the logger specified</returns>
		private static ILoggerWrapper WrapperCreationHandler(ILogger logger)
		{
			return new SLog(logger);
		}

		internal static void AddToMap(ILogger logger,SLog logImpl)
		{
			s_wrapperMap[logger] = logImpl;
		}

		#endregion
	}
}
