using FluentMigrator;

namespace ErogeHelper.Model.DTO.Migration
{
    [Migration(20210819071700)]
    public class ColumnAddSavedataCloud : FluentMigrator.Migration
    {
        public override void Up() =>
            Alter.Table("GameInfo")
                .AddColumn("UseCloudSave").AsBoolean().WithDefaultValue(false)
                .AddColumn("SavedataPath").AsString().WithDefaultValue(string.Empty);

        public override void Down() =>
            Delete
                .Column("UseCloudSave")
                .Column("SavedataPath")
                .FromTable("GameInfo");
    }
}
