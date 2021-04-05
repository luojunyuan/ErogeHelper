using System.Windows;
using System.Windows.Media;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class SingleTextItem
    {
        public SingleTextItem(string ruby, string text, TextTemplateType templateType, ImageSource backgroundColor)
        {
            Ruby = ruby;
            Text = text;
            TextTemplateType = templateType;
            SubMarkColor = backgroundColor;
            
            // UNDONE
            //Application.Current.Dispatcher.InvokeAsync(
            //    () => CanBeSearch = !backgroundColor.ToString().Equals(StaticXamlBitmapImage.TransparentImage.ToString()));
            CanBeSearch = backgroundColor != StaticXamlBitmapImage.TransparentImage;
        }

        // UNDONE: 增加啥，增加 动词的lemma
        public string Ruby { get; }
        public string Text { get; }
        public TextTemplateType TextTemplateType { get; }
        public ImageSource SubMarkColor { get; }
        public bool CanBeSearch { get; }
    }
}
