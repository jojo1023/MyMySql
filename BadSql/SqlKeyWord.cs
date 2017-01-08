using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    [DebuggerDisplay("Input = {Input}")]
    public class SqlKeyWord : ISqlInput
    {
        public string Input { get; set; }
        public SqlKeyWord Parent { get; set; }
        public List<ISqlInput> Children { get; set; }
        public Range ChildrenAmountRange { get; set; }
        public KeyWordTypes KeyWordType { get; set; }
        public SqlKeyWord(string keyWord, SqlKeyWord parent, Range childrenAmountRange, KeyWordTypes keyWordType)
        {
            Input = keyWord;
            Children = new List<ISqlInput>();
            Parent = parent;
            ChildrenAmountRange = childrenAmountRange;
            KeyWordType = keyWordType;
        }
    }
    public enum KeyWordTypes
    {
        Command,
        CommaGroup,
        ParenthesesGroup
    }
}
