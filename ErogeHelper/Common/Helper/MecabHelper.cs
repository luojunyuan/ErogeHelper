using MeCab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WanaKanaSharp;

namespace ErogeHelper.Common.Helper
{
    class MecabHelper
    {
        private readonly MeCabParam parameter;
        private readonly MeCabTagger tagger;

        public MecabHelper()
        {
            parameter = new MeCabParam();
            tagger = MeCabTagger.Create(parameter);
        }

        /// <summary>
        /// Progress sentence
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<MecabWordInfo> SentenceHandle(string sentence)
        {

            List<MecabWordInfo> WordInfoList = new List<MecabWordInfo>();

            foreach (var node in tagger.ParseToNodes(sentence))
            {
                if (node.CharType > 0)
                {
                    var features = node.Feature.Split(',');

                    #region 填充 MecabWordInfo 各项 Property
                    MecabWordInfo word = new MecabWordInfo
                    {
                        Word = node.Surface,
                        PartOfSpeech = features[0],
                        Description = features[1],
                        Feature = node.Feature,
                        Kana = " "
                    };
                    // 加这一步是为了防止乱码进入分词导致无法读取假名
                    if (features.Length >= 8)
                    {
                        word.Kana = features[7];
                    }

                    if (word.PartOfSpeech == "記号" ||
                        WanaKana.IsHiragana(node.Surface) ||
                        WanaKana.IsKatakana(node.Surface))
                    {
                        word.Kana = " ";
                    }
                    #endregion

                    WordInfoList.Add(word);
                }
            }

            return WordInfoList;
        }
    }

    public struct MecabWordInfo
    {

        /// <summary>
        /// 单词
        /// </summary>
        public string Word;

        /// <summary>
        /// 品詞（ひんし）
        /// </summary>
        public string PartOfSpeech;

        /// <summary>
        /// 词性说明
        /// </summary>
        public string Description;

        /// <summary>
        /// 假名 null 在这里也不会报错
        /// </summary>
        public string Kana;

        /// <summary>
        /// 全ての素性文字列
        /// </summary>
        public string Feature;
    }

    enum Hinshi
    {
        名詞,
        助詞,
        動詞,
        助動詞,
        記号,
        副詞
    }
}
