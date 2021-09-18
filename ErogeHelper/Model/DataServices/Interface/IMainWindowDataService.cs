using System.Reactive.Subjects;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IMainWindowDataService
    {
        ReplaySubject<double> DpiSubject { get; }

        HWND Handle { get; }

        void SetHandle(HWND handle);

        ReplaySubject<HWND> HandleSubj { get; }

        Subject<bool> AssistiveTouchBigSizeSubject { get; }
    }
}
