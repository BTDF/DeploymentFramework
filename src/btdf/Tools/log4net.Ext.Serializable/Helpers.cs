// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Reflection;

namespace log4net.helpers
{
	public class CallersTypeName
	{
		/// <summary>
		/// Provided for environments such as BizTalk 2004 which have no effective way of 
		/// getting type information via reflection (so that the type name can be used in
		/// obtaining a log.)
		/// </summary>
		/// <returns></returns>
		public static string Name
		{
			get
			{
				StackTrace st = new StackTrace();
				// Get our caller
				StackFrame sf = st.GetFrame(1);
				MethodBase mb = sf.GetMethod();
				return mb.ReflectedType.ToString();
			}
		}	
	}
}
