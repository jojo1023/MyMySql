using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public interface ILanguageWord : IWord
    {
        LanguageWordTypes LanguageWordType { get; set; }
    }
}
