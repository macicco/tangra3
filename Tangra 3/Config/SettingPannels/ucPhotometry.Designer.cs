﻿namespace Tangra.Config.SettingPannels
{
	partial class ucPhotometry
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pnlSeeingSettings = new System.Windows.Forms.Panel();
			this.label13 = new System.Windows.Forms.Label();
			this.pnlUserSeeing = new System.Windows.Forms.Panel();
			this.nudUserSpecifiedFWHM = new System.Windows.Forms.NumericUpDown();
			this.label14 = new System.Windows.Forms.Label();
			this.rgSeeing = new System.Windows.Forms.GroupBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.groupControl1 = new System.Windows.Forms.GroupBox();
			this.nudPhotoAperture = new System.Windows.Forms.NumericUpDown();
			this.nudInnerAnulusInApertures = new System.Windows.Forms.NumericUpDown();
			this.nudMinimumAnulusPixels = new System.Windows.Forms.NumericUpDown();
			this.cbxPhotoSignalApertureType = new System.Windows.Forms.ComboBox();
			this.cbxBackgroundMethod = new System.Windows.Forms.ComboBox();
			this.groupControl2 = new System.Windows.Forms.GroupBox();
			this.cbxPsfQuadrature = new System.Windows.Forms.ComboBox();
			this.cbxPsfFittingMethod = new System.Windows.Forms.ComboBox();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.pnlSeeingSettings.SuspendLayout();
			this.pnlUserSeeing.SuspendLayout();
			this.rgSeeing.SuspendLayout();
			this.groupControl1.SuspendLayout();
			this.groupControl2.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlSeeingSettings
			// 
			this.pnlSeeingSettings.Controls.Add(this.label13);
			this.pnlSeeingSettings.Controls.Add(this.pnlUserSeeing);
			this.pnlSeeingSettings.Controls.Add(this.rgSeeing);
			this.pnlSeeingSettings.Location = new System.Drawing.Point(8, 127);
			this.pnlSeeingSettings.Name = "pnlSeeingSettings";
			this.pnlSeeingSettings.Size = new System.Drawing.Size(203, 69);
			this.pnlSeeingSettings.TabIndex = 37;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(5, 3);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(43, 13);
			this.label13.TabIndex = 33;
			this.label13.Text = "Seeing:";
			// 
			// pnlUserSeeing
			// 
			this.pnlUserSeeing.Controls.Add(this.nudUserSpecifiedFWHM);
			this.pnlUserSeeing.Controls.Add(this.label14);
			this.pnlUserSeeing.Enabled = false;
			this.pnlUserSeeing.Location = new System.Drawing.Point(121, 41);
			this.pnlUserSeeing.Name = "pnlUserSeeing";
			this.pnlUserSeeing.Size = new System.Drawing.Size(75, 21);
			this.pnlUserSeeing.TabIndex = 36;
			// 
			// nudUserSpecifiedFWHM
			// 
			this.nudUserSpecifiedFWHM.Location = new System.Drawing.Point(1, 1);
			this.nudUserSpecifiedFWHM.Name = "nudUserSpecifiedFWHM";
			this.nudUserSpecifiedFWHM.Size = new System.Drawing.Size(47, 20);
			this.nudUserSpecifiedFWHM.TabIndex = 37;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(53, 5);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(18, 13);
			this.label14.TabIndex = 30;
			this.label14.Text = "px";
			// 
			// rgSeeing
			// 
			this.rgSeeing.Controls.Add(this.radioButton1);
			this.rgSeeing.Location = new System.Drawing.Point(10, 14);
			this.rgSeeing.Name = "rgSeeing";
			this.rgSeeing.Size = new System.Drawing.Size(149, 54);
			this.rgSeeing.TabIndex = 38;
			this.rgSeeing.TabStop = false;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(13, 31);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(86, 13);
			this.label6.TabIndex = 31;
			this.label6.Text = "PSF Quadrature:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 79);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(77, 13);
			this.label3.TabIndex = 29;
			this.label3.Text = "Fitting Method:";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(19, 235);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(144, 13);
			this.label9.TabIndex = 30;
			this.label9.Text = "Pixels in Background Anulus:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(110, 183);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(81, 13);
			this.label8.TabIndex = 29;
			this.label8.Text = "signal apertures";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(22, 158);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(178, 13);
			this.label7.TabIndex = 28;
			this.label7.Text = "Inner Radius of Background Anulus:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(22, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(144, 13);
			this.label1.TabIndex = 25;
			this.label1.Text = "Default Background Method:";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(19, 40);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(119, 13);
			this.label17.TabIndex = 12;
			this.label17.Text = "Default Signal Aperture:";
			// 
			// groupControl1
			// 
			this.groupControl1.Controls.Add(this.nudPhotoAperture);
			this.groupControl1.Controls.Add(this.nudInnerAnulusInApertures);
			this.groupControl1.Controls.Add(this.nudMinimumAnulusPixels);
			this.groupControl1.Controls.Add(this.cbxPhotoSignalApertureType);
			this.groupControl1.Controls.Add(this.cbxBackgroundMethod);
			this.groupControl1.Controls.Add(this.label9);
			this.groupControl1.Controls.Add(this.label17);
			this.groupControl1.Controls.Add(this.label8);
			this.groupControl1.Controls.Add(this.label7);
			this.groupControl1.Controls.Add(this.label1);
			this.groupControl1.Location = new System.Drawing.Point(240, 6);
			this.groupControl1.Name = "groupControl1";
			this.groupControl1.Size = new System.Drawing.Size(230, 285);
			this.groupControl1.TabIndex = 35;
			this.groupControl1.TabStop = false;
			this.groupControl1.Text = "Aperture Photometry";
			// 
			// nudPhotoAperture
			// 
			this.nudPhotoAperture.Location = new System.Drawing.Point(54, 59);
			this.nudPhotoAperture.Name = "nudPhotoAperture";
			this.nudPhotoAperture.Size = new System.Drawing.Size(47, 20);
			this.nudPhotoAperture.TabIndex = 36;
			// 
			// nudInnerAnulusInApertures
			// 
			this.nudInnerAnulusInApertures.Location = new System.Drawing.Point(54, 180);
			this.nudInnerAnulusInApertures.Name = "nudInnerAnulusInApertures";
			this.nudInnerAnulusInApertures.Size = new System.Drawing.Size(47, 20);
			this.nudInnerAnulusInApertures.TabIndex = 35;
			// 
			// nudMinimumAnulusPixels
			// 
			this.nudMinimumAnulusPixels.Location = new System.Drawing.Point(169, 232);
			this.nudMinimumAnulusPixels.Name = "nudMinimumAnulusPixels";
			this.nudMinimumAnulusPixels.Size = new System.Drawing.Size(47, 20);
			this.nudMinimumAnulusPixels.TabIndex = 34;
			// 
			// cbxPhotoSignalApertureType
			// 
			this.cbxPhotoSignalApertureType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxPhotoSignalApertureType.Items.AddRange(new object[] {
            "FHWM",
            "Pixels"});
			this.cbxPhotoSignalApertureType.Location = new System.Drawing.Point(107, 59);
			this.cbxPhotoSignalApertureType.Name = "cbxPhotoSignalApertureType";
			this.cbxPhotoSignalApertureType.Size = new System.Drawing.Size(76, 21);
			this.cbxPhotoSignalApertureType.TabIndex = 33;
			// 
			// cbxBackgroundMethod
			// 
			this.cbxBackgroundMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxBackgroundMethod.Items.AddRange(new object[] {
            "Average Background",
            "Background Mode",
            "Background Gradient Fit",
            "PSF-Fitting Background",
            "Median Background"});
			this.cbxBackgroundMethod.Location = new System.Drawing.Point(25, 112);
			this.cbxBackgroundMethod.Name = "cbxBackgroundMethod";
			this.cbxBackgroundMethod.Size = new System.Drawing.Size(191, 21);
			this.cbxBackgroundMethod.TabIndex = 32;
			// 
			// groupControl2
			// 
			this.groupControl2.Controls.Add(this.cbxPsfQuadrature);
			this.groupControl2.Controls.Add(this.cbxPsfFittingMethod);
			this.groupControl2.Controls.Add(this.pnlSeeingSettings);
			this.groupControl2.Controls.Add(this.label6);
			this.groupControl2.Controls.Add(this.label3);
			this.groupControl2.Location = new System.Drawing.Point(3, 6);
			this.groupControl2.Name = "groupControl2";
			this.groupControl2.Size = new System.Drawing.Size(233, 285);
			this.groupControl2.TabIndex = 36;
			this.groupControl2.TabStop = false;
			this.groupControl2.Text = "PSF-Fitting Photometry";
			// 
			// cbxPsfQuadrature
			// 
			this.cbxPsfQuadrature.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxPsfQuadrature.Items.AddRange(new object[] {
            "Numerical Quadrature in Aperture",
            "Full Analytical Quadrature"});
			this.cbxPsfQuadrature.Location = new System.Drawing.Point(16, 47);
			this.cbxPsfQuadrature.Name = "cbxPsfQuadrature";
			this.cbxPsfQuadrature.Size = new System.Drawing.Size(185, 21);
			this.cbxPsfQuadrature.TabIndex = 39;
			// 
			// cbxPsfFittingMethod
			// 
			this.cbxPsfFittingMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxPsfFittingMethod.Items.AddRange(new object[] {
            "Direct Non-Linear Fit",
            "Linear Fit of Averaged Model"});
			this.cbxPsfFittingMethod.Location = new System.Drawing.Point(16, 95);
			this.cbxPsfFittingMethod.Name = "cbxPsfFittingMethod";
			this.cbxPsfFittingMethod.Size = new System.Drawing.Size(185, 21);
			this.cbxPsfFittingMethod.TabIndex = 38;
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Location = new System.Drawing.Point(7, 12);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(85, 17);
			this.radioButton1.TabIndex = 0;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "radioButton1";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// ucPhotometry
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.groupControl2);
			this.Controls.Add(this.groupControl1);
			this.Name = "ucPhotometry";
			this.Size = new System.Drawing.Size(1052, 349);
			this.pnlSeeingSettings.ResumeLayout(false);
			this.pnlSeeingSettings.PerformLayout();
			this.pnlUserSeeing.ResumeLayout(false);
			this.pnlUserSeeing.PerformLayout();
			this.rgSeeing.ResumeLayout(false);
			this.rgSeeing.PerformLayout();
			this.groupControl1.ResumeLayout(false);
			this.groupControl1.PerformLayout();
			this.groupControl2.ResumeLayout(false);
			this.groupControl2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pnlSeeingSettings;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Panel pnlUserSeeing;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.GroupBox groupControl1;
		private System.Windows.Forms.GroupBox groupControl2;
		private System.Windows.Forms.ComboBox cbxBackgroundMethod;
		private System.Windows.Forms.ComboBox cbxPhotoSignalApertureType;
		private System.Windows.Forms.ComboBox cbxPsfFittingMethod;
		private System.Windows.Forms.NumericUpDown nudMinimumAnulusPixels;
		private System.Windows.Forms.NumericUpDown nudInnerAnulusInApertures;
		private System.Windows.Forms.NumericUpDown nudPhotoAperture;
		private System.Windows.Forms.NumericUpDown nudUserSpecifiedFWHM;
		private System.Windows.Forms.GroupBox rgSeeing;
		private System.Windows.Forms.ComboBox cbxPsfQuadrature;
		private System.Windows.Forms.RadioButton radioButton1;
	}
}
