using System.Reactive.Subjects;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IMainWindowDataService
    {
        Subject<bool> AssistiveTouchBigSizeSubj { get; }

        ReplaySubject<HWND> HandleSubj { get; }

        HWND Handle { get; }

        void SetHandle(HWND handle);
    }
}
