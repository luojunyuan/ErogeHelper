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
        components = new System.ComponentModel.Container();
        ScreenShot = new CheckBox();
        KeytwoEnter = new CheckBox();
        Register = new Button();
        Unregister = new Button();
        FullscreenMask = new CheckBox();
        LEPathTextbox = new TextBox();
        LEPathDiaboxButton = new Button();
        label1 = new Label();
        ProcessComboBox = new ComboBox();
        StartProcess = new Button();
        groupBox1 = new GroupBox();
        DeleteConfigButton = new Button();
        form1BindingSource = new BindingSource(components);
        form1BindingSource1 = new BindingSource(components);
        groupBox1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)form1BindingSource).BeginInit();
        ((System.ComponentModel.ISupportInitialize)form1BindingSource1).BeginInit();
        SuspendLayout();
        // 
        // ScreenShot
        // 
        ScreenShot.AutoSize = true;
        ScreenShot.Location = new Point(38, 289);
        ScreenShot.Margin = new Padding(3, 4, 3, 4);
        ScreenShot.Name = "ScreenShot";
        ScreenShot.Size = new Size(468, 36);
        ScreenShot.TabIndex = 0;
        ScreenShot.Text = "Alt+ScreenPrint instead of Win+Shift+S";
        ScreenShot.UseVisualStyleBackColor = true;
        ScreenShot.CheckedChanged += ScreenShot_CheckedChanged;
        // 
        // KeytwoEnter
        // 
        KeytwoEnter.AutoSize = true;
        KeytwoEnter.Location = new Point(38, 425);
        KeytwoEnter.Margin = new Padding(3, 4, 3, 4);
        KeytwoEnter.Name = "KeytwoEnter";
        KeytwoEnter.Size = new Size(320, 36);
        KeytwoEnter.TabIndex = 1;
        KeytwoEnter.Text = "z key instead of Enter key";
        KeytwoEnter.UseVisualStyleBackColor = true;
        KeytwoEnter.CheckedChanged += KeytwoEnter_CheckedChanged;
        // 
        // Register
        // 
        Register.Location = new Point(64, 45);
        Register.Margin = new Padding(3, 4, 3, 4);
        Register.Name = "Register";
        Register.Size = new Size(160, 85);
        Register.TabIndex = 2;
        Register.Text = "Register";
        Register.UseVisualStyleBackColor = true;
        Register.Click += Register_Click;
        // 
        // Unregister
        // 
        Unregister.Location = new Point(249, 45);
        Unregister.Margin = new Padding(3, 4, 3, 4);
        Unregister.Name = "Unregister";
        Unregister.Size = new Size(160, 85);
        Unregister.TabIndex = 3;
        Unregister.Text = "Unregister";
        Unregister.UseVisualStyleBackColor = true;
        Unregister.Click += Unregister_Click;
        // 
        // FullscreenMask
        // 
        FullscreenMask.AutoSize = true;
        FullscreenMask.Location = new Point(38, 360);
        FullscreenMask.Margin = new Padding(3, 4, 3, 4);
        FullscreenMask.Name = "FullscreenMask";
        FullscreenMask.Size = new Size(215, 36);
        FullscreenMask.TabIndex = 4;
        FullscreenMask.Text = "Fullscreen Mask";
        FullscreenMask.UseVisualStyleBackColor = true;
        FullscreenMask.CheckedChanged += FullscreenMask_CheckedChanged;
        // 
        // LEPathTextbox
        // 
        LEPathTextbox.Location = new Point(38, 213);
        LEPathTextbox.Margin = new Padding(3, 4, 3, 4);
        LEPathTextbox.Name = "LEPathTextbox";
        LEPathTextbox.ReadOnly = true;
        LEPathTextbox.Size = new Size(263, 39);
        LEPathTextbox.TabIndex = 8;
        // 
        // LEPathDiaboxButton
        // 
        LEPathDiaboxButton.Location = new Point(327, 185);
        LEPathDiaboxButton.Margin = new Padding(3, 4, 3, 4);
        LEPathDiaboxButton.Name = "LEPathDiaboxButton";
        LEPathDiaboxButton.Size = new Size(128, 85);
        LEPathDiaboxButton.TabIndex = 9;
        LEPathDiaboxButton.Text = "Select";
        LEPathDiaboxButton.UseVisualStyleBackColor = true;
        LEPathDiaboxButton.Click += LEPathDialogButton_Click;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(34, 160);
        label1.Name = "label1";
        label1.Size = new Size(239, 32);
        label1.TabIndex = 10;
        label1.Text = "Locate Emulator Path";
        // 
        // ProcessComboBox
        // 
        ProcessComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        ProcessComboBox.FormattingEnabled = true;
        ProcessComboBox.Location = new Point(38, 73);
        ProcessComboBox.Margin = new Padding(3, 4, 3, 4);
        ProcessComboBox.Name = "ProcessComboBox";
        ProcessComboBox.Size = new Size(263, 40);
        ProcessComboBox.TabIndex = 11;
        ProcessComboBox.DropDown += ProcessComboBox_DropDown;
        ProcessComboBox.SelectedIndexChanged += ProcessComboBox_SelectedIndexChanged;
        // 
        // StartProcess
        // 
        StartProcess.Location = new Point(327, 60);
        StartProcess.Margin = new Padding(3, 4, 3, 4);
        StartProcess.Name = "StartProcess";
        StartProcess.Size = new Size(128, 85);
        StartProcess.TabIndex = 12;
        StartProcess.Text = "Start";
        StartProcess.UseVisualStyleBackColor = true;
        StartProcess.Click += StartProcess_Click;
        // 
        // groupBox1
        // 
        groupBox1.Controls.Add(DeleteConfigButton);
        groupBox1.Controls.Add(ProcessComboBox);
        groupBox1.Controls.Add(StartProcess);
        groupBox1.Controls.Add(ScreenShot);
        groupBox1.Controls.Add(KeytwoEnter);
        groupBox1.Controls.Add(label1);
        groupBox1.Controls.Add(FullscreenMask);
        groupBox1.Controls.Add(LEPathDiaboxButton);
        groupBox1.Controls.Add(LEPathTextbox);
        groupBox1.Location = new Point(64, 165);
        groupBox1.Margin = new Padding(3, 4, 3, 4);
        groupBox1.Name = "groupBox1";
        groupBox1.Padding = new Padding(3, 4, 3, 4);
        groupBox1.Size = new Size(518, 599);
        groupBox1.TabIndex = 13;
        groupBox1.TabStop = false;
        groupBox1.Text = "Advanced";
        // 
        // DeleteConfigButton
        // 
        DeleteConfigButton.Location = new Point(38, 492);
        DeleteConfigButton.Margin = new Padding(3, 4, 3, 4);
        DeleteConfigButton.Name = "DeleteConfigButton";
        DeleteConfigButton.Size = new Size(160, 85);
        DeleteConfigButton.TabIndex = 13;
        DeleteConfigButton.Text = "Clear Config";
        DeleteConfigButton.UseVisualStyleBackColor = true;
        DeleteConfigButton.Click += DeleteConfigButton_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(797, 780);
        Controls.Add(groupBox1);
        Controls.Add(Unregister);
        Controls.Add(Register);
        Margin = new Padding(3, 4, 3, 4);
        Name = "Form1";
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Preference - V1.0.1.0";
        Load += OnLoaded;
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)form1BindingSource).EndInit();
        ((System.ComponentModel.ISupportInitialize)form1BindingSource1).EndInit();
        ResumeLayout(false);
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
    private ComboBox ProcessComboBox;
    private Button StartProcess;
    private GroupBox groupBox1;
    private BindingSource form1BindingSource1;
    private BindingSource form1BindingSource;
    private Button DeleteConfigButton;
}
