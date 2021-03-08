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
        private const string hostUrl = "https://run.mocky.io/v3/e89b0f50-2993-4eb6-b0fa-8582accd118a";

        public static void SyncGame(DateTime dateTime)
        {
            var client = new RestClient(hostUrl);
            var request = new RestRequest(""); // with dateTime

            var response = client.Get(request);
            var gameList = System.Text.Json.JsonSerializer.Deserialize<List<Game>>(response.Content);
            if (gameList is null)
            {
                Log.Error("Get error when deserialize game list");
                return;
            }

            using var db = new EHDbContext();

            foreach(var game in gameList)
            {
                if (db.Games.Any(g => g.Id == game.Id))
                {
                    db.Games.Update(game);
                }
                else
                {
                    db.Games.Add(game);
                }
            }
            db.SaveChanges();
        }
    }
}
