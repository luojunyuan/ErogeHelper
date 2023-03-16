using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ErogeHelper.Common.Definitions;

namespace ErogeHelper.View.MainGame.Menu;

internal interface ITouchMenuPage
{
    IObservable<TouchMenuPageTag> PageChanged { get; }
    
    void Show(double distance);

    void Close();

    Visibility Visibility { set; }
}
