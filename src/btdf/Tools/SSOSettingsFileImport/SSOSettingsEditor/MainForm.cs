// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SSOSettingsFileManager;

namespace SSOSettingsEditor
{
    public partial class MainForm : Form
    {
        private string _affiliateAppName;
        private bool _isDirty = false;

        private bool IsLoaded
        {
            get { return this.propertyGrid.SelectedObject != null; }
        }

        public MainForm(string affiliateAppName)
        {
            _affiliateAppName = affiliateAppName;

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_affiliateAppName))
            {
                this.appNameTextBox.Text = _affiliateAppName;
                LoadSettingsFromSso();
            }
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            ReloadPropertyGrid();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (_isDirty)
            {
                if (!GetLoseUnsavedChangesAnswer())
                {
                    return;
                }
            }

            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!_isDirty)
            {
                MessageBox.Show(this, "There are no unsaved changes.", "No Unsaved Changes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SSOSettingsManager.WriteSettings(
                this.appNameTextBox.Text,
                ((DictionaryPropertyGridAdapter)(this.propertyGrid.SelectedObject)).Dictionary as Hashtable);

            _isDirty = false;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _isDirty = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty)
            {
                if (!GetLoseUnsavedChangesAnswer())
                {
                    e.Cancel = true;
                }
            }
        }

        private void appNameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                ReloadPropertyGrid();
            }
        }

        private void LoadSettingsFromSso()
        {
            _isDirty = false;

            Hashtable settings = null;

            try
            {
                settings = SSOSettingsManager.GetSettings(this.appNameTextBox.Text, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message, "SSO Read Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.propertyGrid.SelectedObject = new DictionaryPropertyGridAdapter(settings);
            this.propertyGrid.Refresh();
        }

        private bool GetLoseUnsavedChangesAnswer()
        {
            if (MessageBox.Show(
                this,
                "You have unsaved changes. Continue without saving and lose changes?",
                "Lose Unsaved Changes?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                return true;
            }

            return false;
        }

        private void ReloadPropertyGrid()
        {
            if (_isDirty)
            {
                if (!GetLoseUnsavedChangesAnswer())
                {
                    return;
                }
            }

            this.propertyGrid.SelectedObject = null;
            LoadSettingsFromSso();
        }
    }
}
