using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.TableStuff
{
    public class NullICompareable : IComparable
    {
        public int CompareTo(object obj)
        {
            return 2;
        }
        public override string ToString()
        {
            return "NULL";
        }
    }
}
