using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.TableStuff
{
    public class EmptyICompareable : IComparable
    {
        public int CompareTo(object obj)
        {
            return 0;
        }
    }
}
