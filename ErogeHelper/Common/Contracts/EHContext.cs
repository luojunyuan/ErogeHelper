using System.Reflection;

namespace ErogeHelper.Common.Contracts
{
    internal static class EHContext
    {
        public static readonly string? AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }
}
