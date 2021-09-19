namespace ErogeHelper.Model.Services.Interface
{
    public interface ITouchConversionHooker
    {
        bool Enable { get; set; }

        void Init();
    }
}