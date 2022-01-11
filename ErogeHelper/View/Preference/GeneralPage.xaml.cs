﻿using System.Reactive.Disposables;
using ReactiveUI;

namespace ErogeHelper.View.Pages;

public partial class GeneralPage
{
    public GeneralPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel,
                vm => vm.UseBigSizeAssistiveTouch,
                v => v.BigSizeAssistiveTouch.IsOn).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.HideTextWindow,
                v => v.HideTextWindow.IsOn).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.UseDPIDpiCompatibility,
                v => v.DpiCompatibility.IsChecked).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.UseEdgeTouchMask,
                v => v.EdgeTouchMask.IsOn).DisposeWith(d);
        });
    }
}