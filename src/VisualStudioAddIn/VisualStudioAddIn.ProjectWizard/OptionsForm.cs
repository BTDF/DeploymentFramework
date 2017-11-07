// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DeploymentFramework.VisualStudioAddIn.ProjectWizard
{
    public partial class OptionsForm : Form
    {
        internal DeploymentOptions Options { get; set; }
        internal bool WritePropertiesOnlyWhenNonDefault
        {
            get { return writeOnlyWhenNonDefault.Checked; }
        }

        public OptionsForm(DeploymentOptions options)
        {
            this.Options = options;
            InitializeComponent();
            this.propertyGrid1.SelectedObject = this.Options;
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
