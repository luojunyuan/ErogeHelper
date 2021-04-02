using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErogeHelper.View.Control
{
    public class ScrollViewerPointer : ScrollViewer
    {
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            foreach (System.Windows.Window currentWindow in Application.Current.Windows)
            {
                if (currentWindow.Title.Equals(ErogeHelper.Language.Strings.HookConfig_Title))
                {
                    Log.Debug("HookConfig window Activate");
                    currentWindow.Activate();
                }
            }

        }
    }
}