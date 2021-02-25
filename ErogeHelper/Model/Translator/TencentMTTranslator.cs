using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Tmt.V20180321;
using TencentCloud.Tmt.V20180321.Models;

namespace ErogeHelper.Model.Translator
{
    class TencentMTTranslator : ITranslator
    {
        public string Name => "TencentMT";

        public string IconPath => @"/Assets/transparent.png";

        public bool IsEnable { get => DataRepository.TencentMTEnable; set => DataRepository.TencentMTEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => !DataRepository.TencentMTSecretId.Equals(string.Empty);

        public List<Languages> SupportSrcLang => new List<Languages> { Languages.日本語 };

        public List<Languages> SupportDesLang => new List<Languages> { Languages.简体中文 };

        public async Task<string> TranslateAsync(string sourceText, Languages srcLang, Languages desLang)
        {
            // SetCancelToken
            cts.Cancel();
            cts = new CancellationTokenSource();
            var cancelToken = cts.Token;

            // Define Support Language
            string source = srcLang switch
            {
                Languages.日本語 => "ja",
                _ => throw new Exception("Language not supported"),
            };
            string target = desLang switch
            {
                Languages.简体中文 => "zh",
                _ => throw new Exception("Language not supported"),
            };

            Credential cred = new Credential
            {
                SecretId = DataRepository.TencentMTSecretId,
                SecretKey = DataRepository.TencentMTSecretKey
            };

            ClientProfile clientProfile = new ClientProfile();
            HttpProfile httpProfile = new HttpProfile
            {
                Endpoint = "tmt.tencentcloudapi.com"
            };
            clientProfile.HttpProfile = httpProfile;

            TmtClient client = new TmtClient(cred, "ap-chengdu", clientProfile);
            TextTranslateRequest req = new TextTranslateRequest
            {
                SourceText = sourceText,
                Source = source,
                Target = target,
                ProjectId = 0
            };

            string result;
            try
            {
                TextTranslateResponse response = await client.TextTranslate(req);
                var resp = System.Text.Json.JsonSerializer.Deserialize<TencentMTResponse>(
                                                                                AbstractModel.ToJsonString(response))!;
                result = resp.TargetText;
            }
            catch(Exception ex)
            {
                result = ex.Message;
                Log.Warn(ex);
            }

            // Insert CancelAssert Before Return
            if (cancelToken.IsCancellationRequested)
            {
                Log.Debug($"{Name} Canceled");
                return string.Empty;
            }

            return result;
        }

        private static CancellationTokenSource cts = new();

        class TencentMTResponse
        {
            public string TargetText { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public string RequestId { get; set; } = string.Empty;
        }
    }
}
