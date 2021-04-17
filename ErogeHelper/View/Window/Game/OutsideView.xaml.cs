using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Caliburn.Micro;
using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Messenger;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.View.Window.Game
{
    /// <summary>
    /// OutsideView.xaml 的交互逻辑
    /// </summary>
    public partial class OutsideView : IHandle<ViewActionMessage>
    {
        public OutsideView()
        {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();
            var gameWindowHooker = IoC.Get<IGameWindowHooker>();

            _eventAggregator.SubscribeOnUIThread(this);
            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            gameWindowHooker.GamePosChanged += pos =>
            {
                Left += pos.HorizontalChange / _dpi;
                Top += pos.VerticalChange / _dpi;
            };
            Visibility = Visibility.Collapsed;
            Loaded += (_, _) =>
            {
                Utils.HideWindowInAltTab(this);
                EnableBlur();
            };
        }

        private readonly IEventAggregator _eventAggregator;
        private double _dpi;

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
        }

        private void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new NativeMethods.AccentPolicy
            {
                AccentState = NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new NativeMethods.WindowCompositionAttributeData
            {
                Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            NativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public Task HandleAsync(ViewActionMessage message, CancellationToken cancellationToken)
        {
            if (message.WindowType == GetType())
            {
                switch (message.Action)
                {
                    case ViewAction.Hide:
                        Hide();
                        break;
                    case ViewAction.Show:
                        this.MoveToCenter();
                        Show();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return Task.CompletedTask;
        }
        protected override void OnClosed(EventArgs e) => _eventAggregator.Unsubscribe(this);

        // MouseDown="OutsideView_OnMouseDown" would cover the CardPopup 
        private void OutsideView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        // QUESTION: GC active frequently when resize the window with too long Japanese text
        private void ResizeGripper_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Width + e.HorizontalChange >= 100 && Width >= 100)
            {
                Width += e.HorizontalChange;
            }

            if (Height + e.VerticalChange >= 100 && Height >= 100)
            {
                Height += e.VerticalChange;
            }
        }
    }
}
