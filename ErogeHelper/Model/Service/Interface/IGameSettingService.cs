namespace ErogeHelper.Model.Service.Interface
{
    interface IGameSettingService
    {
        TextSetting? GetInfoByMD5(string md5);
    }
}
