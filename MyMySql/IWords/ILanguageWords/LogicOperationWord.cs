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

        public Func<IComparable, IComparable, IComparable> OperationFunction { get; set; }

        public AllWordTypes AllWordType { get; set; }

        public Func<IWord, IWord, ParseSyntaxInfo> ParseSyntax { get; set; }

        public List<List<WordRange>> ChildrenRanges { get; set; }
        public List<IWord> Children { get; set; }
        public List<IWord> WordsThisCouldBe { get; set; }
        public bool Initializing { get; set; }
        public List<WordRange> RangesThatWorked { get; set; }
        public List<Type> TypesThisOperationWorksWith { get; set; }

        IOperation leftChild = null;
        public IOperation LeftChild
        {
            get
            {
                if (Children[0] is IOperation)
                {
                    return (IOperation)Children[0];
                }
                else
                {
                    return leftChild;
                }
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
                if (Children[1] is IOperation)
                {
                    return (IOperation)Children[1];
                }
                else
                {
                    return rightChild;
                }
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
        public bool SetTypeToThis { get; set; }
        public LogicOperationWord(string input, int orderOfOperaionIndex, List<Type> typesThatThisOperationWorksWith, Func<IComparable, IComparable, IComparable> logicFunction, Func<IWord, IWord, ParseSyntaxInfo> parseSyntax, List<List<WordRange>> childrenRanges)
        {
            Input = input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            OperationFunction = logicFunction;
            AllWordType = AllWordTypes.LogicOperation;
            ParseSyntax = parseSyntax;
            ChildrenRanges = childrenRanges;
            Children = new List<IWord>() { null, null };
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = typeof(LogicOperationWord);
            UserInfo = false;
            OrderOfOperationIndex = orderOfOperaionIndex;
            TypesThisOperationWorksWith = typesThatThisOperationWorksWith;
            SetTypeToThis = true;
        }
        public LogicOperationWord(IOperation leftChild, IOperation rightChild, List<WordRange> rangesThatWorked, LogicOperationWord other)
        {
            Input = other.Input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            AllWordType = AllWordTypes.LogicOperation;
            OperationFunction = other.OperationFunction;
            ParseSyntax = other.ParseSyntax;
            ChildrenRanges = other.ChildrenRanges;
            Children = new List<IWord>() { leftChild, rightChild };
            this.leftChild = leftChild;
            this.rightChild = rightChild;
            RangesThatWorked = rangesThatWorked;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            VarType = typeof(LogicOperationWord);
            OrderOfOperationIndex = other.OrderOfOperationIndex;
            TypesThisOperationWorksWith = other.TypesThisOperationWorksWith;
            SetTypeToThis = true;
        }
        public LogicOperationWord(IWord unParsedLeftChild, IWord unParsedRightChild, List<WordRange> rangesThatWorked, LogicOperationWord other)
        {
            Input = other.Input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            AllWordType = AllWordTypes.LogicOperation;
            OperationFunction = other.OperationFunction;
            ParseSyntax = other.ParseSyntax;
            ChildrenRanges = other.ChildrenRanges;
            Children = new List<IWord>() { unParsedLeftChild, unParsedRightChild };
            RangesThatWorked = rangesThatWorked;
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            VarType = typeof(LogicOperationWord);
            this.unParsedLeftChild = unParsedLeftChild;
            this.unParsedRightChild = unParsedRightChild;
            OrderOfOperationIndex = other.OrderOfOperationIndex;
            TypesThisOperationWorksWith = other.TypesThisOperationWorksWith;
            SetTypeToThis = true;
        }
        public IComparable CheckOperation(SqlRow row)
        {
            IComparable leftOperationCheck = LeftChild.CheckOperation(row);
            IComparable rightOperationCheck = RightChild.CheckOperation(row);
            if (leftOperationCheck != null && rightOperationCheck != null)
            {
                return OperationFunction?.Invoke(leftOperationCheck, rightOperationCheck);
            }
            return null;
        }
    }
}
