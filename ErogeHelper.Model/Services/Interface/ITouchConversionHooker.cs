namespace ErogeHelper.Model.Services.Interface;

public interface ITouchConversionHooker : IDisposable
{
    bool Enable { get; set; }

    void Init();
}

