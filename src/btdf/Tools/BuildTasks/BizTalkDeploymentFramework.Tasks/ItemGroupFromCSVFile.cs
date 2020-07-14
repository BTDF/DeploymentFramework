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
using System.IO;

namespace DeploymentFramework.BuildTasks
{
    public class ItemGroupFromCSVFile : Task
    {
        private string _columnNames;
        private string _filename;
        private ITaskItem[] _itemGroup;

        [Output]
        public ITaskItem[] ItemGroup
        {
            get { return _itemGroup; }
        }

        [Required]
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        [Required]
        public string ColumnNames
        {
            get { return _columnNames; }
            set { _columnNames = value; }
        }

        public override bool Execute()
        {
            List<TaskItem> itemGroup = new List<TaskItem>();

            base.Log.LogMessage(
                MessageImportance.Normal,
                "Creating item group from CSV file '{0}' using column names '{1}'.", _filename, _columnNames);

            string[] columnNames = _columnNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            using (StreamReader sr = new StreamReader(_filename))
            {
                string line;

                // Read lines until the end of the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokens = line.Split(new char[] { ',' }, StringSplitOptions.None);

                    if (tokens.Length != columnNames.Length)
                    {
                        base.Log.LogError(
                            "The number of CSV columns detected does not match the number of column names specified. Line: {0}", line);
                        return false;
                    }

                    TaskItem ti = new TaskItem("*");

                    for (int index = 0; index < tokens.Length; index++)
                    {
                        ti.SetMetadata(columnNames[index], tokens[index]);
                    }
                    
                    itemGroup.Add(ti);
                }
            }

            _itemGroup = itemGroup.ToArray();

            return true;
        }
    }
}
