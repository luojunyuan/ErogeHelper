using Dapper.Contrib.Extensions;

namespace ErogeHelper.Model.DAL.Entity.Tables
{
    [Table("GameInfo")]
    public record GameInfoTable
    {
        [ExplicitKey]
        public string Md5 { get; set; } = string.Empty;

        // Saved as "186,143,123"
        public string GameIdList { get; set; } = string.Empty;

        public string RegExp { get; set; } = string.Empty;

        public string TextractorSettingJson { get; set; } = string.Empty;

        public bool IsLoseFocus { get; set; }

        public bool IsEnableTouchToMouse { get; set; }

        public bool UseCloudSave { get; set; }

        public string SavedataPath { get; set; } = string.Empty;
    }
}
