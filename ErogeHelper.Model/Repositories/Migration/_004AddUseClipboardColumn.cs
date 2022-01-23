using FluentMigrator;

namespace ErogeHelper.Model.Repositories.Migration;

[Migration(20220123173900)]
public class _004AddUseClipboardColumn : AutoReversingMigration
{
    public override void Up() =>
        Alter.Table("GameInfo")
            .AddColumn("UseClipboard").AsBoolean().WithDefaultValue(false);
}
