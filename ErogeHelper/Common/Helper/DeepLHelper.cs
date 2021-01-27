using Caliburn.Micro;
using Serilog;
using System;
using System.Threading;
using System.Windows;
using WindowsInput.Events;

namespace ErogeHelper.Common.Helper
{
    class DeepLHelper : StaHelper
    {
        readonly string _format;
        readonly object _data;

        public DeepLHelper(string format, object data)
        {
            _format = format;
            _data = data;
        }

        protected override void Work()
        {
            var obj = new DataObject(
                _format,
                _data
            );

            Clipboard.SetDataObject(obj, true);
        }
    }

    abstract class StaHelper
    {
        readonly ManualResetEvent _complete = new ManualResetEvent(false);

        public void Go()
        {
            var thread = new Thread(new ThreadStart(DoWork))
            {
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        // Thread entry method
        private void DoWork()
        {
            try
            {
                _complete.Reset();
                Work();
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
                if (DontRetryWorkOnFailed)
                    throw;
                else
                {
                    try
                    {
                        Thread.Sleep(100);
                        Work();
                    }
                    catch
                    {
                        // ex from first exception
                        Log.Warn(ex.Message);
                    }
                }
            }
            finally
            {
                _complete.Set();
                WindowsInput.Simulate.Events()
                    .ClickChord(KeyCode.Control, KeyCode.A)
                    .Invoke();
                WindowsInput.Simulate.Events()
                    .ClickChord(KeyCode.Control, KeyCode.V)
                    .Invoke();
            }
        }

        public bool DontRetryWorkOnFailed { get; set; }

        // Implemented in base class to do actual work.
        protected abstract void Work();
    }

    //private void ClipboardPasteSTA(string text)
    //{
    //    Thread thread = new Thread(() => Clipboard.SetText(text));
    //    thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
    //    thread.Start();
    //    thread.Join(); //Wait for the thread to end
    //}
}
