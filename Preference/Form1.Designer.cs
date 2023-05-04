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
            this.LEPathTextbox = new System.Windows.Forms.TextBox();
            this.LEPathDiaboxButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ScreenShot
            // 
            this.ScreenShot.AutoSize = true;
            this.ScreenShot.Location = new System.Drawing.Point(64, 212);
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
            this.KeytwoEnter.Location = new System.Drawing.Point(64, 314);
            this.KeytwoEnter.Name = "KeytwoEnter";
            this.KeytwoEnter.Size = new System.Drawing.Size(300, 28);
            this.KeytwoEnter.TabIndex = 1;
            this.KeytwoEnter.Text = "z key instead of Enter key";
            this.KeytwoEnter.UseVisualStyleBackColor = true;
            this.KeytwoEnter.CheckedChanged += new System.EventHandler(this.KeytwoEnter_CheckedChanged);
            // 
            // Register
            // 
            this.Register.Location = new System.Drawing.Point(64, 34);
            this.Register.Name = "Register";
            this.Register.Size = new System.Drawing.Size(154, 61);
            this.Register.TabIndex = 2;
            this.Register.Text = "Register";
            this.Register.UseVisualStyleBackColor = true;
            this.Register.Click += new System.EventHandler(this.Register_Click);
            // 
            // Unregister
            // 
            this.Unregister.Location = new System.Drawing.Point(249, 34);
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
            this.FullscreenMask.Location = new System.Drawing.Point(64, 265);
            this.FullscreenMask.Name = "FullscreenMask";
            this.FullscreenMask.Size = new System.Drawing.Size(202, 28);
            this.FullscreenMask.TabIndex = 4;
            this.FullscreenMask.Text = "Fullscreen Mask";
            this.FullscreenMask.UseVisualStyleBackColor = true;
            this.FullscreenMask.CheckedChanged += new System.EventHandler(this.FullscreenMask_CheckedChanged);
            // 
            // LEPathTextbox
            // 
            this.LEPathTextbox.Location = new System.Drawing.Point(64, 155);
            this.LEPathTextbox.Name = "LEPathTextbox";
            this.LEPathTextbox.Size = new System.Drawing.Size(263, 31);
            this.LEPathTextbox.TabIndex = 8;
            // 
            // LEPathDiaboxButton
            // 
            this.LEPathDiaboxButton.Location = new System.Drawing.Point(353, 134);
            this.LEPathDiaboxButton.Name = "LEPathDiaboxButton";
            this.LEPathDiaboxButton.Size = new System.Drawing.Size(130, 52);
            this.LEPathDiaboxButton.TabIndex = 9;
            this.LEPathDiaboxButton.Text = "Select";
            this.LEPathDiaboxButton.UseVisualStyleBackColor = true;
            this.LEPathDiaboxButton.Click += new System.EventHandler(this.LEPathDialogButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(60, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(226, 24);
            this.label1.TabIndex = 10;
            this.label1.Text = "Locate Emulator Path";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(797, 585);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LEPathDiaboxButton);
            this.Controls.Add(this.LEPathTextbox);
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
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private CheckBox ScreenShot;
    private CheckBox KeytwoEnter;
    private Button Register;
    private Button Unregister;
    private CheckBox FullscreenMask;
    private TextBox LEPathTextbox;
    private Button LEPathDiaboxButton;
    private Label label1;
}
