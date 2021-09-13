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

        public string Md5 { get; }

        public string FolderName { get; }

        public string LocalPath { get; }

        public DateTime LastTimeModified { get; set; }

        public string PCName { get; }

        public string PCId { get; }

        public string[] ExcludeFiles { get; }
    }
}
