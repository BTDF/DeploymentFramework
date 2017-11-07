using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace GetGacPathHelper
{
	/// <summary>
	/// Summary description for GetGacPathHelper.
	/// </summary>
	public class GetGacPath
	{
		public static string GetPath(
			string pathOrPartialAssemblyName,
			string gacVariant)
		{

			StringBuilder sb = new StringBuilder();

			sb.Append(System.Environment.GetEnvironmentVariable("windir"));
			sb.Append(@"\assembly\");
			sb.Append(gacVariant);
			sb.Append(@"\");

			AssemblyName name;

			if(pathOrPartialAssemblyName.ToUpper().IndexOf(".DLL") == -1)
			{
				Assembly assembly = Assembly.LoadWithPartialName(pathOrPartialAssemblyName);
				name = assembly.GetName(false);
			}
			else
			{
                pathOrPartialAssemblyName = Path.GetFullPath(pathOrPartialAssemblyName);
				name = AssemblyName.GetAssemblyName(pathOrPartialAssemblyName);
			}

			sb.Append(name.Name);
			sb.Append(@"\");

			sb.Append(name.Version);
			sb.Append("__");

			if(name.GetPublicKeyToken() == null)
				throw(new System.Exception("No public key token present - is assembly strong-named?"));

			sb.Append(BytesToHex(name.GetPublicKeyToken()));
			sb.Append(@"\");

			return sb.ToString();
		}

		private static char[] hexDigits = {
														 '0', '1', '2', '3', '4', '5', '6', '7',
														 '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		private static string BytesToHex(byte[] bytes)
		{

			char[] chars = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++) 
			{
				int b = bytes[i];
				chars[i * 2] = hexDigits[b >> 4];
				chars[i * 2 + 1] = hexDigits[b & 0xF];
			}
			return new string(chars);
    
		}
	}
}
