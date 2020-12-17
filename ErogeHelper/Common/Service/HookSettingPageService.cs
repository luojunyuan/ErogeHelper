using ErogeHelper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Service
{
    class HookSettingPageService : IHookSettingPageService
    {
        public string GetRegexp() => File.Exists(GameConfig.Path) ? GameConfig.Regexp : string.Empty;
    }
}
