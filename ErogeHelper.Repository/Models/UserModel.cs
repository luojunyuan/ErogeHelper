using System;

namespace ErogeHelper.Repository.Models
{
    public class User
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The user's login name.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Other information about the user.
        /// </summary>
        public string ExtraInfo { get; set; } = string.Empty;

        /// <summary>
        /// The language used by this user.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// The user's avatar (file token).
        /// </summary>
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// The user's homepage url.
        /// </summary>
        public string HomePage { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// </summary>
        public DateTime AccessTime { get; set; }

        /// <summary>
        /// </summary>
        public DateTime ModifiedTime { get; set; }
    }
}