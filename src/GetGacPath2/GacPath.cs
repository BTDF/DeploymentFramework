using System;
using System.IO;

using Genghis;
using clp = Genghis.CommandLineParser;

namespace GetGacPath
{

	[clp.ParserUsage("\r\nOutputs a physical location within the GAC directory structure for a particular assembly, and optionaly copies a file to that location.\r\n(Assembly may not actually be in the GAC, in which case directory won't exist.)")]
	class GetGacPathCommandLine : CommandLineParser
	{
		[clp.ValueUsage("Partial assembly name or physical path.", MatchPosition=true,ValueName="partialAssemblyName", Optional=false)]
		public string pathOrPartialAssemblyName = "";

		[clp.ValueUsage("Variant of GAC ('gac' for .net 1.0/1.1, or GAC_MSIL/GAC_32/GAC_64 or .net 2.0)", MatchPosition=true,ValueName="gacVariant", Optional=true)]
		public string gacVariant = "gac";

		[clp.ValueUsage("File to copy to destination directory.", MatchPosition=true,ValueName="fileToCopy", Optional=true)]
		public string fileToCopy = "";

	}


	/// <summary>
	/// Summary description for GacPath.
	/// </summary>
	class GacPath
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			GetGacPathCommandLine cl = new GetGacPathCommandLine();
			if( !cl.ParseAndContinue(args) ) 
				return;

			try
			{
				string gacPath = GetGacPathHelper.GetGacPath.GetPath(
					cl.pathOrPartialAssemblyName,
					cl.gacVariant);
			

				if(cl.fileToCopy != string.Empty)
				{
					string destination = Path.Combine(gacPath,Path.GetFileName(cl.fileToCopy));

					Console.WriteLine("Copying {0} to {1}",
						cl.fileToCopy,
						destination);

					File.Copy(
						cl.fileToCopy,
						destination,true);
				}
				else
				{
					Console.WriteLine(gacPath);
				}
			}
			catch(System.Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
