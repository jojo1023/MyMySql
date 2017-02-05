using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords.ILanguageWords
{
    [DebuggerDisplay("MathOperationInput = {Input}")]
    public class MathOperationWord : ILanguageWord
    {
        public string Input { get; set; }

        public LanguageWordTypes LanguageWordType { get; set; }

        public WordTypes WordType { get; set; }

        public Func<IComparable, IComparable, IComparable> MathFunction { get; set; }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord, IWord, ParseSyntaxInfo> ParseSyntax { get; set; }
        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> Children { get; set; }
        public List<IWord> WordsThisCouldBe { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public Type VarType { get; set; }
        public bool UserInfo { get; set; }
        public MathOperationWord(string input, Func<IComparable, IComparable, IComparable> mathFunction, Func<IWord,  IWord, ParseSyntaxInfo> parseSyntax)
        {
            Input = input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            MathFunction = mathFunction;
            AllWordType = AllWordTypes.MathOperation;
            ParseSyntax = parseSyntax;
            ChildrenRanges = new List<List<WordRange>>();
            Children = new List<IWord>();
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
            UserInfo = false;
        }
    }
}
