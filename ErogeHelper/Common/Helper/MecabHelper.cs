using ErogeHelper.Common.Extension;
using ErogeHelper.Model;
using MeCab;
using MeCab.Extension.IpaDic;
using MeCab.Extension.UniDic;
using System.Collections.Generic;
using System.IO;
using WanaKanaSharp;

namespace ErogeHelper.Common.Helper
{
    public class MecabHelper
    {
        private readonly MeCabParam parameter;
        private MeCabTagger tagger = null!;

        public MecabHelper()
        {
            parameter = new MeCabParam();
        }

        public bool CanCreateTagger { get => File.Exists(DataRepository.AppDataDir + @"\dic\char.bin"); }

        public void CreateTagger()
        {
            parameter.DicDir = DataRepository.AppDataDir + @"\dic";
            tagger = MeCabTagger.Create(parameter);
        }

        public IEnumerable<MecabWordInfo> MecabWordIpadicEnumerable(string sentence)　
        {
            // Add Ve paser
            foreach (var node in tagger.ParseToNodes(sentence))
            {
                if (node.CharType > 0)
                {
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

        // https://stackoverflow.com/questions/7300361/memory-mapped-files-ioexception-on-createviewaccessor-for-large-data
        public IEnumerable<MecabWordInfo> MecabWordUnidicEnumerable(string sentence)
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
