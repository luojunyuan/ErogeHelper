using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Tmt.V20180321;
using TencentCloud.Tmt.V20180321.Models;

namespace ErogeHelper.Model.Factory.Translator
{
    public class TencentMtTranslator : ITranslator
    {
        public TencentMtTranslator(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;
        }

        private readonly EhConfigRepository _ehConfigRepository;

        public TranslatorName Name => TranslatorName.TencentMt;

        public string IconPath => @"/assets/site_icon/tencent.com.ico";

        public bool IsEnable { get => _ehConfigRepository.TencentMtEnable; set => _ehConfigRepository.TencentMtEnable = value; }

        public bool NeedEdit => true;

        public bool UnLock => _ehConfigRepository.TencentMtSecretId != string.Empty;

        public List<TransLanguage> SupportSrcLang => new() { TransLanguage.日本語 };

        public List<TransLanguage> SupportDesLang => new() { TransLanguage.简体中文 };

        public async Task<string> TranslateAsync(string sourceText, TransLanguage srcLang, TransLanguage desLang)
        {
            // SetCancelToken
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            var cancelToken = _cts.Token;

            // Define Support Language
            string source = srcLang switch
            {
                TransLanguage.日本語 => "ja",
                _ => throw new Exception("Language not supported"),
            };
            string target = desLang switch
            {
                TransLanguage.简体中文 => "zh",
                _ => throw new Exception("Language not supported"),
            };

            Credential cred = new()
            {
                SecretId = _ehConfigRepository.TencentMtSecretId,
                SecretKey = _ehConfigRepository.TencentMtSecretKey
            };

            ClientProfile clientProfile = new();
            HttpProfile httpProfile = new()
            {
                Endpoint = "tmt.tencentcloudapi.com"
            };
            clientProfile.HttpProfile = httpProfile;

            TmtClient client = new(cred, "ap-chengdu", clientProfile);
            TextTranslateRequest req = new()
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
            catch (Exception ex)
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

        private static CancellationTokenSource _cts = new();

        class TencentMTResponse
        {
            public string TargetText { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public string RequestId { get; set; } = string.Empty;
        }
    }
}