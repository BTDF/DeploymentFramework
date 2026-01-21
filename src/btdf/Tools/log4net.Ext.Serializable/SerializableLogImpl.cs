// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using log4net;
using log4net.helpers;
using log4net.Core;
using log4net.Util;
using System.Runtime.Serialization;
using System.Globalization;

namespace log4net.Ext.Serializable
{
	[Serializable]
	public class SLog : ILoggerWrapper, ISerializable, ILog
	{
		#region Private members

		private enum ConfigMethod {None,RegistryConfigurator, AppConfigConfigurator};
		private ILogger _logger;
		private string _fullName;
		private string _loggerName;
		private PropertiesDictionary _properties = new PropertiesDictionary();
		private ConfigMethod _configMethod = ConfigMethod.None;
		private string _domainName;

		private readonly static Type _slogType = typeof(SLog);

		#endregion
		
		#region Serialization Constructor
		
		private SLog(SerializationInfo info, StreamingContext context)
		{
			_loggerName = (string)info.GetValue("loggerName",typeof(string));
			_domainName = (string)info.GetValue("domainName",typeof(string));
			_properties  = (PropertiesDictionary)info.GetValue("properties",typeof(PropertiesDictionary));

			SLogManager.CreateDomainIfNeeded(_domainName);

			// We don't want to go through SLogManager, because we want to inject THIS deserialized
			// instance into SLogManager's map.
			_logger = LogManager.GetLogger(_domainName,_loggerName).Logger;

			_configMethod = (ConfigMethod)info.GetValue("configMethod",typeof(int));
			if(_configMethod == ConfigMethod.RegistryConfigurator)
			{
				log4net.Ext.Config.RegistryConfigurator.ConfigureAndWatch(_domainName,_domainName);
			}

			if(_configMethod == ConfigMethod.AppConfigConfigurator)
			{
				log4net.Ext.Config.AppConfigConfigurator.ConfigureAndWatch(_domainName, _domainName);
				//log4net.Config.XmlConfigurator.Configure();
			}


			// Get this deserialized instance to the wrapper map so it will be used by others asking for a
			// logger of the same name.
			SLogManager.AddToMap(_logger,this);

			//System.Diagnostics.Trace.WriteLine("Deserializing SLog..." + this.GetHashCode() + ":" + descount++);
		}
		//private static int descount = 0;
		
		#endregion

		#region Public Instance Constructors

		public SLog(ILogger logger)
		{
			_logger = logger;
			_fullName = this.GetType().FullName;
			_loggerName = logger.Name;
			_domainName = logger.Repository.Name;
		}

		#endregion Public Instance Constructors

		#region Public Properties and methods
		
		[Obsolete("This member was discovered to not be threadsafe.  Use the PropertiesCollectionEx class and associated overrides for Info/Log/Warn/Debug instead.")]
		public log4net.Util.PropertiesDictionary Properties {get{return _properties;}}

		// Offered as a convenience to add a item called InstanceId to the standard
		// properties collection.  Used currently by the Paul Bunyan appender to populate
		// the context field.
		[Obsolete("This member was discovered to not be threadsafe.  Use the PropertiesCollectionEx class and associated overrides for Info/Log/Warn/Debug instead.")]
		public string InstanceId
		{
			get
			{
				return (string)_properties["InstanceId"];
			}
			set
			{
				_properties["InstanceId"] = value;
			}
		}
		
		/// <summary>
		/// This logger offers the ability to use at least one configurator, so it can store
		/// state information about how log4net was configured to be used if the appdomain is recycled.
		/// </summary>
		/// <param name="registryPath"></param>
		public void RegistryConfigurator()
		{
			_configMethod = ConfigMethod.RegistryConfigurator;

			// Here, we are making our domain name be equal to the registry path used for configuration.
			log4net.Ext.Config.RegistryConfigurator.ConfigureAndWatch(_domainName,_domainName);
		}


		public void AppConfigConfigurator()
		{
			_configMethod = ConfigMethod.AppConfigConfigurator;

			// Here, we are making our domain name be equal to the App.Settings key used for configuration.
			log4net.Ext.Config.AppConfigConfigurator.ConfigureAndWatch(_domainName,_domainName);
			//log4net.Config.XmlConfigurator.Configure();
			
		}


		#endregion

		#region Implementation of ILog

		private LoggingEventData CreateEventData(
			Level level, object message, Exception t, PropertiesCollectionEx props)
		{
			LoggingEventData data = new LoggingEventData();
			
			data.Message = message.ToString();
			data.ExceptionString = t==null?null:t.ToString();
			data.Properties = props == null ?null:props.PropertiesCollection;
			data.LocationInfo = new LocationInfo(_slogType);
			data.Domain = _domainName;

			data.LoggerName = _logger.Name;
			data.Level = level;
			data.TimeStamp = DateTime.Now;

			return data;
		}

		public void Debug(object message)
		{
			Debug(null,message, null);
		}

		public void Debug(PropertiesCollectionEx props, object message)
		{
			Debug(props,message,null);
		}

		public void Debug(object message, System.Exception t)
		{
			Debug(null,message,t);
		}

		public void Debug(PropertiesCollectionEx props,object message, System.Exception t)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Debug))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Debug,message,t,props));
				_logger.Log(loggingEvent);
			}
		}

		void log4net.ILog.DebugFormat(IFormatProvider provider, string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Debug, String.Format(provider, format, args), null);
		}

		void log4net.ILog.DebugFormat(string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Debug, String.Format(CultureInfo.InvariantCulture, format, args), null);
		}

      void log4net.ILog.DebugFormat(string format, object arg0)
      {
         Logger.Log(_slogType, log4net.Core.Level.Debug, String.Format(CultureInfo.InvariantCulture, format, arg0), null);
      }

      void log4net.ILog.DebugFormat(string format, object arg0, object arg1)
      {
         Logger.Log(_slogType, log4net.Core.Level.Debug, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1), null);
      }

      void log4net.ILog.DebugFormat(string format, object arg0, object arg1, object arg2)
      {
         Logger.Log(_slogType, log4net.Core.Level.Debug, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2), null);
      }

		public void DebugFormat(PropertiesCollectionEx props, string format, params object[] args)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Debug))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Debug,String.Format(format, args),null,props));
				_logger.Log(loggingEvent);
			}
		}

		public void Info(object message)
		{
			Info(null,message,null);
		}

		public void Info(PropertiesCollectionEx props,object message)
		{
			Info(props,message,null);
		}

		public void Info(object message, System.Exception t)
		{
			Info(null,message,t);
		}

		public void Info(PropertiesCollectionEx props,object message,System.Exception t)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Info))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Info,message,t,props));
				_logger.Log(loggingEvent);
			}
		}

		void log4net.ILog.InfoFormat(IFormatProvider provider, string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Info, String.Format(provider, format, args), null);
		}

		void log4net.ILog.InfoFormat(string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Info, String.Format(CultureInfo.InvariantCulture, format, args), null);
		}

      void log4net.ILog.InfoFormat(string format, object arg0)
      {
         Logger.Log(_slogType, log4net.Core.Level.Info, String.Format(CultureInfo.InvariantCulture, format, arg0), null);
      }

      void log4net.ILog.InfoFormat(string format, object arg0, object arg1)
      {
         Logger.Log(_slogType, log4net.Core.Level.Info, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1), null);
      }

      void log4net.ILog.InfoFormat(string format, object arg0, object arg1, object arg2)
      {
         Logger.Log(_slogType, log4net.Core.Level.Info, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2), null);
      }

		public void InfoFormat(PropertiesCollectionEx props, string format, params object[] args)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Info))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Info,String.Format(format, args),null,props));
				_logger.Log(loggingEvent);
			}
		}


		public void Warn(object message)
		{
			Warn(null,message, null);
		}

		public void Warn(PropertiesCollectionEx props,object message)
		{
			Warn(props,message, null);
		}

		public void Warn(object message, System.Exception t)
		{
			Warn(null,message, t);
		}

		public void Warn(PropertiesCollectionEx props, object message, System.Exception t)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Warn))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Warn,message,t,props));
				_logger.Log(loggingEvent);
			}
		}

		void log4net.ILog.WarnFormat(IFormatProvider provider, string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Warn, String.Format(provider, format, args), null);
		}

		void log4net.ILog.WarnFormat(string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Warn, String.Format(CultureInfo.InvariantCulture, format, args), null);
		}

      void log4net.ILog.WarnFormat(string format, object arg0)
      {
         Logger.Log(_slogType, log4net.Core.Level.Warn, String.Format(CultureInfo.InvariantCulture, format, arg0), null);
      }

      void log4net.ILog.WarnFormat(string format, object arg0, object arg1)
      {
         Logger.Log(_slogType, log4net.Core.Level.Warn, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1), null);
      }

      void log4net.ILog.WarnFormat(string format, object arg0, object arg1, object arg2)
      {
         Logger.Log(_slogType, log4net.Core.Level.Warn, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2), null);
      }

		public void WarnFormat(PropertiesCollectionEx props, string format, params object[] args)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Warn))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Warn,String.Format(format, args),null,props));
				_logger.Log(loggingEvent);
			}
		}

		public void Error(object message)
		{
			Error(null,message, null);
		}

		public void Error(PropertiesCollectionEx props,object message)
		{
			Error(props,message, null);
		}

		public void Error(object message, System.Exception t)
		{
			Error(null, message, t);
		}

		public void Error(PropertiesCollectionEx props, object message, System.Exception t)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Error))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Error,message,t, props));
				_logger.Log(loggingEvent);
			}
		}

		void log4net.ILog.ErrorFormat(IFormatProvider provider, string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Error, String.Format(provider, format, args), null);
		}

		void log4net.ILog.ErrorFormat(string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Error, String.Format(CultureInfo.InvariantCulture, format, args), null);
		}

      void log4net.ILog.ErrorFormat(string format, object arg0)
      {
         Logger.Log(_slogType, log4net.Core.Level.Error, String.Format(CultureInfo.InvariantCulture, format, arg0), null);
      }

      void log4net.ILog.ErrorFormat(string format, object arg0, object arg1)
      {
         Logger.Log(_slogType, log4net.Core.Level.Error, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1), null);
      }

      void log4net.ILog.ErrorFormat(string format, object arg0, object arg1, object arg2)
      {
         Logger.Log(_slogType, log4net.Core.Level.Error, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2), null);
      }

		public void ErrorFormat(PropertiesCollectionEx props, string format, params object[] args)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Error))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Error,String.Format(format, args),null,props));
				_logger.Log(loggingEvent);
			}
		}

		public void Fatal(object message)
		{
			Fatal(null, message, null);
		}

		public void Fatal(PropertiesCollectionEx props,object message)
		{
			Fatal(props, message, null);
		}

		public void Fatal(object message, System.Exception t)
		{
			Fatal(null,message,t);
		}

		public void Fatal(PropertiesCollectionEx props,object message, System.Exception t)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Fatal))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Fatal,message,t,props));
				_logger.Log(loggingEvent);
			}

		}

		void log4net.ILog.FatalFormat(IFormatProvider provider, string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Fatal, String.Format(provider, format, args), null);
		}

		void log4net.ILog.FatalFormat(string format, params object[] args)
		{
			Logger.Log(_slogType, log4net.Core.Level.Fatal, String.Format(CultureInfo.InvariantCulture, format, args), null);
		}

      void log4net.ILog.FatalFormat(string format, object arg0)
      {
         Logger.Log(_slogType, log4net.Core.Level.Fatal, String.Format(CultureInfo.InvariantCulture, format, arg0), null);
      }

      void log4net.ILog.FatalFormat(string format, object arg0, object arg1)
      {
         Logger.Log(_slogType, log4net.Core.Level.Fatal, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1), null);
      }

      void log4net.ILog.FatalFormat(string format, object arg0, object arg1, object arg2)
      {
         Logger.Log(_slogType, log4net.Core.Level.Fatal, String.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2), null);
      }

		public void FatalFormat(PropertiesCollectionEx props, string format, params object[] args)
		{
			if (_logger.IsEnabledFor(log4net.Core.Level.Fatal))
			{
				LoggingEvent loggingEvent = new LoggingEvent(CreateEventData(Level.Fatal,String.Format(format, args),null,props));
				_logger.Log(loggingEvent);
			}
		}

		public bool IsErrorEnabled
		{
			get
			{
				return _logger.IsEnabledFor(log4net.Core.Level.Error);
			}
		}

		public bool IsFatalEnabled
		{
			get
			{
				return _logger.IsEnabledFor(log4net.Core.Level.Fatal);
			}
		}

		public bool IsWarnEnabled
		{
			get
			{
				return _logger.IsEnabledFor(log4net.Core.Level.Warn);
			}
		}

		public bool IsInfoEnabled
		{
			get
			{
				return _logger.IsEnabledFor(log4net.Core.Level.Info);
			}
		}

		public bool IsDebugEnabled
		{
			get
			{
				return _logger.IsEnabledFor(log4net.Core.Level.Debug);
			}
		}

	
		#endregion Implementation of ILog

		#region ILoggerWrapper Members

		public ILogger Logger
		{
			get
			{
				return _logger;
			}
		}

		#endregion

		#region ISerializable Members

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("loggerName",_loggerName);
			info.AddValue("properties",_properties);
			info.AddValue("configMethod",(int)_configMethod);
			info.AddValue("domainName",_domainName);

			//System.Diagnostics.Trace.WriteLine("Serializing SLog..." + this.GetHashCode() + ":" + sercount++);
		}
		//private static int sercount=0;

		#endregion

	}
}
