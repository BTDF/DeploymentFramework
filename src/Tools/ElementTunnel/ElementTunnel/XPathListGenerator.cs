// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.
//
// Contributions to ElementTunnel by Tim Rayburn
//

using System.Collections.Generic;
using System.IO;

namespace ElementTunnel
{
    public static class XPathListGenerator
    {
        public static List<string> CreateXPathCollection(string fileName)
        {
            List<string> xpaths = new List<string>();

            //First read all xpaths.
            using (StreamReader sr = new StreamReader(fileName))
            {
                string xpath;
                while (!sr.EndOfStream)
                {
                    xpath = sr.ReadLine();
                    if (!string.IsNullOrEmpty(xpath))
                    {
                        xpaths.Add(xpath);
                    }
                }
            }
            return xpaths;
        }
    }
}
