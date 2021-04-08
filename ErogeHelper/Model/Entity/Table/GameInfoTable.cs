namespace ErogeHelper.Model.Entity.Table
{
    public class GameInfoTable
    {
        public string Md5 { get; set; } = string.Empty;
        // Saved as "186,143,123"
        public string GameIdList { get; set; } = string.Empty;
        public string TextractorSettingJson { get; set; } = string.Empty;

        public bool IsLostFocus { get; set; }
        public bool IsEnableTouchToMouse { get; set; }
    }
}