using ErogeHelper.Common.Entity;
using System.Collections.Generic;
using ErogeHelper.Model.Repository;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IMeCabService
    {
        void CreateTagger(string dicDir);

        IEnumerable<MeCabWord> MeCabWordUniDicEnumerable(string sentence, EhConfigRepository config);
    }
}