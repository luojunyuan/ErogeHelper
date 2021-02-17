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

        bool IsEnable { get; set; }

        bool NeedKey { get; }

        bool UnLock { get; }

        List<Languages> SupportSrcLang { get; }

        List<Languages> SupportDesLang { get; }

        Task<string> TranslateAsyncImpl(string sourceText, Languages srcLang, Languages desLang);

        private static CancellationTokenSource cancelToken = new CancellationTokenSource();

        async Task<string> TranslateAsync(string sourceText)
        {
            // SetCancelToken
            cancelToken.Cancel();
            cancelToken = new CancellationTokenSource();
            var token = cancelToken.Token;

            var result = await TranslateAsyncImpl(sourceText, DataRepository.TransSrcLanguage, DataRepository.TransTargetLanguage);

            // Insert CancelAssert Before Return
            if (token.IsCancellationRequested)
            {
                Log.Debug($"{Name} Canceled");
                return string.Empty;
            }
            return result;
        }
    }
}
