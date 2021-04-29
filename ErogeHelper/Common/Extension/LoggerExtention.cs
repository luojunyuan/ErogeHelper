using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Extension
{
    public static class LoggerExtention
    {
        private static bool _rxDllScanCalled = false;
        public static void DebugCallOnce(this IFullLogger logger, string message)
        {
            if (_rxDllScanCalled)
            { 
                return ;
            }
            else
            {
                _rxDllScanCalled = true;
                logger.Debug(message);
            }
        }
    }
}
