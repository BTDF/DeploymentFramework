// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Runtime.Serialization;
using log4net.Util;

namespace log4net.helpers
{

	/// <summary>
	/// Just a wrapper that makes a PropertiesCollection easier for BizTalk orchestrations.
	/// </summary>
	[Serializable] public class PropertiesCollectionEx : ISerializable
	{
		public PropertiesCollectionEx()
		{
		}

		private PropertiesCollectionEx(SerializationInfo info, StreamingContext context)
		{
			_properties = (PropertiesDictionary)info.GetValue("props",typeof(PropertiesDictionary));
		}

		public string[] GetKeys()
		{
			return _properties.GetKeys();
		}

		public object this[string key]
		{
			get { return _properties[key]; }
			set { _properties[key] = value; }
		}

		public object Get(string key)
		{
			return _properties[key];
		}

		public void Set(string key, object valueToSet)
		{
			_properties[key] = valueToSet;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("props", _properties);
		}

		public PropertiesDictionary PropertiesCollection
		{
			get{return _properties;}
		}

		private PropertiesDictionary _properties = new PropertiesDictionary();
	}
}


