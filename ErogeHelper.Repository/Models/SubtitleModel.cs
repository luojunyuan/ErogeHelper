using System;
using Microsoft.EntityFrameworkCore;

namespace ErogeHelper.Repository.Models
{
    public class Subtitle
    {
        /// <summary>
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign Key
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Navigation Property
        /// </summary>
        public Game Game { get; set; } = new();

        public string Language { get; set; } = "en";

        /// <summary>
        /// </summary>
        public Context Context { get; set; } = new();

        /// <summary>
        /// Translated text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        public int UpVote { get; set; }

        public int DownVote { get; set; }

        /// <summary>
        /// Foreign key
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// Navigation Property
        /// </summary>
        public User Creator { get; set; } = new();

        /// <summary>
        /// </summary>
        public string CreationSubtitle { get; set; } = string.Empty;

        public DateTime CreationTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// </summary>
        public string RevisionSubtitle { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public DateTime RevisionTime { get; set; }

        /// <summary>
        /// </summary>
        public bool Deleted { get; set; }
    }

    /// <summary>
    /// Text context of the comment.
    /// </summary>
    [Owned]
    public class Context
    {
        /// <summary>
        /// </summary>
        public long Hash { get; set; }

        /// <summary>
        /// Suggest size by vnr
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 元のText
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}