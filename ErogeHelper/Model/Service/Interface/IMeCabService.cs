using ErogeHelper.Common.Entity;
using System.Collections.Generic;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IMeCabService
    {
        void CreateTagger();

        IEnumerable<MeCabWord> MeCabWordUniDicEnumerable(string sentence);
    }
}