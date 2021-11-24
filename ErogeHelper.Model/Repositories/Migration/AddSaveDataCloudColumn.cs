﻿using FluentMigrator;

namespace ErogeHelper.Model.Repositories.Migration
{
    [Migration(20210819071700)]
    public class AddSaveDataCloudColumn : AutoReversingMigration
    {
        public override void Up() =>
            Alter.Table("GameInfo")
                .AddColumn("UseCloudSave").AsBoolean().WithDefaultValue(false)
                .AddColumn("SaveDataPath").AsString().WithDefaultValue(string.Empty);
    }
}