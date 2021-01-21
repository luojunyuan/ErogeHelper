using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErogeHelper.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErogeHelper.ViewModel.Control;
using System.Diagnostics;

namespace ErogeHelper.Common.Helper.Tests
{
    [TestClass()]
    public class MecabHelperTests
    {
        [TestMethod()]
        public void MecabWordIpaEnumerableCheck()
        {
            var mecabHelper = new MecabHelper();

            var sentence = "スキル使用者と隣にパートナーがいれば技が発動します。また、発動にはパートナーのＳＰも必要です。";

            foreach (MecabWordInfo mecabWord in mecabHelper.MecabWordIpaEnumerable(sentence))
            {
                Trace.WriteLine($"{mecabWord.Word} {mecabWord.Kana}");
            }
            Assert.IsTrue(true);
        }
    }
}