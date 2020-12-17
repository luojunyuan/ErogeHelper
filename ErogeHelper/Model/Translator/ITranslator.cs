using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    interface ITranslator
    {
        TranslatorName Name { get; }
        bool IsEnable { get; set; }
        List<Language> SupportSrcLang();
        List<Language> SupportDesLang();
        Task<string> TranslatorAsync(string sourceText, Language srcLang, Language desLang);
    }
}
