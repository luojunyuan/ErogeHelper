using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Dictionary.Interface;

namespace ErogeHelper.Model.Factory.Interface
{
    public interface IDictFactory
    {
        IDict GetDictInstance(DictType dictType);
    }
}