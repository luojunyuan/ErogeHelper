using FluentMigrator;

namespace ErogeHelper.Model.Repositories.Migration
{
    [Migration(20210421201800)]
    public class AddUserTermTable : AutoReversingMigration
    {
        public override void Up() =>
            Create.Table("UserTerm")
                .WithColumn("From").AsString()
                .WithColumn("To").AsString();
    }
}
