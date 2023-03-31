using System.IO;
using System.IO.Pipes;

namespace ErogeHelper.AssistiveTouch
{
    internal class IpcRenderer
    {
        private static AnonymousPipeClientStream PipeClient = null!;

        public IpcRenderer(AnonymousPipeClientStream pipeClient)
        {
            PipeClient = pipeClient;
        }

        public static void Send(IpcTypes channel)
        {
            using var sw = new StreamWriter(PipeClient);
            sw.AutoFlush = true;
            sw.WriteLine(channel);
        }
    }

    public enum IpcTypes
    {
        Loaded,
    }
}
