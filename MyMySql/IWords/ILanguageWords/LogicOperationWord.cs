using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords.ILanguageWords
{
    [DebuggerDisplay("LogicOperationInput = {Input}")]
    public class LogicOperationWord : ILanguageWord
    {
        public string Input { get; set; }

        public LanguageWordTypes LanguageWordType { get; set; }

        public WordTypes WordType { get; set; }

        public Func<IComparable, IComparable, bool> LogicFunction { get; set; }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord, IWord, ParseSyntaxInfo> ParseSyntax { get; set; }

        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> Children { get; set; }
        public List<IWord> WordsThisCouldBe { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }

        IWord wordToLeft = null;
        public IWord WordToLeft
        {
            get
            {
                return wordToLeft;
            }
            set
            {
                wordToLeft = value;
                Children[0] = value;
            }
        }
        IWord wordToRight = null;
        public IWord WordToRight
        {
            get
            {
                return wordToRight;
            }
            set
            {
                wordToRight = value;
                Children[1] = value;
            }
        }
        public Type VarType { get; set; }
        public bool UserInfo { get; set; }
        public LogicOperationWord(string input, Func<IComparable, IComparable, bool> logicFunction, Func<IWord, IWord, ParseSyntaxInfo> parseSyntax, List<List<WordRange>> childrenRanges)
        {
            Input = input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            LogicFunction = logicFunction;
            AllWordType = AllWordTypes.LogicOperation;
            ParseSyntax = parseSyntax;
            ChildrenRanges = childrenRanges;
            Children = new List<IWord>() { null, null };
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
            UserInfo = false;
        }
        public LogicOperationWord(IWord wordToLeft, IWord wordToRight, List<WordRange> rangesThatWorked, LogicOperationWord other)
        {
            Input = other.Input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            AllWordType = AllWordTypes.LogicOperation;
            LogicFunction = other.LogicFunction;
            ParseSyntax = other.ParseSyntax;
            ChildrenRanges = other.ChildrenRanges;
            Children = new List<IWord>() { null, null };
            WordToLeft = wordToLeft;
            WordToRight = wordToRight;
            RangesThatWorked = rangesThatWorked;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            VarType = null;
        }
    }
}
