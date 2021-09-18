using ErogeHelper.Model.DataServices.Interface;
using System.Reactive.Subjects;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices
{
    public class MainWindowDataService : IMainWindowDataService
    {
        public ReplaySubject<double> DpiSubject { get; init; } = new(1);

        public HWND Handle { get; private set; }

        public void SetHandle(HWND handle) => Handle = handle;

        public Subject<bool> AssistiveTouchBigSizeSubject { get; init; } = new();

        public ReplaySubject<HWND> HandleSubj { get; init; } = new(1);
    }
}
