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
        TranslatorName Name { get; }

        bool IsEnable { get; set; }

        List<Language> SupportSrcLang();

        List<Language> SupportDesLang();

        private static CancellationTokenSource cancelToken = new CancellationTokenSource();

        async Task<string> TranslateAsync(string sourceText, Language srcLang, Language desLang)
        {
            // SetCancelToken
            cancelToken.Cancel();
            cancelToken = new CancellationTokenSource();

            var result = await TranslateAsyncImpl(sourceText, srcLang, desLang);

            // Insert CancelAssert Before Return
            if (cancelToken.Token.IsCancellationRequested)
            {
                return string.Empty;
            }
            return result;
        }

        Task<string> TranslateAsyncImpl(string sourceText, Language srcLang, Language desLang);
    }
}
