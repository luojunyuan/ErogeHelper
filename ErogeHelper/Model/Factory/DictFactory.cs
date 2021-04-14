using System;
using ErogeHelper.Common.Enum;
using ErogeHelper.Model.Factory.Dictionary;
using ErogeHelper.Model.Factory.Interface;
using ErogeHelper.Model.Repository;

namespace ErogeHelper.Model.Factory
{
    public class DictFactory : IDictFactory
    {
        public DictFactory(EhConfigRepository ehConfigRepository)
        {
            _mojiDict = new MojiDict(ehConfigRepository.MojiSessionToken);
            _jishoDict = new JishoDict();
        }

        private readonly MojiDict _mojiDict;
        private readonly JishoDict _jishoDict;

        public IDict GetDictInstance(DictType dictType)
        {
            return dictType switch
            {
                DictType.Moji => _mojiDict,
                DictType.Jisho => _jishoDict,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}