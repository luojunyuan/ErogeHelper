using Caliburn.Micro;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Extension
{
    static class FurikanaStringExtention
    {
        public static string Katakana2Hiragana(this string input)
        {
            if (input == string.Empty)
                return " ";

            string transform = string.Empty;
            foreach(var character in input)
            {
                if (Katakana2HiraganaMap.TryGetValue(character, out char testValue))
                {
                    transform += Katakana2HiraganaMap[character];
                }
                else
                {
                    Log.Warn($"No {testValue} in Katakana2HiraganaMap");
                }
            }
            return transform;
        }

        static readonly Dictionary<char, char> Katakana2HiraganaMap = new Dictionary<char, char>()
        {
            { 'ア', 'あ' }, { 'イ', 'い' }, { 'ウ', 'う' }, { 'エ', 'え' }, { 'オ', 'お' },
            { 'カ', 'か' }, { 'キ', 'き' }, { 'ク', 'く' }, { 'ケ', 'け' }, { 'コ', 'こ' },
            { 'サ', 'さ' }, { 'シ', 'し' }, { 'ス', 'す' }, { 'セ', 'せ' }, { 'ソ', 'そ' },
            { 'タ', 'た' }, { 'チ', 'ち' }, { 'ツ', 'つ' }, { 'テ', 'て' }, { 'ト', 'と' },
            { 'ナ', 'な' }, { 'ニ', 'に' }, { 'ヌ', 'ぬ' }, { 'ネ', 'ね' }, { 'ノ', 'の' },
            { 'ハ', 'は' }, { 'ヒ', 'ひ' }, { 'フ', 'ふ' }, { 'ヘ', 'ヘ' }, { 'ホ', 'ほ' },
            { 'マ', 'ま' }, { 'ミ', 'み' }, { 'ム', 'む' }, { 'メ', 'め' }, { 'モ', 'も' },
            { 'ヤ', 'や' },                { 'ユ', 'ゆ' },                { 'ヨ', 'よ' },
            { 'ラ', 'ら' }, { 'リ', 'り' }, { 'ル', 'る' }, { 'レ', 'れ' }, { 'ロ', 'ろ' },
            { 'ワ', 'わ' },                                              { 'ヲ', 'を' },
            { 'ン', 'ん' },

            { 'ガ', 'が' }, { 'ギ', 'ぎ' }, { 'グ', 'ぐ' }, { 'ゲ', 'げ' }, { 'ゴ', 'ご' },
            { 'ザ', 'ざ' }, { 'ジ', 'じ' }, { 'ズ', 'ず' }, { 'ゼ', 'ぜ' }, { 'ゾ', 'ぞ' },
            { 'ダ', 'だ' }, { 'ヂ', 'ぢ' }, { 'ヅ', 'づ' }, { 'デ', 'で' }, { 'ド', 'ど' },
            { 'バ', 'ば' }, { 'ビ', 'び' }, { 'ブ', 'ぶ' }, { 'ベ', 'べ' }, { 'ボ', 'ぼ' },

            { 'パ', 'ぱ' }, { 'ピ', 'ぴ' }, { 'プ', 'ぷ' }, { 'ペ', 'ペ' }, { 'ポ', 'ぽ' },

            // 捨て仮名（すてがな）https://rey1229.hatenablog.com/entry/2016/06/26/000129
            { 'ァ', 'ぁ' }, { 'ィ', 'ぃ' }, { 'ゥ', 'ぅ' }, { 'ェ', 'ぇ' }, { 'ォ', 'ぉ' },
            { 'ヵ', 'ゕ' },                　　　　　　　　　 { 'ヶ', 'ゖ' },
                                          { 'ッ', 'っ' },
            { 'ャ', 'ゃ' },                { 'ュ', 'ゅ' },                { 'ョ', 'ょ' },
            { 'ヮ', 'ゎ' },

            // Specific
            { 'ー', 'い' },
        };
    }
}
