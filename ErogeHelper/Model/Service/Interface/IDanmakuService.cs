using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IDanmakuService
    {
        // to danma ku struct
        List<string> QueryDanmaku(string sourceText);
    }
}
