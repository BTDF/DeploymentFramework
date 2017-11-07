using System;
using System.Collections.Generic;
using System.Text;

using BizTalk.NAnt.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using Microsoft.BizTalk.Operations;

namespace BizTalk.NAnt.Tasks
{
   [TaskName("terminateserviceinstances")]
   class TerminateServiceInstances : Task
   {
      BizTalkOperations _operations;

      string _applicationName;

      public TerminateServiceInstances()
      {
         // connect to the BizTalk configuration database that corresponds to our group membership.
         _operations = new BizTalkOperations(BizTalkGroupInfo.GroupDBServerName, BizTalkGroupInfo.GroupMgmtDBName);
      }
      /// <summary>
      /// The application to remove running or suspended instances for.  "*" will remove
      /// all running or suspended instances for all applications.
      /// </summary>
      [TaskAttribute("application", Required=true)]
      [StringValidator(AllowEmpty = false)]
      public string Application
      {
         get { return _applicationName; }
         set { _applicationName = value; }
      }

      protected override void ExecuteTask()
      {
         bool removeAll = false;
         if ((_applicationName == null) || (_applicationName.Length == 0))
         {
            throw new ArgumentNullException("Application", "Application property must be set");
         }
         else
         {
            removeAll = (_applicationName == "*");
            // Get all service instances in the message box.
            foreach (Instance instance in _operations.GetServiceInstances())
            {
               // We will only deal with instances that we can determine 
               // which application it belongs to.
               MessageBoxServiceInstance mbsi = instance as MessageBoxServiceInstance;
               if (mbsi != null)
               {
                  // Only terminate if the service instance application matches the 
                  // application we are intrested in.  "*" will match all applications.
                  bool removeThisInstance = ((removeAll == true) || (mbsi.Application == Application));
                  if (removeThisInstance == true)
                  {
                     // Look for all types of running and suspended statuses.
                     bool running = ((mbsi.InstanceStatus & InstanceStatus.RunningAll) != InstanceStatus.None);
                     bool suspended = ((mbsi.InstanceStatus & InstanceStatus.SuspendedAll) != InstanceStatus.None);

                     if (running || suspended)
                     {
                        CompletionStatus status = _operations.TerminateInstance(mbsi.ID);
                        if (status != CompletionStatus.Succeeded)
                        {
                           this.Log(Level.Warning, string.Format("Instance {0} was not successfully terminated.  Status is {1}", mbsi.URI, status.ToString()));
                        }
                     }
                  }
               }
            }
         }
      }
   }
}
