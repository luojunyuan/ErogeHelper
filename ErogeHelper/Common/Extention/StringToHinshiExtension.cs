﻿using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Extention
{
    public static class StringToHinshiExtension
    {
        public static Hinshi ToHinshi(this string partOfSpeech)
        {
            return partOfSpeech switch
            {
                "名詞" => Hinshi.名詞,
                "動詞" => Hinshi.動詞,
                "形容詞" => Hinshi.形容詞,
                "副詞" => Hinshi.副詞,
                "助詞" => Hinshi.助詞,
                "助動詞" => Hinshi.助動詞,
                "感動詞" => Hinshi.感動詞,
                "形状詞" => Hinshi.形状詞,
                "代名詞" => Hinshi.代名詞,
                "連体詞" => Hinshi.連体詞,
                "接尾辞" => Hinshi.接尾辞,
                "補助記号" => Hinshi.補助記号,
                _ => Hinshi.未定だ
            };
        }
    }
}