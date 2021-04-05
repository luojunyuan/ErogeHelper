using System.Windows.Media;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Extention
{
    public static class HinshiToColor
    {
        public static ImageSource ToColor(this Hinshi partOfSpeech)
        {
            return partOfSpeech switch
            {
                Hinshi.名詞 or Hinshi.代名詞 => StaticXamlBitmapImage.AquaGreenImage,
                Hinshi.動詞 or Hinshi.助動詞 or Hinshi.副詞 => StaticXamlBitmapImage.GreenImage,
                Hinshi.形容詞 or Hinshi.感動詞 or Hinshi.形状詞 or Hinshi.連体詞 or Hinshi.接尾辞 => StaticXamlBitmapImage.PinkImage,
                _ => StaticXamlBitmapImage.TransparentImage, // Hinshi.助詞
            };
        }
    }
}