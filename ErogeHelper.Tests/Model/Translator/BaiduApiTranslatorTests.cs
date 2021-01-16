using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErogeHelper.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ErogeHelper.Model.Translator.Tests
{
    [TestClass()]
    public class BaiduApiTranslatorTests
    {
        [TestMethod()]
        public async Task TranslateImplAsyncTest()
        {
            var queryText = "わたし";
            var redict = "我";

            var BaiduApiTranslatorInstance = new BaiduApiTranslator();
            var result = await BaiduApiTranslatorInstance.TranslateAsyncImpl(queryText, Language.Japenese, Language.ChineseSimplified);
            Assert.AreEqual(redict, result);
        }

        [TestMethod()]
        public async Task TranslatorInterfaceCallerTest()
        {
            var queryText = "皇帝の新しい心";
            var redict = "皇帝的新心";

            ITranslator BaiduApiTranslatorInstance = new BaiduApiTranslator();
            var result = await BaiduApiTranslatorInstance.TranslateAsync(queryText, Language.Japenese, Language.ChineseSimplified);
            Assert.AreEqual(redict, result);
        }

        [TestMethod()]
        public void MultiRequestSpendTimeTest()
        {
            var queryText = "皇帝の新しい心";
            var redict = "皇帝的新心";
            string result = string.Empty;

            var BaiduApiTranslatorInstance = new BaiduApiTranslator();

            List<Task> tasks = new List<Task>();
            // bigger times may cause code:54003	访问频率受限	请降低您的调用频率，或进行身份认证后切换为高级版/尊享版
            var times = 3; 
            for (int i = 0; i < times; i++)
            {
                tasks.Add(Task.Run(async () => 
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    result = await BaiduApiTranslatorInstance.TranslateAsyncImpl(queryText, Language.Japenese, Language.ChineseSimplified);
                    stopWatch.Stop();
                    Trace.WriteLine(stopWatch.ElapsedMilliseconds + "ms");
                }));
            }
            Task t = Task.WhenAll(tasks);
            t.Wait();

            Assert.AreEqual(redict, result);
        }
    }
}