using System;
using System.Net.Http;
using System.Threading.Tasks;
using ErogeHelper.Model.Entity.Response;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using Refit;

namespace ErogeHelper.Model.Service
{
    public class EhServerApiServiceService : IEhServerApiService
    {
        private readonly IEhServerApiService _ehServerApiService;

        public EhServerApiServiceService(EhConfigRepository configRepo)
        {
            // For debug using
            //var httpClient = new HttpClient(new HttpClientDiagnosticsHandler(new HttpClientHandler()))
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(configRepo.EhServerBaseUrl)
            };
            _ehServerApiService = RestService.For<IEhServerApiService>(httpClient);
        }

        public async Task<ApiResponse<GameSettingResponse>> GetGameSetting(string md5)
        {
            return await _ehServerApiService.GetGameSetting(md5).ConfigureAwait(false);
        }
    }
}