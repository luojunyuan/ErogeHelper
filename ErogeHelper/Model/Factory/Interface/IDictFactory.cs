using ErogeHelper.Common.Enum;

namespace ErogeHelper.Model.Factory.Interface
{
    public interface IDictFactory
    {
        IDict GetDictInstance(DictType dictType);
    }
}