using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Dictionary.Tests
{
    [TestClass()]
    public class JishoApiTests
    {
        [TestMethod()]
        [DataRow("走った")]
        [DataRow("もだありません")]
        public async Task SearchWordAsyncTest(string query)
        {
            JishoApi.JishoResult result = await JishoApi.SearchWordAsync(query);

            // Expected, Actual
            Assert.AreEqual(200, result.Meta.Status);
        }

        [TestMethod()]
        [DataRow("pettanko", "", "ぺちゃんこ", "crushed flat", "Na-adjective")]
        [DataRow("原発性免疫不全症", "原発性免疫不全症", "げんぱつせいめんえきふぜんしょう", "primary immunodeficiency syndrome", "Noun")]
        [DataRow("にく", "肉", "にく", "flesh", "Noun")]
        public async Task SearchWordTest(string searchQuery, string firstWord, string firstReading, string firstEnglishDefinition, string firstPartOfSpeech)
        {
            var result = await JishoApi.SearchWordAsync(searchQuery);
            List<JishoApi.Data> datas = result.Data;

            Assert.AreNotEqual(0, datas.Count);
            Assert.AreNotEqual(0, datas[0].Japanese.Count);
            Assert.AreNotEqual(0, datas[0].Senses.Count);
            Assert.AreNotEqual(0, datas[0].Senses[0].EnglishDefinitions.Count);
            Assert.AreNotEqual(0, datas[0].Senses[0].PartsOfSpeech.Count);
            Assert.AreEqual(firstWord, datas[0].Japanese[0].Word);
            Assert.AreEqual(firstReading, datas[0].Japanese[0].Reading);
            Assert.AreEqual(firstEnglishDefinition, datas[0].Senses[0].EnglishDefinitions[0]);
            Assert.AreEqual(firstPartOfSpeech, datas[0].Senses[0].PartsOfSpeech[0]);
        }

        [TestMethod()]
        public async Task SearchWordFullAsync()
        {
            var result = await JishoApi.SearchWordAsync("rori");
            List<JishoApi.Data> datas = result.Data;

            Assert.AreNotEqual(0, datas.Count);
            Assert.AreEqual("ロリ", datas[0].Slug);
            Assert.IsFalse(datas[0].IsCommon);
            Assert.AreEqual(0, datas[0].Tags.Count);
            Assert.AreEqual(0, datas[0].Jlpt.Count);
            Assert.AreNotEqual(0, datas[0].Senses);
            Assert.AreEqual(1, datas[0].Senses[0].Info.Count);
            Assert.AreEqual("also written as 炉裏", datas[0].Senses[0].Info[0]);
            Assert.AreEqual(0, datas[0].Senses[0].Links.Count);
            Assert.AreEqual(2, datas[0].Senses[0].Tags.Count);
            Assert.AreEqual("Colloquialism", datas[0].Senses[0].Tags[0]);
            Assert.AreEqual("Abbreviation", datas[0].Senses[0].Tags[1]);
            Assert.AreEqual(0, datas[0].Senses[0].Restrictions.Count);
            Assert.AreEqual(1, datas[0].Senses[0].SeeAlso.Count);
            Assert.AreEqual("ロリータ", datas[0].Senses[0].SeeAlso[0]);
            Assert.AreEqual(0, datas[0].Senses[0].Antonyms.Count);
            Assert.AreEqual(0, datas[0].Senses[0].Source.Count);
            Assert.IsTrue(datas[0].Attribution.Jmdict);
            Assert.IsFalse(datas[0].Attribution.Jmnedict);
            Assert.AreEqual("False", datas[0].Attribution.Dbpedia);
        }
    }
}