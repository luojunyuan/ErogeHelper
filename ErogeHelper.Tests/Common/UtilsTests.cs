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
        public void TextEvaluateWithRegExpTest_Normal()
        {
            var rawText = "this is raw text";
            var expectText = "th|~S~|is|~E~| |~S~|is|~E~| raw text";
            var regExpPattern = "is";

            string wapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wapperWithSpecialString);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTest_Wildcard()
        {
            var rawText = "草薙の読み方は<ruby>くさなぎ<ruby/>草薙と言っています";
            var expectText = "草薙の読み方は|~S~|<ruby>|~E~|くさなぎ|~S~|<ruby/>|~E~|草薙と言っています";
            var regExpPattern = "<.*?>";

            string wapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wapperWithSpecialString);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTest_WildcardSecond()
        {
            var rawText = "また……会";
            var expectText = "|~S~|ま|~E~||~S~|た|~E~||~S~|…|~E~||~S~|…|~E~||~S~|会|~E~|";
            var regExpPattern = ".";

            string wapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wapperWithSpecialString);
        }

        [TestMethod()]
        public void TextEvaluateWithRegExpTest_Special()
        {
            var rawText = "また……会";
            var expectText = "また……会";
            var regExpPattern = "ま|"; // XXX: idk how to deal |
            // if pattern over with '|', just return text itself

            string wapperWithSpecialString = Utils.TextEvaluateWithRegExp(rawText, regExpPattern);

            Assert.AreEqual(expectText, wapperWithSpecialString);
        }
    }
}