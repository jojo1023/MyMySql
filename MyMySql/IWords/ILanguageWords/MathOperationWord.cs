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
    public class MathOperationWord : ILanguageWord, IOperation
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
        public Type VarType { get; set; }
        public bool UserInfo { get; set; }

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
        public List<Type> TypesThisOperationWorksWith { get; set; }
        public bool SetTypeToThis { get; set; }
        public MathOperationWord(string input, int orderOfOpperationIndex, List<Type> typesThisOperationWorksWith, List<List<WordRange>> childrenRanges, Func<IComparable, IComparable, IComparable> mathFunction, Func<IWord,  IWord, ParseSyntaxInfo> parseSyntax)
        {
            Input = input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            OperationFunction = mathFunction;
            AllWordType = AllWordTypes.MathOperation;
            ParseSyntax = parseSyntax;
            ChildrenRanges = childrenRanges;
            Children = new List<IWord>() { null, null };
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
            UserInfo = false;
            LeftChild = null;
            RightChild = null;
            UnParsedLeftChild = null;
            UnParsedRightChild = null;
            OrderOfOperationIndex = orderOfOpperationIndex;
            TypesThisOperationWorksWith = typesThisOperationWorksWith;
            SetTypeToThis = false;
        }
        public MathOperationWord(IOperation leftChild, IOperation rightChild, MathOperationWord dictionaryMathOperation)
        {
            Input = dictionaryMathOperation.Input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            OperationFunction = dictionaryMathOperation.OperationFunction;
            AllWordType = AllWordTypes.MathOperation;
            ParseSyntax = dictionaryMathOperation.ParseSyntax;
            ChildrenRanges = new List<List<WordRange>>();
            Children = new List<IWord>() { leftChild, rightChild };
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
            UserInfo = false;
            this.leftChild = leftChild;
            this.rightChild = rightChild;
            OrderOfOperationIndex = dictionaryMathOperation.OrderOfOperationIndex;
            TypesThisOperationWorksWith = dictionaryMathOperation.TypesThisOperationWorksWith;
            SetTypeToThis = false;
        }
        public MathOperationWord(IWord unParsedLeftChild, IWord unParsedRightChild, MathOperationWord dictionaryMathOperation)
        {
            Input = dictionaryMathOperation.Input;
            LanguageWordType = LanguageWordTypes.LogicOpperation;
            WordType = WordTypes.Language;
            OperationFunction = dictionaryMathOperation.OperationFunction;
            AllWordType = AllWordTypes.MathOperation;
            ParseSyntax = dictionaryMathOperation.ParseSyntax;
            ChildrenRanges = new List<List<WordRange>>();
            Children = new List<IWord>() { unParsedLeftChild, unParsedRightChild };
            WordsThisCouldBe = new List<IWord>();
            Initializing = false;
            RangesThatWorked = null;
            VarType = null;
            UserInfo = false;
            this.unParsedLeftChild = unParsedLeftChild;
            this.unParsedRightChild = unParsedRightChild;
            OrderOfOperationIndex = dictionaryMathOperation.OrderOfOperationIndex;
            TypesThisOperationWorksWith = dictionaryMathOperation.TypesThisOperationWorksWith;
            SetTypeToThis = false;
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
