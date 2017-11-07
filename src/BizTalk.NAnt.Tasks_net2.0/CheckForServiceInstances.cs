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
   [TaskName("checkforserviceinstances")]
   class CheckForServiceInstances : Task
   {
      BizTalkOperations _operations;

      string _applicationName;

      public CheckForServiceInstances()
      {
         // connect to the BizTalk configuration database that corresponds to our group membership.
         _operations = new BizTalkOperations(BizTalkGroupInfo.GroupDBServerName, BizTalkGroupInfo.GroupMgmtDBName);
      }

      /// <summary>
      /// The application to check for running or suspended instances for.  
      /// </summary>
      [TaskAttribute("application", Required=true)]
      [StringValidator(AllowEmpty = false)]
      public string Application
      {
         get { return _applicationName; }
         set { _applicationName = value; }
      }

      /// <summary>
      /// This function will check to see if there are any service instances for the given
      /// BizTalk application.  If so, it will throw an exception.  
      /// </summary>
      protected override void ExecuteTask()
      {
        // Get all service instances in the message box.
        foreach (Instance instance in _operations.GetServiceInstances())
        {
           // We will only deal with instances that we can determine 
           // which application it belongs to.
           MessageBoxServiceInstance mbsi = instance as MessageBoxServiceInstance;
           if (mbsi != null)
           {
              if (mbsi.Application == Application)
              {
                  throw new BuildException(
                      string.Format(
                        "There is at least one service instance associated with the '{0}' application [Instance Status = {1}]. An application be removed only when there are no associated service instances.",
                        _applicationName,
                        Enum.GetName(typeof(InstanceStatus), mbsi.InstanceStatus)));
              }
           }
        }
      }
   }
}
