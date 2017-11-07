using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Timers;
using Microsoft.BizTalk.SSOClient.Interop;

namespace SSOSettingsFileManager
{
	public class SSOSettingsFileReader
	{
		private static Hashtable _hashOfHashes = new Hashtable();
		public static bool ExceptionOnNull = true;
      private static DateTime _timeOfLastRefresh;
      private const int RefreshIntervalSecs = 60;

      static SSOSettingsFileReader()
      {
         _timeOfLastRefresh = DateTime.Now;
      }
      /// <summary>
      /// Will clear all the caches after the refresh interval has expired.
      /// It is assumed that a lock has been acquired prior to calling this function.
      /// </summary>
      private static void ClearAllCachesIfRefreshIntervalExpired()
      {
         // First get the elapsed time in seconds.
         TimeSpan elapsed = DateTime.Now - _timeOfLastRefresh;

         if (elapsed.TotalSeconds >= RefreshIntervalSecs)
         {
            _hashOfHashes.Clear();
            _timeOfLastRefresh = DateTime.Now;
         }
      }
 
		[Obsolete]
		public static string ReadValue(string affiliateApplication, string valueName)
		{
			return ReadString(affiliateApplication,valueName);
		}

		public static string ReadString(string affiliateApplication, string valueName)
		{
			Hashtable ht = Read(affiliateApplication);
			string val = (string)ht[valueName];
			
			if(val == null && ExceptionOnNull)
			{
				throw(new ArgumentException(
					string.Format("Requested SSO value {0} is not available within affiliate app {1}",
					valueName==null?"empty":valueName,affiliateApplication),"valueName"));
			}

			return val;
		}

		public static int ReadInt32(string affiliateApplication, string valueName)
		{
			return System.Convert.ToInt32(ReadString(affiliateApplication, valueName));
		}

		public static void ClearCache(string affiliateApplication)
		{
			Hashtable ht = (Hashtable)_hashOfHashes[affiliateApplication];
			if(ht != null)
			{
            lock (_hashOfHashes)
            {
               _hashOfHashes[affiliateApplication] = null;
            }
			}
		}

		public static Hashtable Read(string affiliateApplication)
		{
         Hashtable htToReturn = null;

			lock(_hashOfHashes)
			{
            // Clear all caches if the refresh interval has expired.
            ClearAllCachesIfRefreshIntervalExpired();

            // Check to see if the applicatin is in the table.
            htToReturn = (Hashtable)_hashOfHashes[affiliateApplication];
            if (htToReturn == null)
			   {
				   // Not in table.  Read it in from SSO.
				   object propertyValue;
				   ISSOConfigStore configStore = (ISSOConfigStore)new SSOConfigStore();
				   SSOPropertyBag bag = new SSOPropertyBag();
				   configStore.GetConfigInfo(affiliateApplication,SSOHelper.InfoIdentifier, SSOFlag.SSO_FLAG_RUNTIME, bag);
				   bag.Read(SSOHelper.PropName, out propertyValue, 0);

				   // Now we have what should be a settings file in hand as a string...
				   // Need to deserialize it...
				   settings appSettings = null;
				   XmlSerializer serializer = new XmlSerializer(typeof(settings));	
				   StringReader stringReader = new StringReader((string)propertyValue);
				   appSettings = (settings)serializer.Deserialize(stringReader);

				   // Get a hashtable from the property collection, cache it, and return it.
               htToReturn = new Hashtable();
               _hashOfHashes[affiliateApplication] = htToReturn;
				   foreach(property prop in appSettings.propertyCollection)
				   {
                  htToReturn[prop.name] = prop.Value;
				   }
				}
			}
         return htToReturn;
		}

		public static void Update(string affiliateApplication, Hashtable ht)
		{
			settings inSettings = new settings();
			foreach(object key in ht.Keys)
			{
				SSOSettingsFileManager.property prop = new SSOSettingsFileManager.property();
				prop.name = key.ToString();
				prop.Value = (string)ht[key];
				inSettings.propertyCollection.Add(prop);
			}

			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			XmlSerializer serializer = new XmlSerializer(typeof(settings));	
			serializer.Serialize(writer,inSettings);

			SSOHelper helper = new SSOHelper();
			SSOPropertyBag bag = new SSOPropertyBag();
			object o = sb.ToString();
			bag.Write(SSOHelper.PropName,ref o);
				
			if(helper.AppExists(affiliateApplication))
			{
				helper.SaveConfigInfo(bag, affiliateApplication);			
			}

			ClearCache(affiliateApplication);
		}
	}

	/// <summary>
	/// Helper component for SSO interactions
	/// </summary>
	internal class SSOHelper
	{
		public static string PropName = "AppConfig";
		public static string InfoIdentifier = "{56D74464-67EA-464d-A9D4-3EBBA4090010}";

		/// <summary>
		/// Saves the config info.
		/// </summary>
		public void SaveConfigInfo(SSOPropertyBag bag, string affiliateApplication)
		{
			ISSOConfigStore configStore = (ISSOConfigStore)new SSOConfigStore();
			configStore.SetConfigInfo(affiliateApplication, SSOHelper.InfoIdentifier, bag); 
		}

		/// <summary>
		/// Deletes the config info.
		/// </summary>
		public void DeleteConfigInfo(string affiliateApplication)
		{
			ISSOConfigStore configStore = (ISSOConfigStore)new SSOConfigStore();
			configStore.DeleteConfigInfo(affiliateApplication, SSOHelper.InfoIdentifier);
		}

		public void DeleteApp(string appName)
		{
			if(AppExists(appName))
			{
				ISSOAdmin ssoa = GetAdmin();
				ssoa.DeleteApplication(appName);
			}
		}
	
		// Borrowed largely from Flanders' SSOAppConfigHelper object.
		public void CreateApp(string appName, string userGroup, string adminGroup)
		{
			ISSOAdmin ssoa = GetAdmin();
			int fields = 2;
			ssoa.CreateApplication(appName,appName + " Configuration Data","someone@microsoft.com",userGroup,adminGroup,SSOFlag.SSO_FLAG_APP_CONFIG_STORE|SSOFlag.SSO_FLAG_APP_ALLOW_LOCAL,fields);
			ssoa.CreateFieldInfo(appName,"someone@microsoft.com",SSOFlag.SSO_FLAG_FIELD_INFO_SYNC);
			ssoa.CreateFieldInfo(appName,SSOHelper.PropName,SSOFlag.SSO_FLAG_FIELD_INFO_SYNC);
			ssoa.UpdateApplication(appName,null,null,null,null,SSOFlag.SSO_FLAG_ENABLED,SSOFlag.SSO_FLAG_ENABLED);
		}

		// Borrowed from Flanders' SSOAppConfigHelper object.
		public bool AppExists(string appName)
		{
			ISSOAdmin ssoa = GetAdmin();
			string description;
			string contact;
			string grpName;
			string grpAdmin;
			int flags;
			int fields;
			bool ret=false;
			try
			{
				ssoa.GetApplicationInfo(appName,out description,out contact,out grpName,out grpAdmin,out flags, out fields);
				ret=true;
			}
			catch{}
			finally
			{
				if(ssoa!=null)
					Marshal.ReleaseComObject(ssoa);
			}
			return ret;
		}

		// Borrowed from Flanders' SSOAppConfigHelper object.
		ISSOAdmin GetAdmin()
		{
			SSOAdmin ssoadmin  = new SSOAdmin();
			ISSOAdmin ssoa = (ISSOAdmin)ssoadmin;
			return ssoa;
		}

	}
}

/// <summary>
/// This class is simply a forwarder (with a few extra convenience methods) that lives outside the
/// namespace so as to shorten up the syntax in the orchestration expression editor.
/// </summary>
public class SSOSettingsFileReader
{
	public static string ReadString(string affiliateApplication, string valueName)
	{
		return SSOSettingsFileManager.SSOSettingsFileReader.ReadString(affiliateApplication, valueName);
	}

	public static int ReadInt32(string affiliateApplication, string valueName)
	{
		return SSOSettingsFileManager.SSOSettingsFileReader.ReadInt32(affiliateApplication, valueName);
	}

	public static void ClearCache(string affiliateApplication)
	{
		SSOSettingsFileManager.SSOSettingsFileReader.ClearCache(affiliateApplication);
	}

	public static Hashtable Read(string affiliateApplication)
	{
		return SSOSettingsFileManager.SSOSettingsFileReader.Read(affiliateApplication);
	}

}