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
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace ElementTunnel
{
    public class ElementTunneler
    {
        private readonly char[] Whitespace = { ' ', '\t', '\n', '\v', '\f', '\r' };

        private bool _verboseLogging;
        private Regex _xmlPI = new Regex(@"<\?xml .+\?>\r?\n?");

        public int MatchedNodes { get; set; }
        public int EncDecodedNodes { get; set; }
        public int EmptyNodes { get; set; }
        public int AlreadyEncDecNodes { get; set; }

        public ElementTunneler()
        {
        }

        public void TunnelXPaths(XmlDocument inDoc, List<string> xpaths, bool isEncode, bool verboseLogging)
        {
            _verboseLogging = verboseLogging;

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
                        WriteLineToConsole("No nodes found for '{0}' - skipping.", xpath);
                        WriteToConsole();
                        continue;
                    }

                    IterateNodes(isEncode, iter);
                }
                else
                {
                    WriteLineToConsole("No node found for '{0}'.", xpath);
                    WriteToConsole();
                }
            }
        }

        private void IterateNodes(bool isEncode, XPathNodeIterator iterator)
        {
            XmlNode nodeToWorkOn = null;

            while (iterator.MoveNext())
            {
                MatchedNodes++;
                nodeToWorkOn = ((IHasXmlNode)iterator.Current).GetNode();
                WriteToConsole("Found node: {0} - ", BuildPath(nodeToWorkOn));

                if (isEncode)
                {
                    EncodeNode(nodeToWorkOn);
                }
                else
                {
                    DecodeNode(nodeToWorkOn);
                }

                WriteToConsole();
            }
        }

        private void DecodeNode(XmlNode node)
        {
            if (string.IsNullOrEmpty(node.InnerText))
            {
                EmptyNodes++;
                WriteLineToConsole("skipping (empty)");
                return;
            }

            string innerXmlTrimmed = node.InnerXml.TrimStart(Whitespace).TrimEnd(Whitespace);

            if (!innerXmlTrimmed.StartsWith("<"))
            {
                EncDecodedNodes++;
                WriteLineToConsole("decoding");

                string innerText = node.InnerText;
                // Search & Destroy xml processing instructions inside escaped xml
                innerText = _xmlPI.Replace(innerText, string.Empty);
                node.InnerXml = innerText;
            }
            else
            {
                AlreadyEncDecNodes++;
                WriteLineToConsole("skipping (already decoded)");
            }
        }

        private void EncodeNode(XmlNode node)
        {
            if (string.IsNullOrEmpty(node.InnerXml))
            {
                EmptyNodes++;
                WriteLineToConsole("skipping (empty)");
                return;
            }

            // Trim all whitespace chars from the start and end of the inner XML. When the nested XML is eventually extracted
            // and deserialized through an XmlSerializer or loaded into a DOM, whitespace at the beginning or end of the value
            // will generally cause a failure.
            string innerXmlTrimmed = node.InnerXml.TrimStart(Whitespace).TrimEnd(Whitespace);

            // If the inner XML is already encoded, don't encode it again
            if (!innerXmlTrimmed.StartsWith("&lt;"))
            {
                EncDecodedNodes++;
                WriteLineToConsole("encoding");

                node.InnerText = innerXmlTrimmed;
            }
            else
            {
                AlreadyEncDecNodes++;
                WriteLineToConsole("skipping (already encoded)");
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

        private void WriteToConsole()
        {
            if (_verboseLogging)
            {
                Console.WriteLine();
            }
        }

        private void WriteLineToConsole(string msg, params object[] arg)
        {
            if (_verboseLogging)
            {
                Console.WriteLine(msg, arg);
            }
        }

        private void WriteToConsole(string msg, params object[] arg)
        {
            if (_verboseLogging)
            {
                Console.Write(msg, arg);
            }
        }
    }
}
