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
    public class LogicOperationWord : ILanguageWord, IOperation
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

        IOperation leftChild = null;
        public IOperation LeftChild
        {
            get
            {
                return leftChild;
            }
            set
            {
                leftChild = value;
                Children[0] = value;
            }
        }
        IOperation rightChild = null;
        public IOperation RightChild
        {
            get
            {
                return rightChild;
            }
            set
            {
                rightChild = value;
                Children[1] = value;
            }
        }
        public Type VarType { get; set; }
        public bool UserInfo { get; set; }

        IWord unParsedLeftChild = null;
        public IWord UnParsedLeftChild
        {
            get
            {
                return unParsedLeftChild;
            }
            set
            {
                unParsedLeftChild = value;
                Children[0] = value;
            }
        }
        IWord unParsedRightChild = null;
        public IWord UnParsedRightChild
        {
            get
            {
                return unParsedRightChild;
            }
            set
            {
                unParsedRightChild = value;
                Children[1] = value;
            }
        }
        public int OrderOfOperationIndex { get; set; }
        public LogicOperationWord(string input, int orderOfOperaionIndex, Func<IComparable, IComparable, bool> logicFunction, Func<IWord, IWord, ParseSyntaxInfo> parseSyntax, List<List<WordRange>> childrenRanges)
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
            OrderOfOperationIndex = orderOfOperaionIndex;
        }
        public LogicOperationWord(IOperation leftChild, IOperation rightChild, List<WordRange> rangesThatWorked, LogicOperationWord other)
        {
            Input = other.Input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            AllWordType = AllWordTypes.LogicOperation;
            LogicFunction = other.LogicFunction;
            ParseSyntax = other.ParseSyntax;
            ChildrenRanges = other.ChildrenRanges;
            Children = new List<IWord>() { leftChild, rightChild };
            this.leftChild = leftChild;
            this.rightChild = rightChild;
            RangesThatWorked = rangesThatWorked;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            VarType = null;
            OrderOfOperationIndex = other.OrderOfOperationIndex;
        }
        public LogicOperationWord(IWord unParsedLeftChild, IWord unParsedRightChild, List<WordRange> rangesThatWorked, LogicOperationWord other)
        {
            Input = other.Input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            AllWordType = AllWordTypes.LogicOperation;
            LogicFunction = other.LogicFunction;
            ParseSyntax = other.ParseSyntax;
            ChildrenRanges = other.ChildrenRanges;
            Children = new List<IWord>() { unParsedLeftChild, unParsedRightChild };
            RangesThatWorked = rangesThatWorked;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            VarType = null;
            this.unParsedLeftChild = unParsedLeftChild;
            this.unParsedRightChild = unParsedRightChild;
            OrderOfOperationIndex = other.OrderOfOperationIndex;
        }
    }
}
