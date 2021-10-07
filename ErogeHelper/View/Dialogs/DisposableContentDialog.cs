using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.View.Windows;
using ModernWpf.Controls;
using Punchclock;
using Splat;
using System;
using System.Threading.Tasks;

namespace ErogeHelper.View.Dialogs
{
    internal class DisposableContentDialog : ContentDialog, IDisposable, IEnableLogger
    {
        private readonly OperationQueue _queue;

        public DisposableContentDialog()
        {
            _queue = DependencyResolver.GetService<OperationQueue>();
            Owner = DependencyResolver.GetService<PreferenceWindow>();
        }

        public virtual void Dispose() => Hide();

        public async Task<ContentDialogResult> SafeShowAsync(
                int priority = 1, ContentDialogResult defaultResult = ContentDialogResult.None) =>
            await _queue.Enqueue(priority, ConstantValues.TaskQueueContentDialogKey, async () =>
        {
            var res = defaultResult;
            try
            {
                await Dispatcher.Invoke(async () =>
                {
                    Owner.Focus();
                    res = await ShowAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                this.Log().Debug(ex);
            }
            return res;
        });
    }
}
