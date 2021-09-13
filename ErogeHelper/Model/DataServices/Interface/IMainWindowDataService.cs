using System.Reactive.Subjects;
using Vanara.PInvoke;

namespace ErogeHelper.Model.DataServices.Interface
{
    public interface IMainWindowDataService
    {
        public ReplaySubject<double> DpiSubject { get; }

        HWND Handle { get; }

        void SetHandle(HWND handle);
     
        public Subject<bool> AssistiveTouchBigSizeSubject { get; }
    }
}
