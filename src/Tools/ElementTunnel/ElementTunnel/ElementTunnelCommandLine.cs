// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.
//
// Contributions to ElementTunnel by Tim Rayburn
//

using System;
using Genghis;
using clp = Genghis.CommandLineParser;

namespace ElementTunnel
{
    [clp.ParserUsage("\nEncodes/decodes child content of XML element(s) located with an XPath\nwithin an XML document. It applies escaping rules such as &gt; for '<'.\nNamespace declarations, PIs, etc. of nested documents are not preserved.")]
    class ElementTunnelCommandLine : CommandLineParser
    {
        [clp.ValueUsage("Location of input file.", MatchPosition = false, ValueName = "inputFile", Name = "i", Optional = false)]
        public string inputFile = null;

        [clp.ValueUsage("Location of file with xpaths to process", MatchPosition = false, ValueName = "fileWithXPaths", Name = "x", Optional = false)]
        public string fileWithXPaths = null;

        [clp.ValueUsage("Location of output file.", MatchPosition = false, ValueName = "outputFile", Name = "o", Optional = false)]
        public string outputFile = null;

        [clp.FlagUsage("Encode the elements.", MatchPosition = false, Optional = true)]
        public bool encode = false;

        [clp.FlagUsage("Decode the elements.", MatchPosition = false, Optional = true)]
        public bool decode = false;

        public void PrintLogo()
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(base.GetLogo());
            Console.ForegroundColor = originalColor;
        }
    }
}
