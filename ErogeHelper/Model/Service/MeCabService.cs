using ErogeHelper.Common.Entity;
using ErogeHelper.Common.Enum;
using ErogeHelper.Common.Extention;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using MeCab;
using MeCab.Extension.UniDic;
using System.Collections.Generic;
using System.IO;
using WanaKanaNet;

namespace ErogeHelper.Model.Service
{
    public class MeCabService : IMeCabService
    {
        public MeCabService(EhConfigRepository ehConfigRepository)
        {
            _ehConfigRepository = ehConfigRepository;

            _tagger = MeCabTagger.Create(new MeCabParam
            {
                DicDir = Path.Combine(_ehConfigRepository.AppDataDir, "dic")
            });
        }

        private readonly EhConfigRepository _ehConfigRepository;
        private MeCabTagger? _tagger;

        public void CreateTagger()
        {
            _tagger = MeCabTagger.Create(new MeCabParam
            {
                DicDir = Path.Combine(_ehConfigRepository.AppDataDir, "dic")
            });
        }

        public IEnumerable<MeCabWord> MeCabWordUniDicEnumerable(string sentence)
        {
            if (_tagger is null)
                yield break;

            foreach (var node in _tagger.ParseToNodes(sentence))
            {
                if (node.CharType <= 0)
                    continue;

                var mecabWord = new MeCabWord
                {
                    Word = node.Surface,
                    // 全角空格让假名渲染占位
                    Kana = " ",
                    PartOfSpeech = (node.GetPos1() ?? string.Empty).ToHinshi()
                };

                // kana
                if ((node.GetGoshu() ?? string.Empty).Equals("外"))
                {
                    mecabWord.Kana = (node.GetLemma() ?? " ").Split('-')[^1];
                }
                else if (_ehConfigRepository.Romaji)
                {
                    mecabWord.Kana = WanaKana.ToRomaji(node.GetPron() ?? " ");
                }
                else if (!WanaKana.IsKana(node.Surface) &&
                         mecabWord.PartOfSpeech != Hinshi.補助記号)
                {
                    mecabWord.Kana = _ehConfigRepository.Hiragana
                        ? WanaKana.ToHiragana(node.GetPron() ?? " ")
                        : node.GetPron() ?? " "; // Katakana by default
                }

                yield return mecabWord;
            }
        }
    }
}