using System;

namespace ErogeHelper.Common.Extensions
{
    public static class PathCheckExtension
    {
        public static bool IsUncPath(this string path) =>
            Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsUnc;
    }
}
