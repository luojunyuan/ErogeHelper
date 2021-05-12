namespace ErogeHelper.Common.Contract
{
    public static class ConstantValues
    {
        public const string SerilogOutputTemplate =
            "[{Timestamp:MM-dd-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        public const int TextractorReAttachBlockTime = 100;

        public const int WaitGameStartTimeout = 20000;

        public const int GoodWindowSize = 140;
    }
}
