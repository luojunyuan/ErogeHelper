using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caliburn.Micro;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class HookDataService : IHookDataService
    {
        public HookDataService(EhDbRepository ehDbRepository)
        {
            _ehDbRepository = ehDbRepository;
        }

        private readonly EhDbRepository _ehDbRepository;

        private const string GameQuery = "http://vnr.aniclan.com/connection.php?go=game_query";

        public async Task<string> QueryHCode()
        {
            string param = $"md5={_ehDbRepository.Md5}";
            byte[] bs = Encoding.ASCII.GetBytes(param);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GameQuery);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;
            await using (Stream reqStream = req.GetRequestStream())
            {
                await reqStream.WriteAsync(bs, 0, bs.Length);
            }
            using WebResponse wr = await req.GetResponseAsync();
            using StreamReader sr = new(wr.GetResponseStream());
            string xmlString = await sr.ReadToEndAsync();

            var xDoc = XDocument.Parse(xmlString);
            var game = xDoc.Element("grimoire")?.Element("games")?.Element("game");
            if (game?.Element("hook") is not null)
            {
                return game?.Element("hook")?.Value ?? string.Empty;
            }

            return string.Empty;
        }

        public string GetRegExp() => _ehDbRepository.GetGameInfo()?.RegExp ?? string.Empty;
    }
}
