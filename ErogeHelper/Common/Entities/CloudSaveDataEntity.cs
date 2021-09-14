using System;
using System.Collections.Generic;
using System.IO;

namespace ErogeHelper.Common.Entities
{
    public record CloudSaveDataEntity
    {
        public CloudSaveDataEntity(
            string md5,
            string localPath,
            DateTime lastTimeModified,
            string[] excludeFiles,
            string pcName,
            string pcId)
        {
            Md5 = md5;
            FolderName = Path.GetFileNameWithoutExtension(localPath) ?? string.Empty;
            LocalPath = localPath;
            LastTimeModified = lastTimeModified;
            ExcludeFiles = excludeFiles;
            PCName = pcName;
            PCId = pcId;
        }

        public string Md5 { get; init; }

        public string FolderName { get; init; }

        public string LocalPath { get; init; }

        public DateTime LastTimeModified { get; init; }

        public string PCName { get; init; }

        public string PCId { get; init; }

        public string[] ExcludeFiles { get; init; }
    }
}
