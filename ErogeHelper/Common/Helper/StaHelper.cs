using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput.Events;

namespace ErogeHelper.Common.Helper
{
    abstract class StaHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(StaHelper));

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
                log.Warn(ex.Message);
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
                        log.Warn(ex.Message);
                    }
                }
            }
            finally
            {
                _complete.Set();
                WindowsInput.Simulate.Events()
                    .ClickChord(KeyCode.Control, KeyCode.V)
                    .Invoke();
            }
        }

        public bool DontRetryWorkOnFailed { get; set; }

        // Implemented in base class to do actual work.
        protected abstract void Work();
    }
}
