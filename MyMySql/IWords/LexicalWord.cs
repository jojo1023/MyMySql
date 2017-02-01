using MyMySql.IWords.ILanguageWords;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace MyMySql.IWords
{
    [DebuggerDisplay("Input = {Input}")]
    public class LexicalWord : IWord
    {
        public string Input { get; set; }

        public WordTypes WordType { get; set; }
        public SyntaxWord SyntaxParent { get; set; }
        public LexicalWord DotChild { get; set; }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord, IWord, ParseSyntaxInfo> ParseSyntax { get; set; }
        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> Children { get; set; }
        public List<IWord> WordsThisCouldBe { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public Type VarType { get; set; }
        public LexicalWord (string input)
        {
            Input = input;
            WordType = WordTypes.Lexical;
            DotChild = null;
            AllWordType = AllWordTypes.Lexical;
            ParseSyntax = null;
            ChildrenRanges = new List<List<WordRange>>();
            Children = new List<IWord>();
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
        }
    }
}
