using ErogeHelper.Repository.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service
{
    class EHDbService
    {
        private readonly EHDbContext _dbContext;

        public EHDbService()
        {
            _dbContext = new();
        }

        public async Task Migrate()
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                Log.Info("Found new database stuff, start migrating...");
                await _dbContext.Database.MigrateAsync();
                Log.Info("Migrations Coomplete!");
            }
        }

        public async Task SyncGameInfo()
        {
            var newist = await _dbContext.Games.OrderByDescending(g => g.UpdateTime).FirstOrDefaultAsync();
            var gameList = await Api.EHServer.QueryGameUpdateAsync(newist?.UpdateTime ?? new());

            foreach (var game in gameList)
            {
                if (_dbContext.Games.Any(g => g.Id == game.Id))
                {
                    _dbContext.Games.Update(game);
                }
                else
                {
                    await _dbContext.Games.AddAsync(game);
                }
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
