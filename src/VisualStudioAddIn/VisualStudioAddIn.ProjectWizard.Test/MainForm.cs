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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DeploymentFramework.VisualStudioAddIn.ProjectWizard;

namespace VisualStudioAddIn.ProjectWizard.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DeploymentOptions options = new DeploymentOptions();
            options.IncludeBam = true;

            OptionsForm of = new OptionsForm(options);
            of.ShowDialog();

        }
    }
}
