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

        public static List<CloudSaveDataEntity> CheckNull(this List<CloudSaveDataEntity>? CloudGameDataEntities) =>
            CloudGameDataEntities is not null ? CloudGameDataEntities : throw new ArgumentNullException(nameof(CloudGameDataEntities));

        public static CloudSaveDataEntity CheckNull(this CloudSaveDataEntity? cloudGameDataEntity) =>
            cloudGameDataEntity is not null ? cloudGameDataEntity : throw new ArgumentNullException(nameof(cloudGameDataEntity));
    }
}
