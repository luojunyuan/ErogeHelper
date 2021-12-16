using FluentMigrator;

namespace ErogeHelper.Model.Repositories.Migration;

[Migration(20210325151400)]
public class _001AddGameInfoTable : AutoReversingMigration
{
    public override void Up() =>
        Create.Table("GameInfo")
            .WithColumn("Md5").AsString()
            .WithColumn("GameIdList").AsString()
            .WithColumn("RegExp").AsString()
            .WithColumn("TextractorSettingJson").AsString()
            .WithColumn("IsLoseFocus").AsBoolean()
            .WithColumn("IsEnableTouchToMouse").AsBoolean();
}
