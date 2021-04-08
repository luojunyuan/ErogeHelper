namespace ErogeHelper.Model.Service.Interface
{
    public interface IAdjustScreenBrightness
    {
        bool GetBrightness(out short currentBrightness, out short minBrightness, out short maxBrightness);

        void SetBrightness(short brightness);
    }
}