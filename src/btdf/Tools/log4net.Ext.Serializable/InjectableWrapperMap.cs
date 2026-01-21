// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;

using log4net.Repository;
using log4net;
using log4net.helpers;
using log4net.Core;


namespace log4net.Ext.Serializable
{
	/// <summary>
	/// Summary description for InjectableWrapperMap.
	/// </summary>
	internal class InjectableWrapperMap
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WrapperMap" /> class with 
		/// the specified handler to create the wrapper objects.
		/// </summary>
		/// <param name="createWrapperHandler">The handler to use to create the wrapper objects.</param>
		public InjectableWrapperMap(WrapperCreationHandler createWrapperHandler) 
		{
			m_createWrapperHandler = createWrapperHandler;

			// Create the delegates for the event callbacks
			m_shutdownHandler = new LoggerRepositoryShutdownEventHandler(OnShutdown);
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the wrapper object for the specified logger.
		/// </summary>
		/// <value>
		/// The wrapper object for the specified logger.
		/// </value>
		/// <remarks>
		/// If the logger is null then the coresponding wrapper is null
		/// </remarks>
		virtual public ILoggerWrapper this[ILogger logger]
		{
			get
			{
				// If the logger is null then the coresponding wrapper is null
				if (logger == null)
				{
					return null;
				}

				lock(this)
				{
					// Lookup hierarchy in map.
					Hashtable wrappersMap = (Hashtable)m_repositories[logger.Repository];

					if (wrappersMap == null)
					{
						// Hierarchy does not exist in map.
						// Must register with hierarchy

						wrappersMap = new Hashtable();
						m_repositories[logger.Repository] = wrappersMap;

						// Register for config reset & shutdown on repository
						logger.Repository.ShutdownEvent += m_shutdownHandler;
					}

					// Look for the wrapper object in the map
					ILoggerWrapper wrapperObject = wrappersMap[logger] as ILoggerWrapper;

					if (wrapperObject == null)
					{
						// No wrapper object exists for the specified logger

						// Create a new wrapper wrapping the logger
						wrapperObject = CreateNewWrapperObject(logger);
					
						// Store wrapper logger in map
						wrappersMap[logger] = wrapperObject;
					}

					return wrapperObject;
				}

			}

			set
			{
				lock(this)
				{
					// Lookup hierarchy in map.
					Hashtable wrappersMap = (Hashtable)m_repositories[logger.Repository];

					if (wrappersMap == null)
					{
						// Hierarchy does not exist in map.
						// Must register with hierarchy

						wrappersMap = new Hashtable();
						m_repositories[logger.Repository] = wrappersMap;

						// Register for config reset & shutdown on repository
						logger.Repository.ShutdownEvent += m_shutdownHandler;
					}

					wrappersMap[logger] = value;
				}

			}

			
			
		}

		#endregion Public Instance Properties

		#region Protected Instance Properties

		/// <summary>
		/// Gets the map of logger repositories.
		/// </summary>
		/// <value>
		/// Map of logger repositories.
		/// </value>
		protected Hashtable Repositories 
		{
			get { return this.m_repositories; }
		}

		#endregion Protected Instance Properties

		#region Protected Instance Methods

		/// <summary>
		/// Creates the wrapper object for the specified logger.
		/// </summary>
		/// <param name="logger">The logger to wrap in a wrapper.</param>
		/// <remarks>
		/// This implementation uses the <see cref="WrapperCreationHandler"/>
		/// passed to the constructor to create the wrapper. This method
		/// can be overriden in a subclass.
		/// </remarks>
		/// <returns>The wrapper object for the logger.</returns>
		virtual protected ILoggerWrapper CreateNewWrapperObject(ILogger logger)
		{
			if (m_createWrapperHandler != null)
			{
				return m_createWrapperHandler(logger);
			}
			return null;
		}

		/// <summary>
		/// Event handler for repository shutdown event.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		virtual protected void OnShutdown(object sender, EventArgs e)
		{
			lock(this)
			{
				if (sender is ILoggerRepository)
				{
					// Remove all repository from map
					m_repositories.Remove(sender);

					// Unhook all events from the repository
					((ILoggerRepository)sender).ShutdownEvent -= m_shutdownHandler;
				}
			}
		}

		#endregion Protected Instance Methods

		#region Private Instance Variables

		/// <summary>
		/// Map of logger repositories.
		/// </summary>
		private Hashtable m_repositories = new Hashtable();

		/// <summary>
		/// The handler to use to create the extension wrapper objects.
		/// </summary>
		private WrapperCreationHandler m_createWrapperHandler;

		/// <summary>
		/// Internal reference to the delegate used to register for repository shutdown events.
		/// </summary>
		private LoggerRepositoryShutdownEventHandler m_shutdownHandler;
 
		#endregion Private Instance Variables
	}
}
