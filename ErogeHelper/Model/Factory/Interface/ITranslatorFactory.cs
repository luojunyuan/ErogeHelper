using System.Collections.Generic;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Model.Factory.Interface
{
    public interface ITranslatorFactory
    {
        List<ITranslator> AllInstance { get; }

        List<ITranslator> GetEnabledTranslators();

        ITranslator GetTranslator(TranslatorName name);
    }
}