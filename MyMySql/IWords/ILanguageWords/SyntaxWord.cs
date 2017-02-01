using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords.ILanguageWords
{
    [DebuggerDisplay("SyntaxWordInput = {Input}")]
    public class SyntaxWord : ILanguageWord
    {
        public string Input { get; set; }

        public LanguageWordTypes LanguageWordType { get; set; }

        public WordTypes WordType { get; set; }

        public char StartingSyntax { get; set; }

        public List<char> CloseingSyntaxList { get; set; }

        public bool EndLineIsCloseingSyntax { get; set; }

        public string InputInsideSyntax { get; set; }

        List<IWord> wordsInsideSyntax = new List<IWord>();
        public List<IWord> WordsInsideSyntax
        {
            get
            {
                return wordsInsideSyntax;
            }
            set
            {
                wordsInsideSyntax = value;
                Children = value;
            }
        }

        public Func<string, CompilerInfo> FillWordsInsideSyntax { get; set; }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord,  IWord, ParseSyntaxInfo> ParseSyntax { get; set; }
        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> Children
        {
            get
            {
                return WordsInsideSyntax;
            }
            set
            {
                wordsInsideSyntax = value;
            }
        }
        public List<IWord> WordsThisCouldBe { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public Type VarType { get; set; }
        public SyntaxWord(char startingSyntax, List<char> closeingSyntaxList, bool endLineIsCloseingSyntax, Func<string, CompilerInfo> fillWordsInsideSyntax, AllWordTypes allWordType, Func<IWord, IWord, ParseSyntaxInfo> parseSyntax, List<List<WordRange>> childrenRanges)
        {
            Input = startingSyntax.ToString();
            StartingSyntax = startingSyntax;
            CloseingSyntaxList = closeingSyntaxList;
            EndLineIsCloseingSyntax = endLineIsCloseingSyntax;
            FillWordsInsideSyntax = fillWordsInsideSyntax;
            WordType = WordTypes.Language;
            LanguageWordType = LanguageWordTypes.Syntax;
            AllWordType = allWordType;
            ParseSyntax = parseSyntax;
            ChildrenRanges = childrenRanges;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
        }

        public SyntaxWord(string inputInsideSyntax, SyntaxWord dictionarySyntax, List<WordRange> rangesThatWorked)
        {
            Input = dictionarySyntax.Input;
            StartingSyntax = dictionarySyntax.StartingSyntax;
            CloseingSyntaxList = dictionarySyntax.CloseingSyntaxList;
            InputInsideSyntax = inputInsideSyntax;
            EndLineIsCloseingSyntax = dictionarySyntax.EndLineIsCloseingSyntax;
            WordType = WordTypes.Language;
            LanguageWordType = LanguageWordTypes.Syntax;
            AllWordType = dictionarySyntax.AllWordType;
            FillWordsInsideSyntax = dictionarySyntax.FillWordsInsideSyntax;
            ParseSyntax = dictionarySyntax.ParseSyntax;
            ChildrenRanges = new List<List<WordRange>>(dictionarySyntax.ChildrenRanges);
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = rangesThatWorked;
            VarType = null;
        }

        public CompilerInfo FindWordsInsideSyntax()
        {
            CompilerInfo info = FillWordsInsideSyntax.Invoke(InputInsideSyntax);
            if (info.Errors.Count == 0)
            {
                WordsInsideSyntax = info.Lexemes;
            }
            return info;
        }

    }
}
