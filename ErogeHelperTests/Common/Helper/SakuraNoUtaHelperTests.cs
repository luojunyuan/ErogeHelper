using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErogeHelper.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Helper.Tests
{
    [TestClass()]
    public class SakuraNoUtaHelperTests
    {
        [TestMethod()]
        public void QueryTextTest()
        {
            var source = "「なるほど、そういう気持ちは少し分かるかも。でもさ、程度って言うのもあるでしょ？　何を熟読してるのよ。待たされる私の身にもなりなさいって」";
            var source2 = "　俺は最初のページからすべてに目を通す。そして……。"; // \u0005


            Assert.Fail();
        }
    }
}