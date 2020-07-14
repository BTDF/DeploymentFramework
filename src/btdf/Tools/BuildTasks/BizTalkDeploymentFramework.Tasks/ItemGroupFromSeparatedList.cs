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
using System.Text.RegularExpressions;

namespace DeploymentFramework.BuildTasks
{
    public class ItemGroupFromSeparatedList : Task
    {
        private char _separator = ',';
        private bool _reverseList;
        private string _separatedList;
        private string _formatString;
        private string _listItemRegex;
        private ITaskItem[] _itemGroup;

        [Output]
        public ITaskItem[] ItemGroup
        {
            get { return _itemGroup; }
        }

        public string SeparatedList
        {
            get { return _separatedList; }
            set { _separatedList = value; }
        }

        public bool ReverseList
        {
            get { return _reverseList; }
            set { _reverseList = value; }
        }

        public string FormatString
        {
            get { return _formatString; }
            set { _formatString = value; }
        }

        public char Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        public string ListItemRegex
        {
            get { return _listItemRegex; }
            set { _listItemRegex = value; }
        }
	
        public override bool Execute()
        {
            List<TaskItem> itemGroup = new List<TaskItem>();

            if (!string.IsNullOrEmpty(_separatedList))
            {
                string[] listEntries =
                    _separatedList.Split(new char[] { _separator }, StringSplitOptions.RemoveEmptyEntries);

                if (_reverseList)
                {
                    Array.Reverse(listEntries);
                }

                bool useFormatString = !string.IsNullOrEmpty(_formatString);
                bool useRegex = !string.IsNullOrEmpty(_listItemRegex);

                foreach (string listEntry in listEntries)
                {
                    string newListEntry = listEntry;

                    if (useFormatString)
                    {
                        newListEntry = string.Format(_formatString, listEntry);
                    }

                    TaskItem ti = new TaskItem(newListEntry);
                    base.Log.LogMessage(MessageImportance.Low, "Creating TaskItem with ItemSpec '{0}'.", newListEntry);

                    if (useRegex)
                    {
                        Regex regex = new Regex(_listItemRegex, RegexOptions.ExplicitCapture | RegexOptions.Singleline);
                        Match match = regex.Match(listEntry);

                        for (int index = 0; index < match.Groups.Count; index++)
                        {
                            string captureName = regex.GroupNameFromNumber(index);
                            ti.SetMetadata(captureName, match.Groups[index].Value);
                            base.Log.LogMessage(MessageImportance.Low, "Adding metadata '{0}':'{1}' to TaskItem.", captureName, match.Groups[index].Value);
                        }
                    }

                    itemGroup.Add(ti);
                }
            }

            _itemGroup = itemGroup.ToArray();

            return true;
        }
    }
}
