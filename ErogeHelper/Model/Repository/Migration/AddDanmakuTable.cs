using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Repository.Migration
{
    [Migration(20210503223300)]
    class AddDanmakuTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("Danmaku")
                .WithColumn("?").AsString();
        }

        public override void Down()
        {
            Delete.Table("Danmaku");
        }
    }
}
