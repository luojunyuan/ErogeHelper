using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Translator
{
    public interface ITranslator
    {
        string Name { get; }

        string IconPath { get; }

        bool IsEnable { get; set; }

        bool NeedKey { get; }

        /// <summary>
        /// <para>This means translator is weather CanEnable in TransViewModel.cs</para>
        /// <para>Always true if `NeedKey == false`</para>
        /// </summary>
        bool UnLock { get; }

        List<Languages> SupportSrcLang { get; }

        List<Languages> SupportDesLang { get; }

        Task<string> TranslateAsync(string sourceText, Languages srcLang, Languages desLang);
    }
}
