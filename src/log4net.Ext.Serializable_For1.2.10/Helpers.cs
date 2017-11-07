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
