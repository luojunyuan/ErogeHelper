using System;
using Microsoft.EntityFrameworkCore;

namespace ErogeHelper.Repository.Models
{
    public class Subtitle
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public string Language { get; set; } = "en";

        public long Hash { get; set; }

        /// <summary>
        /// Suggest size by vnr
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 元のText
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Translated text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        public int UpVote { get; set; }

        public int DownVote { get; set; }

        public string CreatorName { get; set; } = string.Empty;

        public string CreatorHomePage { get; set; } = string.Empty;

        public string CreationSubtitle { get; set; } = string.Empty;

        public DateTime CreationTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        public string RevisionSubtitle { get; set; } = string.Empty;

        public DateTime RevisionTime { get; set; }

        public bool Deleted { get; set; }
    }
}