// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.IO;

using Genghis;
using clp = Genghis.CommandLineParser;

namespace SetEnvUI
{

    /// <summary>
    /// Get app config settings into an arraylist, or a collection of serialized classes.
    /// We need from config file: prompt text, environment varname, and boolean for password field
    /// We need an optional command line argument for what process to launch with the 
    /// environment variables in place.
    /// </summary>
    public class SetEnvUIForm : System.Windows.Forms.Form
    {
        [clp.ParserUsage("Auto-generate a wizard UI based on a configuration file.")]
        class SetEnvUIParams : CommandLineParser
        {
            [clp.ValueUsage("Configuration file that defines wizard.", AlternateName1 = "c", ValueName = "configFile", Optional = false)]
            public string configFile;

            [clp.ValueUsage("Process to launch at wizard completion.", AlternateName1 = "p", ValueName = "startProcessPath", Optional = false)]
            public string postProcess = null;

            [clp.ValueUsage("Arguments to process to launch at wizard completion.", AlternateName1 = "a", ValueName = "startProcessArgs", Optional = true)]
            public string postProcessArgs = null;
        }

        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnFinish;
        private System.ComponentModel.Container components = null;

        private SetEnvUIConfig setEnvUIConfig;
        private System.Windows.Forms.Label lblStep;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox chkValue;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private int currentIndex = 0;
        private Panel panelRadio;
        private RadioButton radioButton5;
        private RadioButton radioButton4;
        private RadioButton radioButton3;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private List<RadioButton> radioButtons = new List<RadioButton>();

        private static string currentDirectory;

        public SetEnvUIForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            radioButtons.Add(radioButton1);
            radioButtons.Add(radioButton2);
            radioButtons.Add(radioButton3);
            radioButtons.Add(radioButton4);
            radioButtons.Add(radioButton5);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnFinish = new System.Windows.Forms.Button();
            this.lblStep = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chkValue = new System.Windows.Forms.CheckBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.panelRadio = new System.Windows.Forms.Panel();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelRadio.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            this.lblPrompt.Location = new System.Drawing.Point(174, 38);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(309, 73);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "Please enter a blah blah blah here: ";
            this.lblPrompt.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(176, 132);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(314, 20);
            this.txtValue.TabIndex = 1;
            this.txtValue.Text = "textBox1";
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point(254, 277);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(75, 23);
            this.btnPrevious.TabIndex = 2;
            this.btnPrevious.Text = "< &Previous";
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(333, 277);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "&Next >";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnFinish
            // 
            this.btnFinish.Location = new System.Drawing.Point(414, 276);
            this.btnFinish.Name = "btnFinish";
            this.btnFinish.Size = new System.Drawing.Size(75, 23);
            this.btnFinish.TabIndex = 2;
            this.btnFinish.Text = "&Finish";
            this.btnFinish.Click += new System.EventHandler(this.btnFinish_Click);
            // 
            // lblStep
            // 
            this.lblStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStep.Location = new System.Drawing.Point(172, 12);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(100, 23);
            this.lblStep.TabIndex = 3;
            this.lblStep.Text = "Step 1 of x";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SetEnvUI.Properties.Resources.pictureBox1_Image1;
            this.pictureBox1.InitialImage = global::SetEnvUI.Properties.Resources.pictureBox1_Image1;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(157, 311);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // chkValue
            // 
            this.chkValue.Location = new System.Drawing.Point(176, 132);
            this.chkValue.Name = "chkValue";
            this.chkValue.Size = new System.Drawing.Size(314, 24);
            this.chkValue.TabIndex = 1;
            this.chkValue.Text = "I &Agree";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowse.Location = new System.Drawing.Point(494, 130);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(22, 23);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "...";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "All files|*.*";
            this.openFileDialog.InitialDirectory = "c:\\";
            // 
            // panelRadio
            // 
            this.panelRadio.Controls.Add(this.radioButton5);
            this.panelRadio.Controls.Add(this.radioButton4);
            this.panelRadio.Controls.Add(this.radioButton3);
            this.panelRadio.Controls.Add(this.radioButton2);
            this.panelRadio.Controls.Add(this.radioButton1);
            this.panelRadio.Location = new System.Drawing.Point(177, 123);
            this.panelRadio.Name = "panelRadio";
            this.panelRadio.Size = new System.Drawing.Size(339, 140);
            this.panelRadio.TabIndex = 6;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(4, 104);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(85, 17);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "radioButton5";
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(4, 79);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(85, 17);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "radioButton4";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(4, 54);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(85, 17);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "radioButton3";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(4, 29);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(85, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "radioButton2";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(4, 4);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(85, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "radioButton1";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // SetEnvUIForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(522, 311);
            this.Controls.Add(this.panelRadio);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.chkValue);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblStep);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnFinish);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetEnvUIForm";
            this.Text = "SetEnvUI";
            this.Load += new System.EventHandler(this.SetEnvUIForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelRadio.ResumeLayout(false);
            this.panelRadio.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static SetEnvUIParams setEnvUIParams;
        [STAThread]
        static int Main(string[] args)
        {
            currentDirectory = Environment.CurrentDirectory;
            setEnvUIParams = new SetEnvUIParams();
            if (!setEnvUIParams.ParseAndContinue(args))
                return -1;

            Application.Run(new SetEnvUIForm());
            return 0;
        }

        private void SetEnvUIForm_Load(object sender, System.EventArgs e)
        {
            setEnvUIParams.configFile = Path.GetFullPath(setEnvUIParams.configFile);
            setEnvUIConfig = SetEnvUIConfig.LoadFromFile(setEnvUIParams.configFile);
            if (setEnvUIConfig.ConfigItems.Count == 0)
                throw (new ApplicationException("There are no configuration items in SetEnvUIConfig.xml"));

            if (setEnvUIConfig.ConfigItems.Count > 1)
            {
                this.btnFinish.Enabled = false;
                this.btnPrevious.Enabled = false;
                this.AcceptButton = this.btnNext;
            }
            else
            {
                this.btnNext.Enabled = false;
                this.btnPrevious.Enabled = false;
                this.AcceptButton = this.btnFinish;
            }

            this.Text = setEnvUIConfig.DialogCaption;

            RefreshCurrent();
        }


        private void btnPrevious_Click(object sender, System.EventArgs e)
        {
            UpdateCurrent();

            if (currentIndex > 0)
            {
                currentIndex--;
                RefreshCurrent();
            }

            if (currentIndex < setEnvUIConfig.ConfigItems.Count - 1)
            {
                this.btnNext.Enabled = true;
                this.btnFinish.Enabled = false;
                this.AcceptButton = this.btnNext;
            }

            if (currentIndex == 0)
            {
                this.btnPrevious.Enabled = false;
            }
            else
            {
                this.btnPrevious.Enabled = true;
            }

        }

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            UpdateCurrent();

            if (currentIndex < setEnvUIConfig.ConfigItems.Count - 1)
            {
                currentIndex++;
                RefreshCurrent();
            }

            if (currentIndex < setEnvUIConfig.ConfigItems.Count - 1)
            {
                this.btnNext.Enabled = true;
                this.btnFinish.Enabled = false;
            }
            else
            {
                this.btnNext.Enabled = false;
                this.btnFinish.Enabled = true;
                this.AcceptButton = this.btnFinish;
            }

            if (currentIndex > 0)
            {
                this.btnPrevious.Enabled = true;
            }

        }

        private void btnBrowse_Click(object sender, System.EventArgs e)
        {
            openFileDialog.InitialDirectory = currentDirectory;
            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            this.txtValue.Text = openFileDialog.FileName;
        }


        private void UpdateCurrent()
        {
            switch (setEnvUIConfig[currentIndex].ValueType)
            {
                case SetEnvUIValueType.Password:
                    setEnvUIConfig[currentIndex].PromptValue = this.txtValue.Text;
                    break;

                case SetEnvUIValueType.Text:
                    setEnvUIConfig[currentIndex].PromptValue = this.txtValue.Text;
                    break;

                case SetEnvUIValueType.FileSelect:
                    setEnvUIConfig[currentIndex].PromptValue = this.txtValue.Text;
                    break;

                case SetEnvUIValueType.Checkbox:
                    setEnvUIConfig[currentIndex].PromptValue = this.chkValue.Checked ? "true" : "false";
                    break;

                // Figure out which radio button was chosen...
                case SetEnvUIValueType.RadioButtons:
                    {
                        string name = string.Empty;
                        foreach (RadioButton button in radioButtons)
                        {
                            if (button.Checked)
                            {
                                name = button.Name;
                                break;
                            }
                        }
                        setEnvUIConfig[currentIndex].PromptValue = name;
                        break;
                    }

            }
        }

        private void RefreshCurrent()
        {
            this.txtValue.Text = setEnvUIConfig[currentIndex].PromptValue;
            this.chkValue.Checked = (setEnvUIConfig[currentIndex].PromptValue == "true") ? true : false;
            this.lblPrompt.Text = setEnvUIConfig[currentIndex].PromptText;
            this.lblStep.Text = string.Format("Step {0} of {1}", currentIndex + 1, setEnvUIConfig.ConfigItems.Count);

            switch (setEnvUIConfig[currentIndex].ValueType)
            {
                case SetEnvUIValueType.Password:
                    this.txtValue.PasswordChar = '*';
                    this.txtValue.Visible = true;
                    this.chkValue.Visible = false;
                    this.btnBrowse.Visible = false;
                    this.panelRadio.Visible = false;
                    this.txtValue.Focus();
                    break;

                case SetEnvUIValueType.Text:
                    this.txtValue.PasswordChar = (char)0;
                    this.txtValue.Visible = true;
                    this.chkValue.Visible = false;
                    this.btnBrowse.Visible = false;
                    this.panelRadio.Visible = false;
                    this.txtValue.Focus();
                    break;

                case SetEnvUIValueType.Checkbox:
                    this.txtValue.Visible = false;
                    this.chkValue.Visible = true;
                    this.btnBrowse.Visible = false;
                    this.chkValue.Text = setEnvUIConfig[currentIndex].Caption;
                    this.panelRadio.Visible = false;
                    this.chkValue.Focus();
                    break;

                case SetEnvUIValueType.FileSelect:
                    this.txtValue.PasswordChar = (char)0;
                    this.txtValue.Visible = true;
                    this.chkValue.Visible = false;
                    this.btnBrowse.Visible = true;
                    this.panelRadio.Visible = false;
                    this.txtValue.Focus();
                    break;

                case SetEnvUIValueType.RadioButtons:
                    {
                        this.txtValue.Visible = false;
                        this.chkValue.Visible = true;
                        this.btnBrowse.Visible = false;
                        this.chkValue.Visible = false;
                        this.panelRadio.Visible = true;
                        this.radioButton1.Focus();

                        int radioCount = setEnvUIConfig[currentIndex].RadioPrompts.Length;
                        for (int i = 0; i < 5; i++)
                        {
                            if (i < radioCount)
                            {
                                radioButtons[i].Visible = true;
                                radioButtons[i].Text = setEnvUIConfig[currentIndex].RadioPrompts[i];
                                radioButtons[i].Name = setEnvUIConfig[currentIndex].RadioValues[i];
                                if (radioButtons[i].Name == setEnvUIConfig[currentIndex].PromptValue)
                                {
                                    radioButtons[i].Checked = true;
                                }
                            }
                            else
                            {
                                radioButtons[i].Visible = false;
                            }
                        }

                        break;
                    }
            }

        }

        private void btnFinish_Click(object sender, System.EventArgs e)
        {
            UpdateCurrent();

            for (int i = 0; i < setEnvUIConfig.ConfigItems.Count; i++)
            {
                Environment.SetEnvironmentVariable(setEnvUIConfig[i].EnvironmentVarName, setEnvUIConfig[i].PromptValue);
            }

            // This operation will blank out passwords, so do this after setting environment vars.
            setEnvUIConfig.SaveToFile(setEnvUIParams.configFile);

            // Guard against current directory being changed by virtue of file browse operations.
            Environment.CurrentDirectory = currentDirectory;

            if (!string.IsNullOrEmpty(setEnvUIParams.postProcessArgs))
            {
                Process.Start(setEnvUIParams.postProcess, setEnvUIParams.postProcessArgs);
            }
            else
            {
                Process.Start(setEnvUIParams.postProcess);
            }

            Application.Exit();
        }


        private string GetFullPath(string fileName)
        {
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Uri uri = new Uri(assembly.GetName().CodeBase);
            string path = Path.GetDirectoryName(uri.LocalPath);
            string fullPath = Path.Combine(path, fileName);
            return fullPath;
        }
    }
}
