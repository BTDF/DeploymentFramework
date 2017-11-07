// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
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
    [clp.ParserUsage("\nXML encodes/decodes content of element(s) by XPath. Applies escaping rules such\nas &gt; for '<'. Namespace decls, PIs, etc. of nested XML are not preserved.")]
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

        [clp.FlagUsage("Verbose output.", MatchPosition = false, Optional = true)]
        public bool verbose = false;

        public void PrintLogo()
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(base.GetLogo());
            Console.ForegroundColor = originalColor;
        }
    }
}
