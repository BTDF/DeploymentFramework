// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.
//
// Contributions to ElementTunnel by Tim Rayburn
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.IO;

namespace ElementTunnel.Tests
{
    public static class TestExtensions
    {
        public const string encodeTarget = "<yar>123</yar>";
        public const string encodedTarget = "&lt;yar&gt;123&lt;/yar&gt;";

        public static void ShouldBeEncoded(this XmlNode testNode)
        {
            Assert.AreEqual(encodeTarget, testNode.InnerText);
            Assert.AreEqual(encodedTarget, testNode.InnerXml);
        }

        public static void ShouldBe<T>(this T actual, T expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void ForEach(this XmlDocument xDoc, string xPath, Action<XmlNode> action)
        {
            XmlNodeList nodeList = xDoc.SelectNodes(xPath);
            foreach (XmlNode lNode in nodeList)
            {
                action.Invoke(lNode);
            }
        }
    }
}
