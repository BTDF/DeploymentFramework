using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.BizTalk.ExplorerOM;
using BizTalk.NAnt.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace BizTalk.NAnt.Tasks
{
    [TaskName("controlbiztalkapp")]
    class ControlBizTalkApp : Task
    {
        private string _applicationName;
        private string _startOption;
        private string _stopOption;
        BtsCatalogExplorer _catalog = null;

        public ControlBizTalkApp()
        {
            // connect to the BizTalk configuration database that corresponds to our group membership.
            _catalog = new BtsCatalogExplorer();
            _catalog.ConnectionString = string.Format("Server={0};Initial Catalog={1};Integrated Security=SSPI;",
               BizTalkGroupInfo.GroupDBServerName,
               BizTalkGroupInfo.GroupMgmtDBName);
        }

        /// <summary>
        /// A member of ApplicationStartOption enumeration
        /// </summary>
        [TaskAttribute("startOption")]
        [StringValidator(AllowEmpty = false)]
        public string StartOption
        {
            get { return _startOption; }
            set { _startOption = value; }
        }

        /// <summary>
        /// A member of ApplicationStopOption enumeration
        /// </summary>
        [TaskAttribute("stopOption")]
        [StringValidator(AllowEmpty = true)]
        public string StopOption
        {
            get { return _stopOption; }
            set { _stopOption = value; }
        }

        [TaskAttribute("application")]
        [StringValidator(AllowEmpty = true)]
        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        protected override void ExecuteTask()
        {
            int retryCount = 5;

            if ((string.IsNullOrEmpty(_startOption) && string.IsNullOrEmpty(_stopOption))
                || (!string.IsNullOrEmpty(_startOption) && !string.IsNullOrEmpty(_stopOption)))
            {
                throw new BuildException("Please specify either StartOption or StopOption.", this.Location);
            }

            Application application = _catalog.Applications[_applicationName];
            if (application == null)
            {
                this.Log(Level.Warning, "Unable to find application '" + _applicationName + "' in catalog.", this.Location);
                return;
            }

            ApplicationStartOption startOption = 0;
            ApplicationStopOption stopOption = 0;

            if (!string.IsNullOrEmpty(_startOption))
            {
                startOption = ParseStartEnum(_startOption);
            }
            else
            {
                stopOption = ParseStopEnum(_stopOption);
            }

            for (int i = 0; i < retryCount; i++)
            {
                this.Log(Level.Info, string.Format("(Retry count {0})", i));
                try
                {
                    if (startOption != 0)
                    {
                        this.Log(Level.Info, string.Format("Starting {0} application...\n", _applicationName));
                        application.Start(startOption);
                    }
                    else
                    {
                        this.Log(Level.Info, string.Format("Stopping {0} application...\n", _applicationName));
                        application.Stop(stopOption);
                    }

                    _catalog.SaveChanges();
                    break;
                }
                catch (Microsoft.BizTalk.ExplorerOM.BtsException ex)
                {
                    try
                    {
                        _catalog.DiscardChanges();
                    }
                    catch { }

                    if (!ex.Message.Contains("deadlocked"))
                    {
                        throw new BuildException(ex.Message, this.Location);
                    }
                }
            }
        }

        private ApplicationStartOption ParseStartEnum(string enumComponents)
        {
            ApplicationStartOption result = 0;

            string[] enumComponentsSplit = enumComponents.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string enumComponent in enumComponentsSplit)
            {
                result |= (ApplicationStartOption)Enum.Parse(typeof(ApplicationStartOption), enumComponent);
            }

            return result;
        }

        private ApplicationStopOption ParseStopEnum(string enumComponents)
        {
            ApplicationStopOption result = 0;

            string[] enumComponentsSplit = enumComponents.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string enumComponent in enumComponentsSplit)
            {
                result |= (ApplicationStopOption)Enum.Parse(typeof(ApplicationStopOption), enumComponent);
            }

            return result;
        }
    }
}
