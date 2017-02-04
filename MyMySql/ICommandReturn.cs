using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public interface ICommandReturn
    {
        Table ReturnTable { get; set; }
        RenderString ReturnString { get; set; }
        string ToString();
    }
}
