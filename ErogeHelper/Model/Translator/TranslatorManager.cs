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

    enum TranslatorName
    { 
        BaiduApi,
        Yeekit,
    }

    enum Language
    {
        Auto,
        ChineseSimplified,
        Japenese,
        English,
    }
}
