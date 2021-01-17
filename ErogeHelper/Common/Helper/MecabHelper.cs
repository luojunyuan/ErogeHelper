using ErogeHelper.Common.Extension;
using ErogeHelper.Model;
using MeCab;
using MeCab.Extension.IpaDic;
using MeCab.Extension.UniDic;
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
            //parameter.DicDir = @"C:\Users\k1mlka\source\repos\luojunyuan\Eroge-Helper-pakage\mecab-UniDic";
            tagger = MeCabTagger.Create(parameter);
        }

        public IEnumerable<MecabWordInfo> MecabWordIpaEnumerable(string sentence)
        {
            foreach (var node in tagger.ParseToNodes(sentence))
            {
                if (node.CharType > 0)
                {
                    var features = node.Feature.Split(',');

                    #region 填充 MecabWordInfo 各项 Property
                    MecabWordInfo word = new MecabWordInfo
                    {
                        Word = node.Surface,
                        PartOfSpeech = node.GetPartsOfSpeech(),
                        Kana = node.GetReading()
                    };

                    if (string.IsNullOrWhiteSpace(word.Kana) || 
                        word.PartOfSpeech == "記号" ||
                        WanaKana.IsHiragana(node.Surface) ||
                        WanaKana.IsKatakana(node.Surface))
                    {
                        word.Kana = " ";
                    }
                    else
                    {
                        if (DataRepository.Romaji == true)
                        {
                            word.Kana = WanaKana.ToRomaji(word.Kana);
                        }
                        else if (DataRepository.Hiragana == true)
                        {
                            // Not Implament yet
                            //word.Kana = WanaKana.ToHiragana(word.Kana);

                            word.Kana = word.Kana.Katakana2Hiragana();
                        }
                        // Katakana by default
                    }
                    #endregion

                    yield return word;
                }
            }
        }

        public IEnumerable<MecabWordInfo> MecabWordUniEnumerable(string sentence)
        {
            foreach (var node in tagger.ParseToNodes(sentence))
            {
                if (node.CharType > 0)
                {
                    var features = node.Feature.Split(',');

                    #region 填充 MecabWordInfo 各项 Property
                    MecabWordInfo word = new MecabWordInfo
                    {
                        Word = node.Surface,
                        PartOfSpeech = node.GetPos1(),
                        Kana = " "
                    };
                    // 加这一步是为了防止乱码进入分词导致无法读取假名
                    if (features.Length >= 8)
                    {
                        word.Kana = node.GetPron();
                    }

                    if (word.PartOfSpeech == "補助記号" ||
                        WanaKana.IsHiragana(node.Surface) ||
                        WanaKana.IsKatakana(node.Surface) ||
                        node.GetGoshu() == "記号")
                    {
                        word.Kana = " ";
                    }

                    if (node.GetGoshu() == "外")
                    {
                        var lemma = node.GetLemma().Split('-');
                        if (lemma.Length == 2)
                            word.Kana = lemma[1];
                    }
                    #endregion

                    yield return word;
                }
            }
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
        /// 假名，null 在这里也不会报错
        /// </summary>
        public string Kana;
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
