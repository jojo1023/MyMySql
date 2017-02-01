using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.ICustomWords
{
    public interface ICustomWord : IWord
    {
        CustomWordTypes CustomWordType { get; set; }

        string Alias { get; set; }
        
    }
}
