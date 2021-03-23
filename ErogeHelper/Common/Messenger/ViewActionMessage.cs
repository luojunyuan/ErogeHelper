using System;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Messenger
{
    public class ViewActionMessage
    {
        /// <summary>
        /// </summary>
        /// <param name="viewModelType">Sub name `ViewModel` mapping to `View`</param>
        /// <param name="action"></param>
        /// <param name="dialogType"></param>
        /// <param name="context"></param>
        public ViewActionMessage(Type viewModelType, ViewAction action, ModernDialog? dialogType = null, object? context = null)
        {
            var viewName = context is null ? 
                viewModelType.ToString().Replace("Model", string.Empty) :
                viewModelType.ToString().Replace("Model", string.Empty)[..^4] + '.' + context;

            WindowType = Type.GetType(viewName) ?? throw new InvalidCastException(viewName);
            Action = action;
            DialogType = dialogType;
        }

        public Type WindowType { get; set; } 

        public ViewAction Action { get; set; }

        public ModernDialog? DialogType { get; set; }
    }
}