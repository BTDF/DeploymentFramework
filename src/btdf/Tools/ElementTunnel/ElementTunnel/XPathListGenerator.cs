// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
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
