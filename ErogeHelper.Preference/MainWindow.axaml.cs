using System;
using System.Diagnostics;
using Avalonia.Controls;

namespace ErogeHelper.Preference;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Title = (DateTime.Now - Process.GetCurrentProcess().StartTime).Milliseconds.ToString();
    }
}