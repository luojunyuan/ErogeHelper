using FluentMigrator;

namespace ErogeHelper.Model.Repositories.Migration;

[Migration(20220125225300)]
public class AddCommentTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Comment")
            .WithColumn("Hash").AsInt64()
            .WithColumn("Text").AsString()
            .WithColumn("UserComment").AsString()
            .WithColumn("Username").AsString()
            .WithColumn("CommentId").AsString()
            .WithColumn("GameMd5").AsString()
            .WithColumn("CreationTime").AsDateTime();

        Alter.Table("GameInfo")
           .AddColumn("CommentLastSyncTime").AsDateTime().WithDefaultValue(DateTime.MinValue);
    }
}
