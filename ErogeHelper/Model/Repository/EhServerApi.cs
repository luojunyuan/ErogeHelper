using Refit;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Net.Http;
using System.Threading.Tasks;
using ErogeHelper.Model.Repository.Interface;
using System;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Repository.Entity;

namespace ErogeHelper.Model.Repository
{
    public class EhServerApi : IEhServerApi
    {
        private readonly IEhServerApi _ehServerApi;

        public EhServerApi(EhConfigRepository configRepo)
        {
            // For debug using
            //var httpClient = new HttpClient(new HttpClientDiagnosticsHandler(new HttpClientHandler()))
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(configRepo.EhServerBaseUrl)
            };
            _ehServerApi = RestService.For<IEhServerApi>(httpClient);
        }

        public async Task<ApiResponse<GameSetting>> GetGameSetting(string md5)
        {
            return await _ehServerApi.GetGameSetting(md5).ConfigureAwait(false);
        }
    }
}