using FluentMigrator;

namespace ErogeHelper.Model.Repositories.Migration;

[Migration(20220123173900)]
public class AddUseClipboardColumn : AutoReversingMigration
{
    public override void Up() =>
        Alter.Table("GameInfo")
            .AddColumn("UseClipboard").AsBoolean().WithDefaultValue(false);
}
