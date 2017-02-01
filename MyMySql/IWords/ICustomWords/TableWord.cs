using MyMySql.IWords;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.ICustomWords
{
    [DebuggerDisplay("TableInput = {Input}")]
    public class TableWord : ICustomWord
    {

        public string Input { get; set; }

        public string Alias { get; set; }

        public CustomWordTypes CustomWordType { get; set; }

        public WordTypes WordType { get; set; }

        public Table TableDirectory { get; set; }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord,  IWord, ParseSyntaxInfo> ParseSyntax { get; set; }
        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> Children { get; set; }
        public List<IWord> WordsThisCouldBe { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public Type VarType { get; set; }
        public TableWord(string input, string alias, Table tableDirectory, Func<IWord, IWord, ParseSyntaxInfo> parseSyntax, bool initializing)
        {
            Input = input;
            Alias = alias;
            CustomWordType = CustomWordTypes.Table;
            WordType = WordTypes.Custom;
            TableDirectory = tableDirectory;
            AllWordType = AllWordTypes.Table;
            ParseSyntax = parseSyntax;
            ChildrenRanges = new List<List<WordRange>>();
            Children = new List<IWord>();
            WordsThisCouldBe = new List<IWord>();
            Initializing = initializing;
            RangesThatWorked = null;
            VarType = null;
        }

    }
}
