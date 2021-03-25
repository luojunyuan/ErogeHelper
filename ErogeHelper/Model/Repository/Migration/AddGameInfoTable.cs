using FluentMigrator;

namespace ErogeHelper.Model.Repository.Migration
{
    [Migration(20210325151400)]
    public class AddGameInfoTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("GameInfo")
                .WithColumn("Md5").AsString()
                .WithColumn("GameIdList").AsString()
                .WithColumn("GameSettingJson").AsString();
        }

        public override void Down()
        {
            Delete.Table("GameInfo");
        }
    }
}