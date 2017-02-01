using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public class SqlColumn
    {
        public Type VarType { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Constructor creates a SqlColumn with a name and a type
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="type">The type of objects in the column</param>
        public SqlColumn(string name, Type type)
        {
            Name = name;
            VarType = type;
        }
    }
}
