using ErogeHelper.Repository.Data;
using ErogeHelper.Repository.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Api
{
    static class EHServer
    {
        // TODO
        private const string hostUrl = "https://run.mocky.io/v3/e89b0f50-2993-4eb6-b0fa-8582accd118a";

        public static async Task<List<Game>> QueryGameUpdateAsync(DateTime dateTime)
        {
            var client = new RestClient(hostUrl);
            var request = new RestRequest(""); // with dateTime

            var response = await client.ExecuteGetAsync(request);
            var gameList = System.Text.Json.JsonSerializer.Deserialize<List<Game>>(response.Content);
            if (gameList is null)
            {
                Log.Error("Get error when deserialize game list");
                return new List<Game>();
            }
            else
            {
                return gameList;
            }
        }
    }
}
