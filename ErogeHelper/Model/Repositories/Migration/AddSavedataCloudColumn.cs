using FluentMigrator;

namespace ErogeHelper.Model.Repositories.Migration
{
    [Migration(20210819071700)]
    public class AddSavedataCloudColumn : AutoReversingMigration
    {
        public override void Up() =>
            Alter.Table("GameInfo")
                .AddColumn("UseCloudSave").AsBoolean().WithDefaultValue(false)
                .AddColumn("SavedataPath").AsString().WithDefaultValue(string.Empty);
    }
}
