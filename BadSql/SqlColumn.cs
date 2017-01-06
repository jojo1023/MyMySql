using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    public class SqlColumn
    {
        public Type VarType { get; set; }
        public string Name { get; set; }

        public SqlColumn(string name, Type type)
        {
            Name = name;
            VarType = type;
        }
    }
}
