using System.IO.Pipes;

namespace ErogeHelper;

internal class IpcMain
{
    private readonly AnonymousPipeServerStream _serverIn;

    public IpcMain(AnonymousPipeServerStream serverIn)
    {
        _serverIn = serverIn;
        Start();
    }

    private void Start()
    {
        new TaskFactory().StartNew(() =>
        {
            Thread.CurrentThread.Name = "IpcMain listening loop";
            var sr = new StreamReader(_serverIn);
            string? temp;
            while (true)
            {
                temp = sr.ReadLine();
                Enum.TryParse(temp, out IpcTypes channel);
                if (channel == IpcTypes.Loaded && DictionaryOfEvents.ContainsKey(IpcTypes.Loaded))
                {
                    DictionaryOfEvents[IpcTypes.Loaded].Invoke();
                    DictionaryOfEvents.Remove(IpcTypes.Loaded);
                }
            }
        }, TaskCreationOptions.LongRunning);
    }


    private static readonly Dictionary<IpcTypes, Action> DictionaryOfEvents = new();

    public static void Once(IpcTypes channel, Action callback)
    {
        DictionaryOfEvents.Add(channel, callback);
    }
}

public enum IpcTypes
{
    Loaded,
}
