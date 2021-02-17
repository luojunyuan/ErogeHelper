using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    class TranslatorManager
    {
        public static List<ITranslator> GetAll { get; set; } = new List<ITranslator>()
        {
            new BaiduApiTranslator(),
            new YeekitTranslator(),
        };

        public static List<ITranslator> GetEnabled()
        {
            var collect = new List<ITranslator>();

            foreach (var translator in GetAll)
            {
                if (translator.IsEnable)
                {
                    collect.Add(translator);
                }
            }

            return collect; 
        }
    }

    public enum TranslatorName
    { 
        BaiduApi,
        Yeekit,
    }

    public enum Languages
    {
        Auto,
        简体中文,
        日本語,
        English,
    }

}
