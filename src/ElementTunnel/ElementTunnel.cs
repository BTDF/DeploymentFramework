using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

using Genghis;
using clp = Genghis.CommandLineParser;

namespace ElementTunnel
{

	[clp.ParserUsage("\nEncodes/Decodes the child content of xml element(s) (located with an xpath)\nwithin an xml document.\n(That is, it applies escaping rules such as &gt; for '<')\nNamespace declarations, PIs, etc. of nested documents are not preserved.")]
	class ElementTunnelCommandLine : CommandLineParser
	{
		[clp.ValueUsage("Location of input file.", MatchPosition=false,ValueName="inputFile",Name="i", Optional=false)]
		public string inputFile;

		[clp.ValueUsage("Location of file with xpaths to process", MatchPosition=false,ValueName="fileWithXPaths", Name="x", Optional=false)]
		public string fileWithXPaths;

		[clp.ValueUsage("Location of output file.", MatchPosition=false,ValueName="outputFile", Name="o", Optional=false)]
		public string outputFile;

		[clp.FlagUsage("Encode the elements.",MatchPosition=false,Optional=true)]
		public bool encode=false;

		[clp.FlagUsage("Decode the elements.",MatchPosition=false,Optional=true)]
		public bool decode=false;


	}

	class ElementTunnelUtil
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			try
			{
				//Search & Destroy xml processing instructions inside escaped xml.
				Regex xmlPI = new Regex(@"<\?xml .+\?>\r?\n?");

				ElementTunnelCommandLine cl = new ElementTunnelCommandLine();
				if( !cl.ParseAndContinue(args) ) 
					return 1;

				if((cl.encode && cl.decode) || (!cl.encode && !cl.decode))
				{
					Console.Error.WriteLine("Specify encode or decode.");
					return 1;
				}

				XmlDocument inDoc = new XmlDocument();
				inDoc.Load(cl.inputFile);

				ArrayList xpaths = new ArrayList();

				//First read all xpaths.
				using (StreamReader sr = new StreamReader(cl.fileWithXPaths))
				{
					string xpath = string.Empty;
					while((xpath = sr.ReadLine()) != null)
					{
						if(!xpath.Equals(string.Empty))
						{
							xpaths.Add(xpath);
						}
					}
				}

				//When decoding double escaped xml paths should be handled in reverse.
				if(cl.decode)
				{
					xpaths.Reverse();
				}

				foreach(string xpath in xpaths)
				{
					XPathNavigator nav = inDoc.CreateNavigator();
					object eval = nav.Evaluate(xpath);

					if(eval is XPathNodeIterator)
					{
						XPathNodeIterator iter = eval as XPathNodeIterator;

						if(iter.Count == 0)
						{
							Console.Error.WriteLine("No nodes were found for the supplied xpath expression {0} - skipping.",xpath);
						}

						XmlNode nodeToWorkOn = null;
						while(iter.MoveNext())
						{
							nodeToWorkOn = ((IHasXmlNode)iter.Current).GetNode();
							Console.WriteLine("\nOperating on node: {0}\n",nodeToWorkOn.OuterXml);

							if(cl.encode)
								nodeToWorkOn.InnerText = nodeToWorkOn.InnerXml; // System.Security.SecurityElement.Escape(nodeToWorkOn.InnerXml);

							if(cl.decode)
							{
								string text = System.Web.HttpUtility.UrlDecode(nodeToWorkOn.InnerText);
								text = xmlPI.Replace(text, string.Empty);
								nodeToWorkOn.InnerXml = text;
							}
						}
		
					}
					else
					{
						Console.Error.WriteLine("No node was found for the supplied xpath expression: {0}",xpath);
					}
				}

				inDoc.Save(cl.outputFile);
				Console.WriteLine("\nComplete - output file has been saved.");
            
				return 0;
			}
			catch(Exception exception)
			{
				Console.Error.WriteLine("Error:");
				while(exception != null)
				{
					Console.Error.WriteLine(exception.Message);
					exception = exception.InnerException;
				}
				return 1;
			}
		}
	}
}
