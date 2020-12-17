using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Dictionary
{
    class DictManager
    {
        private static LingoesDict lingoes = new LingoesDict();

        public static void init()
        {
            
        }

        public delegate string CallBack();
        public static void Find(string word, CallBack callBack)
        {
        }
    }

    class WordInfo
    { }
}
