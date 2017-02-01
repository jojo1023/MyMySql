using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public enum Opperations
    {
        Equal,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        NotEqual
    }

    public enum WordTypes
    {
        Custom,
        Language,
        CustomCustom,
        Lexical
    }

    public enum LanguageWordTypes
    {
        Keyword,
        LogicOpperation,
        MathOpperation,
        Function,
        Syntax
    }

    public enum CustomWordTypes
    {
        Table,
        Column
    }
    
    public enum SyntaxTypes
    {
        Comma,
        Quote,
        Parentheses,
        Temp
    }
    public enum AllWordTypes
    {
        Comma,
        Parentheses,
        LogicOperation,
        MathOperation,
        Table,
        Column,
        CustomCustom,
        Syntax,
        Keyword,
        Lexical,
        Function
    }
}
