using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErogeHelper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void TextEvaluateWithRegExpTest()
        {
            var rawText = "this is raw text";
            var regExpPattern = "is";

            string wapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual("th|~S~|is|~E~| |~S~|is|~E~| raw text", wapperWithSpecialString);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTestSecond()
        {
            var rawText = "草薙の読み方は<ruby>くさなぎ<ruby/>草薙と言っています";
            var regExpPattern = "<.*?>";

            string wapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual("草薙の読み方は|~S~|<ruby>|~E~|くさなぎ|~S~|<ruby/>|~E~|草薙と言っています", wapperWithSpecialString);
        }
    }
}