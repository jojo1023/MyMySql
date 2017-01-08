using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    public interface ISqlInput
    {
        string Input { get; set; }
        SqlKeyWord Parent { get; set; }
    }
}
