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

        public List<MeCabWord> IpaDicParser(string sentence)
        {
            var mecabEnumerable =  tagger.ParseToNodes(sentence);
            var ve = new VeParse(mecabEnumerable);

            return ve.Words();
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

    // These Code below is a port of https://github.com/Kimtaro/ve
    public class MeCabWord
    {
        private string _pronunciation;
        private Grammar _grammar;
        private string _lemma; // "聞く"
        private List<MeCabNode> _tokens = new(); // those which were eaten up by this one word: {聞か, せ, られ}
        private string _word; // "聞かせられ"

        public MeCabWord(
            string reading,
            string pronunciation,
            Grammar grammar,
            string lemma,
            PartOfSpeech partOfSpeech,
            string nodeStr,
            MeCabNode token)
        {
            Reading = reading;
            _pronunciation = pronunciation;
            _grammar = grammar;
            _lemma = lemma;
            PartOfSpeech = partOfSpeech;
            _word = nodeStr;
            _tokens.Add(token);
        }

        public string Word { get => _word; }

        public string Reading { get; set; }

        public PartOfSpeech PartOfSpeech { get; set; }

        public string Lemma { get => _lemma; }

        public List<MeCabNode> Tokens { get => _tokens; }

        public void AppendToWord(string suffix) => _word += suffix;

        public void AppendToReading(string suffix) => Reading += suffix;

        public void AppendToTranscription(string suffix) => _pronunciation += suffix;

        // Not sure when this would change.
        public void AppendToLemma(string suffix) => _lemma += suffix;

        public override string ToString() => _word;
    }

    class VeParse
    {
        private readonly MeCabNode[] tokenArray;
        private const string NO_DATA = "*";

        public VeParse(IEnumerable<MeCabNode> nodeEnumerable)
        {
            tokenArray = new List<MeCabNode>(nodeEnumerable).ToArray();
        }

        public List<MeCabWord> Words()
        {
            List<MeCabWord> wordList = new();
            MeCabNode previous = new();

            // XXX: begin: i = 1, end = Length-1
            for (int i = 1; i < tokenArray.Length - 1; i++)
            {
                int finalSlot = wordList.Count - 1;
                MeCabNode current = tokenArray[i];
                MeCabNode following;
                PartOfSpeech partOfSpeech;
                Grammar grammar = Grammar.Unassigned;

                bool eat_next = false;
                bool eat_lemma = true;
                bool attach_to_previous = false;
                bool also_attach_to_lemma = false;
                bool update_pos = false;

                switch (current.GetPartsOfSpeech())
                {
                    case "名詞":
                        {
                            partOfSpeech = PartOfSpeech.Noun;
                            if (current.GetPartsOfSpeechSection1().Equals(NO_DATA))
                                break;

                            switch (current.GetPartsOfSpeechSection1())
                            {
                                case "固有名詞":
                                    partOfSpeech = PartOfSpeech.ProperNoun;
                                    break;
                                case "代名詞":
                                    partOfSpeech = PartOfSpeech.Pronoun;
                                    break;
                                case "副詞可能":
                                case "サ変接続":
                                case "形容動詞語幹":
                                case "ナイ形容詞語幹":
                                    // Refers to line 213 of Ve.
                                    if (current.GetPartsOfSpeechSection2().Equals(NO_DATA))
                                        break;

                                    // protects against array overshooting.
                                    if (i == tokenArray.Length - 1)
                                        break;

                                    following = tokenArray[i + 1];
                                    switch (following.GetConjugatedForm()) // [CTYPE]
                                    {
                                        case "サ変・スル":
                                            partOfSpeech = PartOfSpeech.Verb;
                                            eat_next = true;
                                            break;
                                        case "特殊・ダ":
                                            partOfSpeech = PartOfSpeech.Adjective;
                                            if (following.GetPartsOfSpeechSection1().Equals("体言接続"))
                                            {
                                                eat_next = true;
                                                eat_lemma = false;
                                            }
                                            break;
                                        case "特殊・ナイ":
                                            partOfSpeech = PartOfSpeech.Adjective;
                                            eat_next = true;
                                            break;
                                        default:
                                            if (following.GetPartsOfSpeech().Equals("助詞")
                                                && following.Surface.Equals("に"))
                                            {
                                                // Ve script redundantly (I think) also has eat_next = false here.
                                                partOfSpeech = PartOfSpeech.Adverb;
                                            }
                                            break;
                                    }
                                    break;
                                case "非自立":
                                case "特殊":
                                    // Refers to line 233 of Ve.
                                    if (current.GetPartsOfSpeechSection2().Equals(NO_DATA))
                                        break;

                                    // protects against array overshooting.
                                    if (i == tokenArray.Length - 1)
                                        break;

                                    following = tokenArray[i + 1];

                                    switch (current.GetPartsOfSpeechSection2())
                                    {
                                        case "副詞可能":
                                            if (following.GetPartsOfSpeech().Equals("助詞")
                                                && following.Surface.Equals("に"))
                                            {
                                                partOfSpeech = PartOfSpeech.Adverb;
                                                // Changed this to false because 'case JOSHI' has 'attach_to_previous = true'.
                                                eat_next = false;
                                            }
                                            break;
                                        case "助動詞語幹":
                                            if (following.GetConjugatedForm().Equals("特殊・ダ"))
                                            {
                                                partOfSpeech = PartOfSpeech.Verb;
                                                grammar = Grammar.Auxiliary;
                                                if (following.GetInflection().Equals("体言接続"))
                                                {
                                                    eat_next = true;
                                                }
                                            }
                                            else if (following.GetPartsOfSpeech().Equals("助詞")
                                                && following.GetPartsOfSpeechSection2().Equals("副詞化"))
                                            {
                                                partOfSpeech = PartOfSpeech.Adverb;
                                                eat_next = true;
                                            }
                                            break;
                                        case "形容動詞語幹":
                                            partOfSpeech = PartOfSpeech.Adjective;
                                            if (following.GetConjugatedForm().Equals("特殊・ダ") &&
                                                following.GetInflection().Equals("体言接続")
                                                || following.GetPartsOfSpeechSection1().Equals("連体化"))
                                            {
                                                eat_next = true;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case "数":
                                    // TODO: "recurse and find following numbers and add to this word. Except non-numbers 
                                    // like 幾"
                                    // Refers to line 261.
                                    partOfSpeech = PartOfSpeech.Number;
                                    if (wordList.Count > 0 && wordList[finalSlot].PartOfSpeech.Equals(PartOfSpeech.Number))
                                    {
                                        attach_to_previous = true;
                                        also_attach_to_lemma = true;
                                    }
                                    break;
                                case "接尾":
                                    // Refers to line 267.
                                    if (current.GetPartsOfSpeechSection2().Equals("人名"))
                                    {
                                        partOfSpeech = PartOfSpeech.Suffix;
                                    }
                                    else
                                    {
                                        if (current.GetPartsOfSpeechSection2().Equals("特殊") &&
                                            current.GetOriginalForm().Equals("さ"))
                                        {
                                            update_pos = true;
                                            partOfSpeech = PartOfSpeech.Noun;
                                        }
                                        else
                                        {
                                            also_attach_to_lemma = true;
                                        }
                                        attach_to_previous = true;
                                    }
                                    break;
                                case "接続詞的":
                                    partOfSpeech = PartOfSpeech.Conjunction;
                                    break;
                                case "動詞非自立的":
                                    partOfSpeech = PartOfSpeech.Verb;
                                    grammar = Grammar.Nominal; // not using.
                                    break;
                                default:
                                    // Keep partOfSpeech as Noun, as it currently is.
                                    break;
                            }
                        }   
                        break;
                    case "接頭詞":
                        // TODO: "elaborate this when we have the "main part" feature for words?"
                        partOfSpeech = PartOfSpeech.Prefix;
                        break;
                    case "助動詞":
                        // Refers to line 290.
                        partOfSpeech = PartOfSpeech.Postposition;
                        List<string> tokushuList = new List<string> { "特殊・タ", "特殊・ナイ", "特殊・タイ", "特殊・マス", "特殊・ヌ" };
                        if (!previous.GetPartsOfSpeechSection1().Equals("係助詞")
                            && tokushuList.Contains(current.GetConjugatedForm()))
                        {
                            attach_to_previous = true;
                        }
                        else if (current.GetConjugatedForm().Equals("不変化型")
                            && current.GetOriginalForm().Equals("ん"))
                        {
                            attach_to_previous = true;
                        }
                        else if (current.GetConjugatedForm().Equals("特殊・ダ") || current.GetConjugatedForm().Equals("特殊・デス")
                            && !current.Surface.Equals("な"))
                        {
                            partOfSpeech = PartOfSpeech.Verb;
                        }
                        break;
                    case "動詞":
                        // Refers to line 299.
                        partOfSpeech = PartOfSpeech.Verb;
                        switch (current.GetPartsOfSpeechSection1())
                        {
                            case "接尾":
                                attach_to_previous = true;
                                break;
                            case "非自立":
                                if (!current.GetInflection().Equals("命令ｉ"))
                                {
                                    attach_to_previous = true;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case "形容詞":
                        partOfSpeech = PartOfSpeech.Adjective;
                        break;
                    case "助詞":
                        // Refers to line 309.
                        partOfSpeech = PartOfSpeech.Postposition;
                        List<string> qualifyingList2 = new List<string> { "て", "で", "ば" };
                        if (current.GetPartsOfSpeechSection1().Equals("接続助詞") &&
                            qualifyingList2.Contains(current.Surface))
                            //|| current.Surface.Equals("に")) // added NI
                        {
                            attach_to_previous = true;
                        }
                        break;
                    case "連体詞":
                        partOfSpeech = PartOfSpeech.Determiner;
                        break;
                    case "接続詞":
                        partOfSpeech = PartOfSpeech.Conjunction;
                        break;
                    case "副詞":
                        partOfSpeech = PartOfSpeech.Adverb;
                        break;
                    case "記号":
                        partOfSpeech = PartOfSpeech.Symbol;
                        break;
                    case "フィラー":
                    case "感動詞":
                        partOfSpeech = PartOfSpeech.Interjection;
                        break;
                    case "その他":
                        partOfSpeech = PartOfSpeech.Other;
                        break;
                    default:
                        partOfSpeech = PartOfSpeech.TBD;
                        break;
                        // C'est une catastrophe
                }

                if (attach_to_previous && wordList.Count > 0)
                {
                    // these sometimes try to add to null readings.
                    wordList[finalSlot].Tokens.Add(current);
                    wordList[finalSlot].AppendToWord(current.Surface);
                    wordList[finalSlot].AppendToReading(current.GetReading());
                    wordList[finalSlot].AppendToTranscription(current.GetPronounciation());
                    if (also_attach_to_lemma)
                    {
                        wordList[finalSlot].AppendToLemma(current.GetOriginalForm()); // lemma == basic.
                    }
                    if (update_pos)
                    {
                        wordList[finalSlot].PartOfSpeech = partOfSpeech;
                    }
                }
                else
                {
                    MeCabWord word = new MeCabWord(
                        current.GetReading(),
                        current.GetPronounciation(),
                        grammar,
                        current.GetOriginalForm(),
                        partOfSpeech,
                        current.Surface,
                        current
                    );

                    if (eat_next)
                    {
                        if (i == tokenArray.Length - 1)
                        {
                            throw new System.InvalidOperationException("There's a path that allows array overshooting.");
                        }
                        following = tokenArray[i + 1];
                        word.Tokens.Add(following);
                        word.AppendToWord(following.Surface);
                        word.AppendToReading(following.GetReading());
                        word.AppendToTranscription(following.GetPronounciation());
                        if (eat_lemma)
                        {
                            word.AppendToLemma(following.GetOriginalForm());
                        }
                    }
                    wordList.Add(word);
                }

                previous = current;
            }

            return wordList;
        }
    }

    public enum PartOfSpeech
    {
        Noun,
        ProperNoun,
        Pronoun,
        Adjective,
        Adverb,
        Determiner,
        Preposition,
        Postposition,
        Verb,
        Suffix,
        Prefix,
        Conjunction,
        Interjection,
        Number,
        Unknown,
        Symbol,
        Other,
        TBD
    }

    public enum Grammar
    {
        Unassigned,
        Auxiliary,
        Nominal,
    }
}
