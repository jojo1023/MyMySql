using MyMySql.IWords;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public interface IWord
    {
        string Input { get; set; }
        WordTypes WordType { get; set; }
        AllWordTypes AllWordType { get; set; }
        Func<IWord, IWord, ParseSyntaxInfo> ParseSyntax { get; set; }
        List<List<WordRange>> ChildrenRanges { get; set; }
        List<WordRange> RangesThatWorked { get; set; }
        List<IWord> Children { get; set; }
        List<IWord> WordsThisCouldBe { get; set; }
        bool Initializing { get; set; }
        Type VarType { get; set; }
        bool UserInfo { get; set; }
    }
    public struct ParseSyntaxInfo
    {
        public IWord Word;
        public List<string> Errors;
    }
}
