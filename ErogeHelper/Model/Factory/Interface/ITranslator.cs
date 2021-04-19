using System.Collections.Generic;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Model.Factory.Interface
{
    public interface ITranslator
    {
        TranslatorName Name { get; }

        string IconPath { get; }

        /// <summary>
        /// Determined by user
        /// </summary>
        bool IsEnable { get; set; }

        bool NeedEdit { get; }

        /// <summary>
        /// <para>This means translator is weather CanEnable in TransViewModel.cs</para>
        /// <para>Please make it always true if `NeedEdit == false`</para>
        /// </summary>
        bool UnLock { get; }

        List<TransLanguage> SupportSrcLang { get; }

        List<TransLanguage> SupportDesLang { get; }

        Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang);
    }
}