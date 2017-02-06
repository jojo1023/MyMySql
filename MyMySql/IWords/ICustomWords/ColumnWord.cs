using MyMySql.ICustomWords;
using MyMySql.IWords.ILanguageWords;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords.ICustomWords
{
    [DebuggerDisplay("ColumnInput = {Input}")]
    public class ColumnWord : ICustomWord, IOperation
    {
        public string Alias { get; set; }

        public CustomWordTypes CustomWordType { get; set; }

        public string Input { get; set; }

        public WordTypes WordType { get; set; }

        public SqlColumn ColumnDirectory { get; set; }

        public Table OwningTable { get; set; }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord,  IWord, ParseSyntaxInfo> ParseSyntax{get; set;}

        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> Children { get; set; }
        public List<IWord> WordsThisCouldBe { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public Type VarType { get; set; }
        public bool UserInfo { get; set; }

        public IOperation LeftChild { get; set; }

        public IOperation RightChild { get; set; }

        public IWord UnParsedLeftChild { get; set; }

        public IWord UnParsedRightChild { get; set; }
        public int OrderOfOperationIndex { get; set; }
        public ColumnWord(string input, SqlColumn columnDirectory, Table owningTasble, Func<IWord, IWord, ParseSyntaxInfo> parseSyntax, bool initializing)
        {
            Alias = input;
            Input = input;
            CustomWordType = CustomWordTypes.Column;
            WordType = WordTypes.Custom;
            ColumnDirectory = columnDirectory;
            OwningTable = owningTasble;
            AllWordType = AllWordTypes.Column;
            ParseSyntax = parseSyntax;
            ChildrenRanges = new List<List<WordRange>>();
            Children = new List<IWord>();
            WordsThisCouldBe = new List<IWord>();
            Initializing = initializing;
            RangesThatWorked = null;
            if(ColumnDirectory == null)
            {
                VarType = null;
            }
            else
            {
                VarType = ColumnDirectory.VarType;
            }
            UserInfo = true;
            LeftChild = null;
            RightChild = null;
            UnParsedLeftChild = null;
            UnParsedRightChild = null;
            OrderOfOperationIndex = 0;
        }
        public ColumnWord(ColumnWord other)
        {
            Alias = other.Input;
            Input = other.Input;
            CustomWordType = CustomWordTypes.Column;
            WordType = WordTypes.Custom;
            ColumnDirectory = other.ColumnDirectory;
            OwningTable = other.OwningTable;
            AllWordType = AllWordTypes.Column;
            ParseSyntax = other.ParseSyntax;
            ChildrenRanges = new List<List<WordRange>>();
            Children = new List<IWord>();
            WordsThisCouldBe = other.WordsThisCouldBe;
            Initializing = other.Initializing;
            RangesThatWorked = null;
            UserInfo = true;
            if (ColumnDirectory == null)
            {
                VarType = null;
            }
            else
            {
                VarType = ColumnDirectory.VarType;
            }
            LeftChild = null;
            RightChild = null;
            UnParsedLeftChild = null;
            UnParsedRightChild = null;
            OrderOfOperationIndex = 0;
        }
    }
}
