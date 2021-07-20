using Splat;

namespace ErogeHelper.Common.Extension
{
    public static class LoggerExtension
    {
        private static bool _rxDllScanCalled;

        public static void RxUIWarningTipOnce(this IFullLogger logger, string message)
        {
            if (!_rxDllScanCalled)
            {
                _rxDllScanCalled = true;
                logger.Debug(message);
            }
        }
    }
}
