using FluentMigrator;

namespace ErogeHelper.Model.Repository.Migration
{
    [Migration(20210421201800)]
    public class AddUserTermTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("UserTerm")
                .WithColumn("From").AsString()
                .WithColumn("To").AsString();
        }

        public override void Down()
        {
            Delete.Table("UserTerm");
        }
    }
}
