// Scott Colestock / traceofthought.net

using System;
using System.IO;
using System.Xml;
using System.Net;

using System.Xml.XPath;
using System.Collections;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.NAnt.Tasks
{
	/// <summary>
	/// This class is used for starting (or removing) all of the receive ports, send ports, and send port
	/// groups that are found in a BizTalk bnding file.
	/// </summary>
   [TaskName("controlbiztalkports")]
   public class ControlBizTalkPorts : Task
   {
      private string _bindingFile;
      private bool _startMode;
      private string _excludeFromStart = string.Empty;
      private string[] _excludeFromStartArray = {};
      private BtsCatalogExplorer _catalog;

      public ControlBizTalkPorts()
      {
         // connect to the BizTalk configuration database that corresponds to our group membership.
         _catalog = new BtsCatalogExplorer();
         _catalog.ConnectionString = string.Format("Server={0};Initial Catalog={1};Integrated Security=SSPI;",
            BizTalkGroupInfo.GroupDBServerName,
            BizTalkGroupInfo.GroupMgmtDBName);
      }

      /// <summary>
      /// The mode attribute indicates if we are starting the ports ("start") or removing the ports ("remove")
      /// </summary>
      [TaskAttribute("mode", Required=true)]
      [StringValidator(AllowEmpty=false)]
      public string Mode
      {
         get{ return _startMode?"start":"remove";}
         set
         {
            if(value != "remove" && value != "start")
               throw(new BuildException("mode attribute must have value of start or remove.", this.Location));

            _startMode = value == "start"?true:false;
         }
      }

      /// <summary>
      /// The binding file attribute indicates which binding file this task will operate on.
      /// </summary>
      [TaskAttribute("bindingfile", Required=true)]
      [StringValidator(AllowEmpty=false)]
      public string BindingFile
      {
         get{ return _bindingFile;}
         set{ _bindingFile = value;}
      }


      /// <summary>
      /// Comma separated list of receive locations, send ports, and send port groups that will be
      /// excluded from the Start/Enlist operations.
      /// </summary>
      [TaskAttribute("excludefromstart", Required=false)]
      [StringValidator(AllowEmpty=true)]
      public string ExcludeFromStart
      {
         get{ return _excludeFromStart;}
         set{
            _excludeFromStart = value;
            _excludeFromStartArray = _excludeFromStart.Split(new char[]{','});
         }
      }

      protected override void ExecuteTask()
      {
         PropertyDictionary props = this.Properties;
         string fileLoc = _bindingFile;
         XPathDocument doc = new XPathDocument(fileLoc);
         XPathNavigator nav = doc.CreateNavigator();

         this.Log(Level.Info,"\ncontrolbiztalkports:\n");

         if(_startMode)
            this.Log(Level.Info,"Enabling/Starting...\n");
         else
            this.Log(Level.Info,"Unenlisting/Removing...\n");

         try
         {
            DoReceivePorts(nav);

				if(_startMode)
				{
					DoSendPorts(nav);           
					DoSendPortGroups(nav);
				}
				else
				{
					DoSendPortGroups(nav);
					DoSendPorts(nav);           
				}

            this.Log(Level.Info,"(Committing changes to the catalog)...\n");
            _catalog.SaveChanges();
         }
         catch(System.Exception ex)
         {
            this.Log(Level.Info,"(Discarding changes to the catalog)...\n");
            this.Log(Level.Info,ex.ToString());
            _catalog.DiscardChanges();
            throw new BuildException("Failed to control ports.", this.Location, ex);
         }
      }

      void DoSendPortGroups(XPathNavigator nav)
      {
         XPathNodeIterator sendIter = (XPathNodeIterator)nav.Select("/BindingInfo/DistributionListCollection/DistributionList/@Name");
         while(sendIter.MoveNext())
         {
            SendPortGroup sendPortGroup = _catalog.SendPortGroups[sendIter.Current.Value];

            if(sendPortGroup == null)
            {
               if(_startMode)
               {
                  throw(new BuildException(string.Format("Send port group {0} not found, unable to start group.",sendIter.Current.Value), this.Location));
               }
               else
               {
                  this.Log(Level.Warning,"Send port group {0} not found, not stopping group.",sendIter.Current.Value);
                  return;
               }
            }

            // If we are not in the exclusion list, or we happen to be stopping.
            if(Array.IndexOf(_excludeFromStartArray,sendIter.Current.Value) == -1 || !_startMode)
            {
               this.Log(Level.Info,"Send port group: {0}\n",sendIter.Current.Value);
               sendPortGroup.Status = _startMode == true?PortStatus.Started:PortStatus.Bound;
            }
            else
            {
               sendPortGroup.Status = PortStatus.Stopped;
               this.Log(Level.Info,"Send port group bound/stopped: {0}\n",sendIter.Current.Value);
            }

            // Delete the send port group
            if(!_startMode)
            {
					// Remove send ports from the group first...
					ArrayList removals = new ArrayList();
					foreach(SendPort port in sendPortGroup.SendPorts)
					{
						bool portInBindingFile = 
							(bool)nav.Evaluate(string.Format(
								"boolean(count(/BindingInfo/SendPortCollection/SendPort[@Name='{0}']) > 0)",
								port.Name));

						if(portInBindingFile)
						{
							removals.Add(port);
						}
					}

					foreach(SendPort port in removals)
						sendPortGroup.SendPorts.Remove(port);

					if(sendPortGroup.SendPorts.Count == 0)
					{
						this.Log(Level.Info,"Send port group empty, so removing: {0}\n",sendIter.Current.Value);
						_catalog.RemoveSendPortGroup(sendPortGroup);
					}
					else
					{
						this.Log(Level.Info,"Send port group is not empty, so not removing: {0}\n",sendIter.Current.Value);
					}
            }
         }
      }

      void DoSendPorts(XPathNavigator nav)
      {
         XPathNodeIterator sendIter = (XPathNodeIterator)nav.Select("/BindingInfo/SendPortCollection/SendPort/@Name");
         while(sendIter.MoveNext())
         {
            SendPort sendPort = _catalog.SendPorts[sendIter.Current.Value];

            if(sendPort == null)
            {
               if(_startMode)
               {
                  throw(new BuildException(string.Format("Send port {0} not found, unable to start port.",sendIter.Current.Value), this.Location));
               }
               else
               {
                  this.Log(Level.Warning,"Send port {0} not found, not stopping port.",sendIter.Current.Value);
                  return;
               }
            }

            // If we are not in the exclusion list, or we happen to be stopping.
            if(Array.IndexOf(_excludeFromStartArray,sendIter.Current.Value) == -1 || !_startMode)
            {
               this.Log(Level.Info,"Send port: {0}\n",sendIter.Current.Value);
               sendPort.Status = _startMode == true?PortStatus.Started:PortStatus.Bound;
            }
            else
            {
               sendPort.Status = PortStatus.Stopped;
               this.Log(Level.Info,"Send port bound/stopped: {0}\n",sendIter.Current.Value);
            }

            // Delete the send port
            if(!_startMode)
            {
               _catalog.RemoveSendPort(sendPort);
            }
         }
      }

      void DoReceivePorts(XPathNavigator nav)
      {
         XPathNodeIterator rcvIter = (XPathNodeIterator)nav.Select("/BindingInfo/ReceivePortCollection/ReceivePort/@Name");
         while(rcvIter.MoveNext())
         {
            ReceivePort rcvPort = _catalog.ReceivePorts[rcvIter.Current.Value];
            
            if(rcvPort == null)
            {
               if(_startMode)
               {
                  throw(new BuildException(string.Format("Receive port {0} not found, unable to start port.",rcvIter.Current.Value)));
               }
               else
               {
                  this.Log(Level.Warning,"Receive port {0} not found, not stopping port.",rcvIter.Current.Value);
                  return;
               }
            }

            this.Log(Level.Info,"Receive Port: {0}\n",rcvPort.Name);
            this.Log(Level.Info,"\tReceive locations: \n");
            foreach(ReceiveLocation rcvLocation in rcvPort.ReceiveLocations)
            {
               // If we are not in the exclusion list, or we happen to be stopping.
               if(Array.IndexOf(_excludeFromStartArray,rcvLocation.Name) == -1 || !_startMode)
               {
                  this.Log(Level.Info,"\t{0}\n",rcvLocation.Name);
                  rcvLocation.Enable = _startMode;
               }
               else
               {
                  rcvLocation.Enable = false;
                  this.Log(Level.Info,"\t{0} not enabled.\n",rcvLocation.Name);
               }
            }
                   

            // Delete the receive port.
            if(!_startMode)
            {
               _catalog.RemoveReceivePort(rcvPort);
            }
         }
      }



   }
}
