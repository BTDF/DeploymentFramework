// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.IO;
using System.Configuration;
using System.Net;
using System.Web;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace BizTalkSample.DeploymentTest
{
    [TestFixture]
    public class VerifyDeployment
    {
        public VerifyDeployment()
        {
        }

        [Test]
        public void TestEchoViaHttpAdapter()
        {
            // Don't take this sample as good nunit practice - this is simply a placeholder for what
            // a deployment verification might look like.
            string fileName = @"TestFiles\S1_output.xml";

            if (!File.Exists(fileName))
            {
                fileName = @"..\..\..\TestFiles\S1_output.xml";
            }

            string response = PostRequest("http://localhost/BTDFAdvancedSample/btshttpreceive.dll", fileName, true);
            StreamReader sr = new System.IO.StreamReader(fileName);
            string expected = sr.ReadToEnd();

            Assert.IsTrue(response.Equals(expected), "Echo didn't echo.  Got: " + response);
        }

        [Test]
        public void TestTopLevelOrch()
        {
            // Go get send port location...
            string fileLoc = @"Deployment\PortBindings.xml";

            if (!File.Exists(fileLoc))
            {
                fileLoc = @"..\..\..\Deployment\PortBindings.xml";
            }

            XPathDocument doc = new XPathDocument(fileLoc);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator sendIter = (XPathNodeIterator)nav.Select("/BindingInfo/SendPortCollection/SendPort[@Name=\"Advanced_Send_FILE\"]/PrimaryTransport/Address");
            sendIter.MoveNext();
            string location = Path.GetDirectoryName(sendIter.Current.Value);
            Console.WriteLine("Looking at send address: " + location);

            // Make sure we aren't fooled by old files.
            string[] oldFiles = Directory.GetFiles(location);
            foreach (string file in oldFiles)
            {
                File.Delete(file);
            }

            string testFileLoc = @"TestFiles\S1_output.xml";
            string outFileLoc = @"InDir\S1_output.xml";

            if (!File.Exists(testFileLoc))
            {
                testFileLoc = @"..\..\..\TestFiles\S1_output.xml";
                outFileLoc = @"..\..\..\InDir\S1_output.xml";
            }

            File.Copy(testFileLoc, outFileLoc, true);
            File.SetAttributes(outFileLoc, FileAttributes.Normal);
            System.Threading.Thread.Sleep(new TimeSpan(0, 0, 15));

            string[] files = Directory.GetFiles(location);
            if (files.Length == 0)
            {
                throw (new Exception("File didn't appear in output direcory within 15 seconds..."));
            }
        }

        private string PostRequest(string url, string fileName, bool failOnError)
        {
            string retVal = string.Empty;
            
            using (StreamReader sr = new System.IO.StreamReader(fileName))
            {
                NUnitUtility.HttpPost(url, sr.ReadToEnd(), out retVal, true);
                return retVal;
            }
        }
    }
}