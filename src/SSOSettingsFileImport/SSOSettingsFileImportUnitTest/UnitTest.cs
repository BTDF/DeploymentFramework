using System;
using System.Collections;
using fm = SSOSettingsFileManager;

namespace SSOSettingsFileImportUnitTest
{
   /// <summary>
   /// Summary description for UnitTest.
   /// </summary>
   class UnitTest
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main(string[] args)
      {
         Hashtable ht = fm.SSOSettingsFileReader.Read("BizTalkSample");
         string test = (string)ht["FileSendLocation"];
         Console.WriteLine(test);
         System.Threading.Thread.Sleep(15000);
         ht = fm.SSOSettingsFileReader.Read("BizTalkSample");

         ht["FileSendLocation"] = "updated";
         fm.SSOSettingsFileReader.Update("BizTalkSample", ht);
         test = (string)ht["FileSendLocation"];
         Console.WriteLine(test);

         Hashtable ht2 = fm.SSOSettingsFileReader.Read("BizTalkSample");
         string test2 = (string)ht2["FileSendLocation"];
         Console.WriteLine(test2);

         fm.SSOSettingsFileReader.ClearCache("BizTalkSample");
         string test3 = fm.SSOSettingsFileReader.ReadString("BizTalkSample", "FileSendLocation");
         Console.WriteLine(test3);

         try
         {
            Hashtable ht3 = SSOSettingsFileReader.Read("Bogus");
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.ToString());
         }


         Console.ReadLine();
      }
   }
}
