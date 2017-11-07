// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.
//
// Contributions to ElementTunnel by Tim Rayburn
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ElementTunnel
{
	class Program
	{
        private const int INT_ErrorReturn = 1;
        private const int INT_SuccessReturn = 0;

        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static int Main(string[] args)
        {
            try
            {
                ElementTunnelCommandLine cl = new ElementTunnelCommandLine();

                if (!cl.ParseAndContinue(args))
                {
                    return INT_ErrorReturn;
                }

                if ((cl.encode && cl.decode) || (!cl.encode && !cl.decode))
                {
                    Console.WriteLine(cl.GetUsage("ERROR: Specify either encode or decode."));
                    return INT_ErrorReturn;
                }

                cl.PrintLogo();

                List<string> xPaths = XPathListGenerator.CreateXPathCollection(cl.fileWithXPaths);

                ElementTunneler etun = new ElementTunneler();

                XmlDocument inDoc = new XmlDocument();
                inDoc.PreserveWhitespace = true;
                
                inDoc.Load(cl.inputFile);

                etun.TunnelXPaths(inDoc, xPaths, cl.encode, cl.verbose);

                inDoc.Save(cl.outputFile);

                Console.WriteLine("XPaths                        : " + xPaths.Count.ToString());
                Console.WriteLine("Transformed Nodes             : " + etun.EncDecodedNodes.ToString());
                Console.WriteLine("Empty Nodes (skipped)         : " + etun.EmptyNodes.ToString());
                Console.WriteLine("No Transform Req'd (skipped)  : " + etun.AlreadyEncDecNodes.ToString());
                Console.WriteLine("Total Matched Nodes           : " + etun.MatchedNodes.ToString());

                Console.WriteLine("\nComplete - output file has been saved.");

                return INT_SuccessReturn;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("Error:");
                Console.Error.WriteLine(exception.ToString());
                return INT_ErrorReturn;
            }
        }
	}
}
