using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords.ILanguageWords
{
    [DebuggerDisplay("Input = {Input}")]
    public class Keyword : ILanguageWord
    {
        public string Input { get; set; }

        public LanguageWordTypes LanguageWordType { get; set; }

        public WordTypes WordType { get; set; }

        List<IWord> keywordParameters = new List<IWord>();
        public List<IWord> KeywordParameters
        {
            get
            {
                return keywordParameters;
            }
            set
            {
                keywordParameters = value;
                Children = value;
            }
        }

        List<List<WordRange>> ranges = new List<List<WordRange>>();
        public List<List<WordRange>> Ranges
        {
            get
            {
                return ranges;
            }
            set
            {
                ranges = value;
                ChildrenRanges = value;
            }
        }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord, IWord, ParseSyntaxInfo> ParseSyntax { get; set; }
        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> WordsThisCouldBe { get; set; }
        public List<IWord> Children
        {
            get
            {
                return KeywordParameters;
            }
            set
            {
                keywordParameters = value;
            }
        }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public Type VarType { get; set; }
        public bool UserInfo { get; set; }
        public Keyword(string input, List<List<WordRange>> ranges)
        {
            Input = input;
            LanguageWordType = LanguageWordTypes.Keyword;
            WordType = WordTypes.Language;
            KeywordParameters = new List<IWord>();
            Ranges = ranges;
            AllWordType = AllWordTypes.Keyword;
            ParseSyntax = null;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
            UserInfo = false;
        }

        public Keyword(Keyword other, List<IWord> keywordParameters, List<WordRange> rangesThatWorked)
        {
            Input = other.Input;
            LanguageWordType = LanguageWordTypes.Keyword;
            WordType = WordTypes.Language;
            KeywordParameters = keywordParameters;
            Ranges = other.Ranges;
            AllWordType = AllWordTypes.Keyword;
            ParseSyntax = null;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = rangesThatWorked;
            VarType = null;
            UserInfo = false;
        }
    }

}
