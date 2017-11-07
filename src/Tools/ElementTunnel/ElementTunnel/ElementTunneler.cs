// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.
//
// Contributions to ElementTunnel by Tim Rayburn
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace ElementTunnel
{
    public class ElementTunneler
    {
        private Regex _xmlPI;

        public ElementTunneler()
        {
            // Search & Destroy xml processing instructions inside escaped xml
            _xmlPI = new Regex(@"<\?xml .+\?>\r?\n?");
        }

        public void TunnelXPaths(XmlDocument inDoc, List<string> xpaths, bool isEncode)
        {
            //When decoding double escaped xml paths should be handled in reverse.
            if (!isEncode)
            {
                xpaths.Reverse();
            }

            foreach (string xpath in xpaths)
            {
                XPathNavigator nav = inDoc.CreateNavigator();
                XPathNodeIterator iter = nav.Evaluate(xpath) as XPathNodeIterator;

                if (iter != null)
                {
                    if (iter.Count == 0)
                    {
                        Console.WriteLine("No nodes found for the XPath '{0}' - skipping.", xpath);
                        Console.WriteLine();
                    }

                    IterateNodes(isEncode, iter);
                }
                else
                {
                    Console.WriteLine("No node found for the XPath '{0}'.", xpath);
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\nComplete - output file has been saved.");
        }

        private void IterateNodes(bool isEncode, XPathNodeIterator iterator)
        {
            XmlNode nodeToWorkOn = null;

            while (iterator.MoveNext())
            {
                nodeToWorkOn = ((IHasXmlNode)iterator.Current).GetNode();
                Console.WriteLine("Found node: {0}", BuildPath(nodeToWorkOn));
                Console.WriteLine();

                if (isEncode)
                {
                    EncodeNode(nodeToWorkOn);
                }
                else
                {
                    DecodeNode(nodeToWorkOn);
                }
            }
        }

        private void DecodeNode(XmlNode inNode)
        {
            string text = inNode.InnerText.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'").Replace("&quot;", "\"");
            text = _xmlPI.Replace(text, string.Empty);
            inNode.InnerXml = text;
        }

        private void EncodeNode(XmlNode inNode)
        {
            if (!inNode.InnerXml.Equals(string.Empty))
            {
                inNode.InnerText = inNode.InnerXml;
            }
        }

        private string BuildPath(XmlNode node)
        {
            string path = string.Empty;

            while (node != null && node.LocalName != "#document")
            {
                path = "/" + node.LocalName + path;
                node = node.ParentNode;
            }

            return path;
        }
    }
}
