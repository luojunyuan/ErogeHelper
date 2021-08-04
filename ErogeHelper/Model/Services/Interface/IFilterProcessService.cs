using ErogeHelper.Model.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Services.Interface
{
    public interface IFilterProcessService
    {
        IEnumerable<ProcessDataModel> Filter();
    }
}
