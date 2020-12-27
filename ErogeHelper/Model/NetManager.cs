using System.Collections.Generic;

namespace ErogeHelper.Model
{
    class NetManager
    {
        public static Dictionary<int, string> GetGameFiles()
        {
            // var response = await client.PostAsync<>(V3_API + 'game/files').ConfigureAwait(false);
            // check response
            // foreach (var game in response)
            // {
            //     // new a GameBean
            //     // id md5 name(filename) itemId visitCount CommentCount
            //     // add to a dict<id: int, GameBean>
            // }
            // Console.WriteLine(dict.lenth);
            // catch(ConnectionError)
            // catch(HttpError)
            // catch(Exception)

            var ret = new Dictionary<int, string>();
            ret.Add(42119, "");

            return ret;
        }
    }
}