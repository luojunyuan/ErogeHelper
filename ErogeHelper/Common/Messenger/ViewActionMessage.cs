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
        public ViewActionMessage(Type viewModelType, ViewAction action)
        {
            var viewName = viewModelType.ToString().Replace("Model", string.Empty);
            WindowType = Type.GetType(viewName) ?? throw new InvalidCastException(viewName);
            Action = action;
        }

        public Type WindowType { get; set; } 

        public ViewAction Action { get; set; }
    }
}