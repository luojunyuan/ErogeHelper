namespace Preference;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ScreenShot = new System.Windows.Forms.CheckBox();
            this.KeytwoEnter = new System.Windows.Forms.CheckBox();
            this.Register = new System.Windows.Forms.Button();
            this.Unregister = new System.Windows.Forms.Button();
            this.FullscreenMask = new System.Windows.Forms.CheckBox();
            this.MagTouch = new System.Windows.Forms.CheckBox();
            this.MagTouchInstall = new System.Windows.Forms.Button();
            this.MagTouchBox = new System.Windows.Forms.GroupBox();
            this.MagTouchBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ScreenShot
            // 
            this.ScreenShot.AutoSize = true;
            this.ScreenShot.Location = new System.Drawing.Point(70, 152);
            this.ScreenShot.Name = "ScreenShot";
            this.ScreenShot.Size = new System.Drawing.Size(430, 28);
            this.ScreenShot.TabIndex = 0;
            this.ScreenShot.Text = "Alt+ScreenPrint instead of Win+Shift+S";
            this.ScreenShot.UseVisualStyleBackColor = true;
            this.ScreenShot.CheckedChanged += new System.EventHandler(this.ScreenShot_CheckedChanged);
            // 
            // KeytwoEnter
            // 
            this.KeytwoEnter.AutoSize = true;
            this.KeytwoEnter.Location = new System.Drawing.Point(70, 254);
            this.KeytwoEnter.Name = "KeytwoEnter";
            this.KeytwoEnter.Size = new System.Drawing.Size(300, 28);
            this.KeytwoEnter.TabIndex = 1;
            this.KeytwoEnter.Text = "z key instead of Enter key";
            this.KeytwoEnter.UseVisualStyleBackColor = true;
            this.KeytwoEnter.CheckedChanged += new System.EventHandler(this.KeytwoEnter_CheckedChanged);
            // 
            // Register
            // 
            this.Register.Location = new System.Drawing.Point(70, 53);
            this.Register.Name = "Register";
            this.Register.Size = new System.Drawing.Size(154, 61);
            this.Register.TabIndex = 2;
            this.Register.Text = "Register";
            this.Register.UseVisualStyleBackColor = true;
            this.Register.Click += new System.EventHandler(this.Register_Click);
            // 
            // Unregister
            // 
            this.Unregister.Location = new System.Drawing.Point(255, 53);
            this.Unregister.Name = "Unregister";
            this.Unregister.Size = new System.Drawing.Size(173, 61);
            this.Unregister.TabIndex = 3;
            this.Unregister.Text = "Unregister";
            this.Unregister.UseVisualStyleBackColor = true;
            this.Unregister.Click += new System.EventHandler(this.Unregister_Click);
            // 
            // FullscreenMask
            // 
            this.FullscreenMask.AutoSize = true;
            this.FullscreenMask.Location = new System.Drawing.Point(70, 205);
            this.FullscreenMask.Name = "FullscreenMask";
            this.FullscreenMask.Size = new System.Drawing.Size(202, 28);
            this.FullscreenMask.TabIndex = 4;
            this.FullscreenMask.Text = "Fullscreen Mask";
            this.FullscreenMask.UseVisualStyleBackColor = true;
            this.FullscreenMask.CheckedChanged += new System.EventHandler(this.FullscreenMask_CheckedChanged);
            // 
            // MagTouch
            // 
            this.MagTouch.AutoSize = true;
            this.MagTouch.Location = new System.Drawing.Point(30, 42);
            this.MagTouch.Name = "MagTouch";
            this.MagTouch.Size = new System.Drawing.Size(214, 28);
            this.MagTouch.TabIndex = 5;
            this.MagTouch.Text = "Touch for magpie";
            this.MagTouch.UseVisualStyleBackColor = true;
            this.MagTouch.CheckedChanged += new System.EventHandler(this.MagTouch_CheckedChanged);
            // 
            // MagTouchInstall
            // 
            this.MagTouchInstall.Location = new System.Drawing.Point(30, 86);
            this.MagTouchInstall.Name = "MagTouchInstall";
            this.MagTouchInstall.Size = new System.Drawing.Size(270, 56);
            this.MagTouchInstall.TabIndex = 6;
            this.MagTouchInstall.Text = "Install MagpieTouch";
            this.MagTouchInstall.UseVisualStyleBackColor = true;
            this.MagTouchInstall.Click += new System.EventHandler(this.MagTouchInstall_Click);
            // 
            // MagTouchBox
            // 
            this.MagTouchBox.Controls.Add(this.MagTouchInstall);
            this.MagTouchBox.Controls.Add(this.MagTouch);
            this.MagTouchBox.Location = new System.Drawing.Point(70, 314);
            this.MagTouchBox.Name = "MagTouchBox";
            this.MagTouchBox.Size = new System.Drawing.Size(352, 182);
            this.MagTouchBox.TabIndex = 7;
            this.MagTouchBox.TabStop = false;
            this.MagTouchBox.Text = "Magpie Touch";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 524);
            this.Controls.Add(this.MagTouchBox);
            this.Controls.Add(this.FullscreenMask);
            this.Controls.Add(this.Unregister);
            this.Controls.Add(this.Register);
            this.Controls.Add(this.KeytwoEnter);
            this.Controls.Add(this.ScreenShot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preference";
            this.Load += new System.EventHandler(this.OnLoaded);
            this.MagTouchBox.ResumeLayout(false);
            this.MagTouchBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private CheckBox ScreenShot;
    private CheckBox KeytwoEnter;
    private Button Register;
    private Button Unregister;
    private CheckBox FullscreenMask;
    private CheckBox MagTouch;
    private Button MagTouchInstall;
    private GroupBox MagTouchBox;
}
