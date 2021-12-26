
namespace PokerSolitaire
{
    partial class OptionsDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.pbCardBack = new System.Windows.Forms.PictureBox();
            this.cbImage = new System.Windows.Forms.ComboBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.gbScoring = new System.Windows.Forms.GroupBox();
            this.rbEasy = new System.Windows.Forms.RadioButton();
            this.rbNormal = new System.Windows.Forms.RadioButton();
            this.rbHard = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCardBack)).BeginInit();
            this.gbScoring.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.pbCardBack);
            this.panel1.Controls.Add(this.cbImage);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(279, 112);
            this.panel1.TabIndex = 0;
            // 
            // pbCardBack
            // 
            this.pbCardBack.Location = new System.Drawing.Point(197, 6);
            this.pbCardBack.Name = "pbCardBack";
            this.pbCardBack.Size = new System.Drawing.Size(71, 96);
            this.pbCardBack.TabIndex = 1;
            this.pbCardBack.TabStop = false;
            // 
            // cbImage
            // 
            this.cbImage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbImage.FormattingEnabled = true;
            this.cbImage.Location = new System.Drawing.Point(6, 6);
            this.cbImage.Name = "cbImage";
            this.cbImage.Size = new System.Drawing.Size(185, 21);
            this.cbImage.TabIndex = 0;
            this.cbImage.SelectedIndexChanged += new System.EventHandler(this.cbImage_SelectedIndexChanged);
            // 
            // BtnOK
            // 
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnOK.Location = new System.Drawing.Point(12, 185);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 2;
            this.BtnOK.Text = "&OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(93, 185);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 3;
            this.BtnCancel.Text = "&Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // gbScoring
            // 
            this.gbScoring.Controls.Add(this.rbHard);
            this.gbScoring.Controls.Add(this.rbNormal);
            this.gbScoring.Controls.Add(this.rbEasy);
            this.gbScoring.Location = new System.Drawing.Point(12, 130);
            this.gbScoring.Name = "gbScoring";
            this.gbScoring.Size = new System.Drawing.Size(279, 49);
            this.gbScoring.TabIndex = 1;
            this.gbScoring.TabStop = false;
            this.gbScoring.Text = "Win Scoring:";
            // 
            // rbEasy
            // 
            this.rbEasy.AutoSize = true;
            this.rbEasy.Location = new System.Drawing.Point(6, 19);
            this.rbEasy.Name = "rbEasy";
            this.rbEasy.Size = new System.Drawing.Size(75, 17);
            this.rbEasy.TabIndex = 0;
            this.rbEasy.Text = "&Easy (100)";
            this.rbEasy.UseVisualStyleBackColor = true;
            this.rbEasy.CheckedChanged += new System.EventHandler(this.RbEasy_CheckedChanged);
            // 
            // rbNormal
            // 
            this.rbNormal.AutoSize = true;
            this.rbNormal.Checked = true;
            this.rbNormal.Location = new System.Drawing.Point(87, 19);
            this.rbNormal.Name = "rbNormal";
            this.rbNormal.Size = new System.Drawing.Size(85, 17);
            this.rbNormal.TabIndex = 1;
            this.rbNormal.TabStop = true;
            this.rbNormal.Text = "&Normal (150)";
            this.rbNormal.UseVisualStyleBackColor = true;
            this.rbNormal.CheckedChanged += new System.EventHandler(this.RbNormal_CheckedChanged);
            // 
            // rbHard
            // 
            this.rbHard.AutoSize = true;
            this.rbHard.Location = new System.Drawing.Point(178, 19);
            this.rbHard.Name = "rbHard";
            this.rbHard.Size = new System.Drawing.Size(75, 17);
            this.rbHard.TabIndex = 2;
            this.rbHard.Text = "&Hard (200)";
            this.rbHard.UseVisualStyleBackColor = true;
            this.rbHard.CheckedChanged += new System.EventHandler(this.RbHard_CheckedChanged);
            // 
            // OptionsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 218);
            this.Controls.Add(this.gbScoring);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "OptionsDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsDlg_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCardBack)).EndInit();
            this.gbScoring.ResumeLayout(false);
            this.gbScoring.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cbImage;
        private System.Windows.Forms.PictureBox pbCardBack;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.GroupBox gbScoring;
        private System.Windows.Forms.RadioButton rbHard;
        private System.Windows.Forms.RadioButton rbNormal;
        private System.Windows.Forms.RadioButton rbEasy;
    }
}