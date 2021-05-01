using ErogeHelper.ViewModel.Entity.NotifyItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Messenger
{
    public class NewTextThreadSelectedMessage
    {
        public NewTextThreadSelectedMessage(HookMapItem hookMapItem, bool status)
        {
            HookMapItem = hookMapItem;
            Status = status;
        }

        public HookMapItem HookMapItem { get; }
        public bool Status { get; }
    }
}
