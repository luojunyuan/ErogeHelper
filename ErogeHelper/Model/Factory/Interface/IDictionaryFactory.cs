using ErogeHelper.Common.Enum;

namespace ErogeHelper.Model.Factory.Interface
{
    public interface IDictionaryFactory
    {
        IDict GetDictInstance(DictType dictType);
    }
}