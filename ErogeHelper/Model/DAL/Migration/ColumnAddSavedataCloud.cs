using FluentMigrator;

namespace ErogeHelper.Model.DAL.Migration
{
    [Migration(20210819071700)]
    public class ColumnAddSavedataCloud : FluentMigrator.Migration
    {
        public override void Up() =>
            Alter.Table("GameInfo")
                .AddColumn("UseCloudSave").AsBoolean().WithDefaultValue(false)
                .AddColumn("CloudPath").AsString();

        public override void Down() =>
            Delete
                .Column("UseCloudSave")
                .Column("CloudPath")
                .FromTable("GameInfo");
    }
}
