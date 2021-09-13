using ErogeHelper.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;

namespace ErogeHelper.Common.Extensions
{
    public static class ArgumentNullCheckExtension
    {
        public static string CheckFileExist(this string filePath) =>
            File.Exists(filePath) ? filePath : throw new ArgumentNullException(nameof(filePath));
    }
}
