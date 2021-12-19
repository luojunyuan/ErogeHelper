﻿namespace ErogeHelper.Shared.Contracts;

public static class EHContext
{
    public static readonly string RoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static readonly string RoamingEHFolder = Path.Combine(RoamingPath, "ErogeHelper");

    public static readonly string ConfigFilePath = Path.Combine(RoamingEHFolder, "EHSettings.json");

    public static readonly string DbFilePath = Path.Combine(RoamingEHFolder, "eh.db");

    public static readonly string DbConnectString = $"Data Source={DbFilePath}";

    public static readonly string MeCabDicFolder = Path.Combine(RoamingEHFolder, "dic");

    public static readonly string MeCabDicFile = Path.Combine(MeCabDicFolder, "char.bin");
}
