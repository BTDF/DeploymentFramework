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
   /// <summary>
   /// Summary description for VerifyDeployment.
   /// </summary>
   [TestFixture]
   public class VerifyDeployment
   {
      public VerifyDeployment()
      {
         //
         // TODO: Add constructor logic here
         //
      }

      [TestFixtureSetUp]
      public void Init()
      {
      }

      [TestFixtureTearDown]
      public void Dispose()
      {
      }

      [Test]
      public void TestEcho()
      {
         // Don't take this sample as good nunit practice - this is simply a placeholder for what
         // a deployment verification might look like.
         string fileName = @"testfiles\BizTalkSampleS1_output.xml";
         string response = PostRequest("http://localhost/BizTalkSampleVDir/btshttpreceive.dll", fileName, true);
         StreamReader sr = new System.IO.StreamReader(fileName);
         string expected = sr.ReadToEnd();

         Assert.IsTrue(response.Equals(expected), "Echo didn't echo.  Got: " + response);
      }

      [Test] 
      public void TestTopLevelOrch()
      {
         // Don't take this sample as good nunit practice - this is simply a placeholder for what
         // a deployment verification might look like.  

         // Go get send port location...
         string fileLoc = @"BizTalkSample.PortBindings.xml";
         XPathDocument doc = new XPathDocument(fileLoc);
         XPathNavigator nav = doc.CreateNavigator();
         XPathNodeIterator sendIter = (XPathNodeIterator)nav.Select("/BindingInfo/SendPortCollection/SendPort[@Name=\"BizTalkSample_SampleS2_File\"]/PrimaryTransport/Address");
         sendIter.MoveNext();
         string location = Path.GetDirectoryName(sendIter.Current.Value);
         Console.WriteLine("Looking at send address: " + location);

			// Make sure we aren't fooled by old files.
			string[] oldFiles = Directory.GetFiles(location);
			foreach(string file in oldFiles)
			{
				File.Delete(file);
			}

		   File.Copy(@"testfiles\BizTalkSampleS1_output.xml",@"BizTalkSample_InDir\BizTalkSampleS1_output.xml",true);   
         System.Threading.Thread.Sleep(new TimeSpan(0,0,15));

         string[] files = Directory.GetFiles(location);
         if(files.Length == 0)
         {
            throw(new Exception("File didn't appear in output direcory within 5 seconds..."));
         }
            
      }

		private string PostRequest(string url, string fileName, bool failOnError)
		{
			string retVal = string.Empty;
			StreamReader sr = new System.IO.StreamReader(fileName);
			NUnitUtility.HttpPost(url,sr.ReadToEnd(),out retVal,true);
			return retVal;
		}

      


   }
}