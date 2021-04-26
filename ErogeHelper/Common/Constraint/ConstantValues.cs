using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Constraint
{
    static class ConstantValues
    {
        public const string SerilogOutputTemplate = 
            "[{Timestamp:MM-dd-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    }
}
