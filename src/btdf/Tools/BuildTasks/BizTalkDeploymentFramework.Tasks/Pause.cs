// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Drawing;

namespace DeploymentFramework.BuildTasks
{
    public class Pause : Task
    {
        private enum MessageTypeEnum
        {
            Normal,
            Warning,
            Error
        }

        private MessageTypeEnum _messageType = MessageTypeEnum.Normal;
        private string _message;

        [Required]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string MessageType
        {
            get { return _messageType.ToString(); }
            set
            {
                if (!Enum.IsDefined(typeof(MessageTypeEnum), value))
                {
                    throw new ArgumentOutOfRangeException("value", "Mode must be Normal, Warning or Error.");
                }

                _messageType = (MessageTypeEnum)Enum.Parse(typeof(MessageTypeEnum), value);
            }
        }

        public override bool Execute()
        {
            switch (_messageType)
            {
                case MessageTypeEnum.Error:
                    base.Log.LogError(_message);
                    break;
                case MessageTypeEnum.Warning:
                    base.Log.LogWarning(_message);
                    break;
                default:
                    base.Log.LogMessage(_message);
                    break;
            }

            Console.ReadKey(true);

            return true;
        }
    }
}
