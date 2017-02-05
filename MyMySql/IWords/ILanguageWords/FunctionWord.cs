using MyMySql.ICustomWords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords.ILanguageWords
{
    public class FunctionWord : ILanguageWord
    {
        public AllWordTypes AllWordType { get; set; }

        public List<IWord> Children { get; set; }

        public List<List<WordRange>> ChildrenRanges { get; set; }

        public string Input { get; set; }

        public LanguageWordTypes LanguageWordType { get; set; }

        public Func<IWord, IWord, ParseSyntaxInfo> ParseSyntax { get; set; }

        public List<IWord> WordsThisCouldBe { get; set; }

        public WordTypes WordType { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public Type VarType { get; set; }
        public Func<FunctionWord, List<TableWord>,List<IWord>> BeforeCommandFunction { get; set; }
        public Func<FunctionWord, ICommandReturn, ICommandReturn> AfterCommandFunction { get; set; }
        public bool UserInfo { get; set; }
        public FunctionWord(string input, List<List<WordRange>> childrenRanges, Func<IWord, IWord, ParseSyntaxInfo> parseSyntax, Func<FunctionWord, List<TableWord>, List<IWord>> beforeCommandFunction, Func<FunctionWord, ICommandReturn, ICommandReturn> afterCommandFunction)
        {
            Input = input;
            ChildrenRanges = childrenRanges;
            Children = new List<IWord>();
            AllWordType = AllWordTypes.Function;
            LanguageWordType = LanguageWordTypes.Function;
            WordType = WordTypes.Language;
            WordsThisCouldBe = new List<IWord>();
            ParseSyntax = parseSyntax;
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
            BeforeCommandFunction = beforeCommandFunction;
            AfterCommandFunction = afterCommandFunction;
            UserInfo = true;
        }

        public FunctionWord(List<IWord> children, FunctionWord dictionaryFunctionWord, List<WordRange> rangesThatWorked)
        {
            Input = dictionaryFunctionWord.Input;
            ChildrenRanges = dictionaryFunctionWord.ChildrenRanges;
            Children = children;
            AllWordType = AllWordTypes.Function;
            LanguageWordType = LanguageWordTypes.Function;
            WordType = WordTypes.Language;
            WordsThisCouldBe = new List<IWord>();
            ParseSyntax = dictionaryFunctionWord.ParseSyntax;
            Initializing = false;
            RangesThatWorked = rangesThatWorked;
            VarType = null;
            BeforeCommandFunction = dictionaryFunctionWord.BeforeCommandFunction;
            AfterCommandFunction = dictionaryFunctionWord.AfterCommandFunction;
            UserInfo = true;
        }
    }
}
