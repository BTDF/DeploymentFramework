using System;
using System.Collections;
using SSOSettingsFileManager;

public class SSONameValueDump
{

	public static int Main(string[] args)
	{
		if(args.Length == 0)
		{
			Console.WriteLine("Pass in the same of the affiliate application to retrieve the name-value pairs from the SSO.");
			return 1;
		}
   
		// pass in affiliate app name as a command line parameter
		Hashtable ht = null;
		try
		{
			ht = SSOSettingsFileReader.Read(args[0]);
		}
		catch(System.Exception ex)
		{
			Console.WriteLine("Unable to dump name-value pairs: " + ex.Message);
			return 1;
		}
      
   
		foreach(object o in ht.Keys)
		{
			Console.WriteLine("Key: {0}  Value: {1}",o,ht[o]);
		}
      
		return 0;
	}

	#region Helper methods

	private static void WL(object text, params object[] args)
	{
		Console.WriteLine(text.ToString(), args);	
	}
	
	private static void RL()
	{
		Console.ReadLine();	
	}
	
	private static void Break() 
	{
		System.Diagnostics.Debugger.Break();
	}

	#endregion
}