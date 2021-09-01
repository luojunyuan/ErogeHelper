using System;

namespace ErogeHelper.Common.Entities
{
    public class CloudGameDataEntity
    {
        public CloudGameDataEntity(string md5, DateTime lastTimeModified, string pcName, string uniqueId)
        {
            Md5 = md5;
            LastTimeModified = lastTimeModified;
            PCName = pcName;
            UniqueId = uniqueId;
        }

        public string Md5 { get; }

        // public string FolderName { get; }

        // public string LocalPath { get; }

        public DateTime LastTimeModified { get; }

        public string PCName { get; }

        public string UniqueId { get; }

        // public List<string> splitFiles { get; }
    }
}
