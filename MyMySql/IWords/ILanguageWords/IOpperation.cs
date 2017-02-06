using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords.ILanguageWords
{
    public interface IOperation : IWord
    {
        IWord UnParsedLeftChild { get; set; }
        IWord UnParsedRightChild { get; set; }
        IOperation LeftChild { get; set; }
        IOperation RightChild { get; set; }
        int OrderOfOperationIndex { get; set; }
    }
}
