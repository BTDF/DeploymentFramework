// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;

namespace BizTalkSample.Components
{
	/// <summary>
	/// Returns resources that are embedded in an assembly, providing a strong typing and cache.
	/// </summary>
	public class EmbeddedResourceCache
	{
		private static Hashtable _templateCache = new Hashtable();
		private static Assembly _assembly = Assembly.GetExecutingAssembly();
		private static string _namespace;
 
		static EmbeddedResourceCache()
		{
			// Keeps this class able to drop into any namespace.
			Type type = MethodBase.GetCurrentMethod().DeclaringType;
			_namespace = type.Namespace;
		}
            
		public static XmlDocument GetXmlDocResource(string fileName)
		{
			XmlDocument doc = null;
			string key = fileName + "_XmlDocument";
 
			if(_templateCache.ContainsKey(key))
				doc = (XmlDocument)_templateCache[key];
			else
			{
				lock(_templateCache.SyncRoot)
				{
					if(_templateCache.ContainsKey(key))
						doc = (XmlDocument)((XmlDocument)_templateCache[key]).CloneNode(true);
					else
					{
						string resourceName = string.Format("{0}.{1}", _namespace, fileName);

                        using (Stream stream = _assembly.GetManifestResourceStream(resourceName))
                        {
                            doc = new XmlDocument();
                            doc.Load(stream);
                        }
 
						_templateCache[key] = doc;
					}
				}
			}
 
			return doc;
		}
 
		public static string GetStringResource(string fileName)
		{
			string resourceContents;
			string key = fileName + "_String";
 
			if(_templateCache.ContainsKey(key))
				resourceContents = (string)_templateCache[key];
			else
			{
				lock(_templateCache.SyncRoot)
				{
					if(_templateCache.ContainsKey(key))
						resourceContents = (string)_templateCache[key];
					else
					{
						string resourceName = string.Format("{0}.{1}",
							_namespace,fileName);
 
						StreamReader reader = new StreamReader(
							_assembly.GetManifestResourceStream(resourceName));
 
						resourceContents = reader.ReadToEnd();
						_templateCache[key] = resourceContents;
					}
				}
			}
 
			return resourceContents;
		}
	}
}
