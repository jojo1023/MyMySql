using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BadSql
{
    class Program
    {
        static string xmlFile = "SqlData.xml";
        static XDocument xdoc = XDocument.Load(xmlFile);
        static Dictionary<string, Table> tables = new Dictionary<string, Table>();
        static Dictionary<string, Range> keywords = new Dictionary<string, Range>();

        public static void Main(string[] args)
        {
            keywords.Add("select", new Range(0, int.MaxValue, false, true));
            keywords.Add("from", new Range(1, 2, false, false));
            keywords.Add("where", new Range(3, 4, false, false));
            keywords.Add("insert", new Range(0, 2, false, false));
            keywords.Add("into", new Range(1, 2, false, false));
            keywords.Add("values", new Range(1, int.MaxValue, true, true));
            keywords.Add("create", new Range(0, 1, false, false));
            keywords.Add("table", new Range(1, 3, true, false));
            keywords.Add("delete", new Range(0, 1, false, false));
            keywords.Add("sort", new Range(1, 2, false, false));
            keywords.Add("drop", new Range(0, 1, false, false));
            keywords.Add("update", new Range(1, 2, false, false));
            keywords.Add("set", new Range(1, int.MaxValue, false, true));

            LoadXML();
            Random random = new Random();
            List<SqlKeyWord> input;

            SqlColumn whereCollum;
            Opperations whereOpperation;
            IComparable whereValue;
            bool hasWhere;
            Table table;
            string originalInput;
            string output = "";

            do
            {
                string errors;
                originalInput = Console.ReadLine();
                input = Split2(originalInput, out errors);
                while (input.Count > 0 && errors == "")
                {
                    switch (input[0].Input.ToLower())
                    {
                        #region Select
                        case ("select"):
                            if (input.Count >= 2)
                            {
                                List<SqlColumn> collums = new List<SqlColumn>();
                                whereCollum = new SqlColumn("", typeof(int));
                                whereOpperation = Opperations.Equal;
                                whereValue = null;
                                hasWhere = false;
                                table = null;
                                SqlKeyWord select = input[0];
                                SqlKeyWord from = input[1];
                                if (from.Input.ToLower() != "from")
                                {
                                    errors += "Incorect Use Of Select, ";
                                    break;
                                }
                                if (select.Children.Count == 1 && isSqlKeyWord(KeyWordTypes.CommaGroup, select.Children[0]) && ((SqlKeyWord)select.Children[0]).Children.Count == 1 && ((SqlKeyWord)select.Children[0]).Children[0].Input == "*" && tables.ContainsKey(from.Children[0].Input))
                                {
                                    table = tables[from.Children[0].Input];
                                    collums = table.SqlColumns;
                                }
                                else if (tables.ContainsKey(from.Children[0].Input))

                                {
                                    table = tables[from.Children[0].Input];
                                    bool parseingError = false;
                                    for (int i = 0; i < select.Children.Count; i++)
                                    {
                                        if (isSqlKeyWord(KeyWordTypes.CommaGroup, select.Children[i]))
                                        {
                                            SqlKeyWord commaGroup = (SqlKeyWord)select.Children[i];
                                            if (commaGroup.Children.Count == 1)
                                            {
                                                SqlColumn possibleCollumn = table[commaGroup.Children[0].Input];
                                                if (possibleCollumn != null)
                                                {
                                                    collums.Add(possibleCollumn);
                                                }
                                                else
                                                {
                                                    errors += table.Name + " Table Doesn't Have the Collumn: " + commaGroup.Children[0].Input + ", ";
                                                    parseingError = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                errors += "Incorect Collumn Syntax, ";
                                                parseingError = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            errors += "Collumn Parseing Error, ";
                                            parseingError = true;
                                            break;
                                        }
                                    }
                                    if (parseingError)
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    errors += "Table Doesn't Exist, ";
                                    break;
                                }
                                if (input.Count >= 3 && input[2].Input.ToLower() == "where")
                                {
                                    SqlKeyWord whereKeyWord = input[2];
                                    hasWhere = GetWhereInfo(whereKeyWord, table, out whereCollum, out whereOpperation, out whereValue);
                                    if (!hasWhere)
                                    {
                                        errors += "Incorect Where Syntax, ";
                                        break;
                                    }
                                }

                                List<SqlRow> rows = table.Select(hasWhere, whereCollum.Name, whereOpperation, whereValue);
                                input.RemoveAt(0);
                                input.RemoveAt(0);
                                if (hasWhere)
                                {
                                    input.RemoveAt(0);
                                }

                                output = DisplayTable(table, rows, collums);
                            }
                            break;
                        #endregion

                        #region Insert
                        case ("insert"):
                            if (input.Count >= 2)
                            {
                                SqlKeyWord Values = null;
                                List<SqlKeyWord> ValueSets = new List<SqlKeyWord>();
                                table = null;
                                bool hasInto = false;
                                if (input.Count >= 3 && input[1].Input.ToLower() == "into" && input[2].Input.ToLower() == "values")
                                {
                                    if (tables.ContainsKey(input[1].Children[0].Input))
                                    {
                                        table = tables[input[1].Children[0].Input];
                                        Values = input[2];
                                        hasInto = true;
                                    }
                                    else
                                    {
                                        errors += "Table Doesn't Exist, ";
                                        break;
                                    }
                                }
                                else if (input[1].Input.ToLower() == "values" && input[0].Children.Count == 1)
                                {
                                    if (tables.ContainsKey(input[0].Children[0].Input))
                                    {
                                        table = tables[input[0].Children[0].Input];
                                        Values = input[1];
                                    }
                                    else
                                    {
                                        errors += "Table Doesn't Exist, ";
                                        break;
                                    }
                                }
                                else
                                {
                                    errors += "Incorect Syntax Near Insert, ";
                                    break;
                                }
                                bool correctSyntax = true;
                                for (int i = 0; i < Values.Children.Count; i++)
                                {
                                    if (isSqlKeyWord(KeyWordTypes.CommaGroup, Values.Children[i]) && ((SqlKeyWord)Values.Children[i]).Children.Count == 1 && isSqlKeyWord(KeyWordTypes.ParenthesesGroup, ((SqlKeyWord)Values.Children[i]).Children[0]))
                                    {
                                        ValueSets.Add(((SqlKeyWord)((SqlKeyWord)Values.Children[i]).Children[0]));
                                    }
                                    else
                                    {
                                        errors += "Incorect Syntax Near Values, ";
                                        break;
                                    }
                                }
                                if (!correctSyntax)
                                {
                                    break;
                                }

                                List<List<IComparable>> values = new List<List<IComparable>>();
                                for (int i = 0; i < ValueSets.Count; i++)
                                {
                                    bool valueSetsOk = true;
                                    if (ValueSets[i].Children.Count != table.SqlColumns.Count)
                                    {
                                        errors += "Incorect Syntax Near Values, ";
                                    }
                                    else
                                    {
                                        bool valueSetOk = true;
                                        List<IComparable> currentValueSet = new List<IComparable>();
                                        for (int j = 0; j < ValueSets[i].Children.Count; j++)
                                        {
                                            if (isSqlKeyWord(KeyWordTypes.CommaGroup, ValueSets[i].Children[j]))
                                            {
                                                SqlKeyWord CommaGroup = (SqlKeyWord)ValueSets[i].Children[j];
                                                if (CommaGroup.Children.Count == 1)
                                                {
                                                    SqlColumn currentCollumn = table.SqlColumns[j];
                                                    object compareValue = null;

                                                    try
                                                    {
                                                        compareValue = ((IConvertible)CommaGroup.Children[0].Input).ToType(currentCollumn.VarType, System.Globalization.CultureInfo.InvariantCulture);
                                                    }
                                                    catch
                                                    {
                                                        errors += "Incorect Type Of Value Being Inserted, ";
                                                        valueSetOk = false;
                                                        break;
                                                    }
                                                    if (compareValue is IComparable)
                                                    {
                                                        currentValueSet.Add((IComparable)compareValue);
                                                    }
                                                    else
                                                    {
                                                        errors += "Incorect Type Of Value Being Inserted, ";
                                                        valueSetOk = false;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    errors += "Incorect Syntax Near Value, ";
                                                    valueSetOk = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                errors += "Values Parseing Error, ";
                                                valueSetOk = false;
                                                break;
                                            }
                                        }
                                        if (valueSetOk)
                                        {
                                            values.Add(currentValueSet);
                                        }
                                        else
                                        {
                                            valueSetsOk = false;
                                            break;
                                        }
                                    }
                                    if (!valueSetsOk)
                                    {
                                        break;
                                    }
                                }
                                for (int i = 0; i < values.Count; i++)
                                {
                                    table.AddRow(values[i].ToArray());
                                }
                                output = values.Count.ToString() + " Row(s) Inserted";

                                input.RemoveAt(0);
                                input.RemoveAt(0);
                                if (hasInto)
                                {
                                    input.RemoveAt(0);
                                }
                            }
                            break;
                        #endregion

                        #region CreateTable
                        case ("create"):
                            if (input.Count >= 2 && input[1].Input.ToLower() == "table" && input[1].Children.Count == 2)
                            {
                                if (input[1].Children[0].GetType() == typeof(SqlCustomInput) && isSqlKeyWord(KeyWordTypes.ParenthesesGroup, input[1].Children[1]))
                                {

                                    if (!((SqlCustomInput)input[1].Children[0]).InQuotes)
                                    {
                                        if (!tables.ContainsKey(input[1].Children[0].Input))
                                        {
                                            SqlKeyWord ParenGroup = (SqlKeyWord)input[1].Children[1];
                                            string tableName = input[1].Children[0].Input;
                                            List<SqlColumn> newCollumns = new List<SqlColumn>();
                                            List<string> collumnNames = new List<string>();
                                            bool goodSyntax = true;
                                            if (ParenGroup.Children.Count > 0)
                                            {
                                                for (int i = 0; i < ParenGroup.Children.Count; i++)
                                                {
                                                    if (isSqlKeyWord(KeyWordTypes.CommaGroup, ParenGroup.Children[i]) && ((SqlKeyWord)ParenGroup.Children[i]).Children.Count == 2)
                                                    {
                                                        SqlKeyWord CommaGroup = (SqlKeyWord)ParenGroup.Children[i];
                                                        Type collumnType = Type.GetType(CommaGroup.Children[1].Input);
                                                        if (collumnType != null)
                                                        {
                                                            if (!isInQuotes(CommaGroup.Children[0]))
                                                            {
                                                                if (!collumnNames.Contains(CommaGroup.Children[0].Input))
                                                                {
                                                                    collumnNames.Add(CommaGroup.Children[0].Input);
                                                                    newCollumns.Add(new SqlColumn(CommaGroup.Children[0].Input, collumnType));
                                                                }
                                                                else
                                                                {
                                                                    errors += "Collumn Already Exits In Table, ";
                                                                    goodSyntax = false;
                                                                    break;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                errors += "Incorect Syntax Near Table, ";
                                                                goodSyntax = false;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            errors += CommaGroup.Children[1].Input + " is Not a Type, ";
                                                            goodSyntax = false;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        errors += "Incorect Collumn Syntax, ";
                                                        goodSyntax = false;
                                                        break;
                                                    }
                                                }
                                                if (!goodSyntax)
                                                {
                                                    break;
                                                }
                                                tables.Add(tableName, new Table(tableName, newCollumns.ToArray()));
                                                output = "Table Added";
                                                input.RemoveAt(0);
                                                input.RemoveAt(0);
                                            }
                                            else
                                            {
                                                errors += "Table Must Have One Or More Collumns, ";
                                            }
                                        }
                                        else
                                        {
                                            errors += "Incorrect Syntax Near Table, ";
                                        }
                                    }
                                    else
                                    {
                                        errors += "Table Already Exists, ";
                                    }
                                }
                                else
                                {
                                    errors += "Incorect Syntax Near Create, ";
                                }
                            }
                            else
                            {
                                errors += "Incorect Syntax Near Create, ";
                            }
                            break;
                        #endregion

                        #region Delete
                        case ("delete"):
                            whereCollum = new SqlColumn("", typeof(int));
                            whereOpperation = Opperations.Equal;
                            whereValue = null;
                            hasWhere = false;
                            if (input.Count >= 2 && input[1].Input.ToLower() == "from")
                            {
                                if (tables.ContainsKey(input[1].Children[0].Input))
                                {
                                    table = tables[input[1].Children[0].Input];

                                    if (input.Count >= 3 && input[2].Input.ToLower() == "where")
                                    {
                                        if (isSqlKeyWord(KeyWordTypes.Command, input[2]))
                                        {
                                            if (!GetWhereInfo((SqlKeyWord)input[2], table, out whereCollum, out whereOpperation, out whereValue))
                                            {
                                                errors += "Incorect Syntax Near Where, ";
                                                break;
                                            }
                                            hasWhere = true;
                                        }
                                        else
                                        {
                                            errors += "Incorect Syntax Near Where, ";
                                            break;
                                        }
                                    }

                                    int amountOfRows = table.Delete(hasWhere, whereCollum.Name, whereOpperation, whereValue);

                                    output = amountOfRows.ToString() + " Rows " + "Affected";
                                    input.RemoveAt(0);
                                    input.RemoveAt(0);
                                    if (hasWhere)
                                    {
                                        input.RemoveAt(0);
                                    }
                                }
                                else
                                {
                                    errors += "Table Doesn't Exist, ";
                                }
                            }
                            else
                            {
                                errors += "Incorrect Use of Delete, ";
                            }
                            break;
                        #endregion

                        #region Sort
                        case ("sort"):
                            if (tables.ContainsKey(input[0].Children[0].Input))
                            {
                                table = tables[input[0].Children[0].Input];
                                table.Sort();
                                output = "Table Sorted";

                                input.RemoveAt(0);
                            }
                            else
                            {
                                errors += "Table Doesn't Exist, ";
                            }
                            break;
                        #endregion

                        #region Drop Table
                        case ("drop"):
                            if (input.Count >= 2)
                            {
                                if (input[1].Input.ToLower() == "table" && input[1].Children.Count == 1)
                                {
                                    if (tables.ContainsKey(input[1].Children[0].Input))
                                    {
                                        tables.Remove(input[1].Children[0].Input);
                                        output = "1 Table Droped";
                                        input.RemoveAt(0);
                                        input.RemoveAt(0);
                                    }
                                    else
                                    {
                                        errors += "Table Doesn't Exist, ";
                                    }
                                }
                                else
                                {
                                    errors += "Incorrect Syntax Near Table, ";
                                }
                            }
                            break;
                        #endregion

                        #region Update
                        case ("update"):
                            whereCollum = new SqlColumn("", typeof(int));
                            whereOpperation = Opperations.Equal;
                            whereValue = null;
                            hasWhere = false;
                            if (input.Count >= 2 && input[1].Input.ToLower() == "set")
                            {
                                if (tables.ContainsKey(input[0].Children[0].Input))
                                {
                                    table = tables[input[0].Children[0].Input];
                                    SqlKeyWord setKeyWord = input[1];
                                    List<SqlKeyWord> CommaGroups = new List<SqlKeyWord>();
                                    bool syntaxGood = true;
                                    for (int i = 0; i < setKeyWord.Children.Count; i++)
                                    {
                                        if (isSqlKeyWord(KeyWordTypes.CommaGroup, setKeyWord.Children[i]) && ((SqlKeyWord)setKeyWord.Children[i]).Children.Count == 3)
                                        {
                                            CommaGroups.Add((SqlKeyWord)setKeyWord.Children[i]);
                                        }
                                        else
                                        {
                                            errors += "Incorrect Syntax Near Set, ";
                                            syntaxGood = false;
                                            break;
                                        }
                                    }
                                    if (!syntaxGood)
                                    {
                                        break;
                                    }
                                    List<ColumnValuePair> setPairs = new List<ColumnValuePair>();
                                    for (int i = 0; i < CommaGroups.Count; i++)
                                    {
                                        SqlColumn collumnToSet = table[CommaGroups[i].Children[0].Input];
                                        if (collumnToSet != null)
                                        {
                                            if (CommaGroups[i].Children[1].Input == "=")
                                            {
                                                object valueObject = null;
                                                try
                                                {
                                                    valueObject = ((IConvertible)CommaGroups[i].Children[2].Input).ToType(collumnToSet.VarType, System.Globalization.CultureInfo.InvariantCulture);
                                                }
                                                catch
                                                {
                                                    errors += "Value is Not the Same Type as Collumn, ";
                                                    syntaxGood = false;
                                                    break;
                                                }
                                                if (valueObject is IComparable)
                                                {
                                                    IComparable value = (IComparable)valueObject;
                                                    setPairs.Add(new ColumnValuePair(collumnToSet, value));
                                                }
                                            }
                                            else
                                            {
                                                errors += "Incorect Syntax Near Set, ";
                                                syntaxGood = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            errors += "Collumn: " + CommaGroups[i].Children[0].Input + " Doesn't Exist, ";
                                            syntaxGood = false;
                                            break;
                                        }
                                    }
                                    if (!syntaxGood)
                                    {
                                        break;
                                    }

                                    if (input.Count >= 3 && input[2].Input.ToLower() == "where")
                                    {
                                        hasWhere = true;
                                        if (!GetWhereInfo(input[2], table, out whereCollum, out whereOpperation, out whereValue))
                                        {
                                            errors += "Incorect Syntax Near Where, ";
                                            break;
                                        }
                                    }

                                    int amountOfUpdatedRows = table.Update(hasWhere, setPairs, whereCollum.Name, whereOpperation, whereValue, out errors);

                                    if (errors == "")
                                    {
                                        output = amountOfUpdatedRows + " Rows Updated";

                                        input.RemoveAt(0);
                                        input.RemoveAt(0);
                                        if (hasWhere)
                                        {
                                            input.RemoveAt(0);
                                        }
                                    }
                                }
                                else
                                {
                                    errors += "Table Doesn't Exist, ";
                                }
                            }
                            else
                            {
                                errors += "Incorect Syntax Near Update, ";
                            }
                            break;
                        #endregion

                        #region Default
                        default:
                            errors += input[0].Input + " is Not a Starting Command, ";
                            break;
                            #endregion
                    }
                    Console.WriteLine(output);
                    output = "";
                }
                if (errors.Length > 2)
                {
                    errors = errors.Remove(errors.Length - 2, 2);
                    output = errors;
                    Console.WriteLine(output);
                }
            } while (originalInput.ToLower() != "exit");
            SaveXML();
        }

        public static bool GetWhereInfo(SqlKeyWord WhereKeyWord, Table table, out SqlColumn whereCollum, out Opperations whereOpperation, out IComparable whereValue)
        {
            whereCollum = null;
            whereOpperation = Opperations.Equal;
            whereValue = null;
            bool whereWorks = false;
            if (WhereKeyWord.Input.ToLower() == "where" && WhereKeyWord.Children.Count >= 3)
            {
                SqlColumn tempCollumn = table[WhereKeyWord.Children[0].Input];

                if (tempCollumn != null)
                {
                    whereCollum = tempCollumn;
                    bool hasOpperation = true;
                    switch (WhereKeyWord.Children[1].Input)
                    {
                        case ("="):
                            whereOpperation = Opperations.Equal;
                            break;
                        case (">"):
                            whereOpperation = Opperations.GreaterThan;
                            break;
                        case ("<"):
                            whereOpperation = Opperations.LessThan;
                            break;
                        case (">="):
                            whereOpperation = Opperations.GreaterThanOrEqual;
                            break;
                        case ("<="):
                            whereOpperation = Opperations.LessThanOrEqual;
                            break;
                        case ("!="):
                            whereOpperation = Opperations.NotEqual;
                            break;
                        default:
                            hasOpperation = false;
                            break;
                    }
                    if (hasOpperation)
                    {
                        object compareValue;

                        try
                        {
                            compareValue = ((IConvertible)WhereKeyWord.Children[2].Input).ToType(whereCollum.VarType, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            compareValue = null;
                        }
                        if (compareValue != null && compareValue is IComparable)
                        {
                            whereValue = (IComparable)compareValue;
                            whereWorks = true;
                        }
                    }
                }
            }

            return whereWorks;
        }

        public static bool isSqlKeyWord(KeyWordTypes keyWordType, ISqlInput possibleKeyWord)
        {
            return possibleKeyWord.GetType() == typeof(SqlKeyWord) && keyWordType == ((SqlKeyWord)possibleKeyWord).KeyWordType;
        }

        public static bool isInQuotes(ISqlInput possibleInQuotes)
        {
            if (possibleInQuotes.GetType() == typeof(SqlCustomInput))
            {
                return ((SqlCustomInput)possibleInQuotes).InQuotes;
            }
            return true;
        }

        public static List<SqlKeyWord> Split2(string input, out string errors)
        {
            List<SqlKeyWord> returnList = new List<SqlKeyWord>();
            errors = "";
            string currentWord = "";
            int amountOfParentheses = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ' || input[i] == '(')
                {
                    if (input[i] == '(')
                    {
                        amountOfParentheses++;
                    }
                    if (keywords.ContainsKey(currentWord.ToLower()))
                    {
                        SqlKeyWord keyWord = new SqlKeyWord(currentWord, null, keywords[currentWord.ToLower()], KeyWordTypes.Command);
                        SqlKeyWord nextKeyWord;
                        keyWord.Children = GetKeyWordChildren2(keyWord, input, i, amountOfParentheses, new List<ISqlInput>(), out amountOfParentheses, out i, out errors, out nextKeyWord);
                        returnList.Add(keyWord);
                        returnList = GetBottomKeyWords2(nextKeyWord, input, i, amountOfParentheses, returnList, errors, out i, out amountOfParentheses, out errors);
                        break;
                    }
                    else
                    {
                        errors += "Incorrect Use of" + currentWord + ", ";
                        break;
                    }
                }
                else if (input[i] == '"')
                {
                    errors += "Incorrect Quote Syntax, ";
                    break;
                }
                else if (input[i] == ')')
                {
                    errors += "Incorrect Parentheses Syntax, ";
                    break;
                }
                else if (input[i] == ',')
                {
                    errors += "Incorrect Comma Syntax, ";
                    break;
                }
                else
                {
                    currentWord += input[i];
                }
            }

            if (amountOfParentheses != 0)
            {
                errors += "Incorrect Parentheses Syntax, ";
            }

            return returnList;
        }

        public static List<SqlKeyWord> GetBottomKeyWords2(SqlKeyWord nextKeyWord, string input, int currentIndex, int amountOfParenthesesIn, List<SqlKeyWord> currentList, string errorsIn, out int newIndex, out int amountOfParentheses, out string errors)
        {
            newIndex = currentIndex;
            amountOfParentheses = amountOfParenthesesIn;
            errors = errorsIn;
            if (nextKeyWord != null)
            {
                SqlKeyWord newNextKeyWord;
                nextKeyWord.Children = GetKeyWordChildren2(nextKeyWord, input, currentIndex, amountOfParenthesesIn, new List<ISqlInput>(), out amountOfParentheses, out newIndex, out errors, out newNextKeyWord);
                currentList.Add(nextKeyWord);

                if (newNextKeyWord != null)
                {
                    currentList = GetBottomKeyWords2(newNextKeyWord, input, newIndex, amountOfParentheses, currentList, errors, out newIndex, out amountOfParentheses, out errors);
                }
            }
            return currentList;
        }

        public static List<ISqlInput> GetKeyWordChildren2(SqlKeyWord keyWord, string input, int currentIndex, int amountOfParenthesesIn, List<ISqlInput> currentList, out int amountOfParentheses, out int newIndex, out string errors, out SqlKeyWord nextKeyWord)
        {
            List<ISqlInput> returnList = currentList;

            amountOfParentheses = amountOfParenthesesIn;
            newIndex = currentIndex;
            errors = "";
            nextKeyWord = null;
            bool inQuotes = false;
            string currentWord = "";
            bool justInQuotes = false;
            bool justHadComma = false;
            SqlKeyWord commaGroup = new SqlKeyWord("CommaGroup", keyWord, new Range(1, int.MaxValue, true, false), KeyWordTypes.CommaGroup);
            for (int i = currentIndex; i < input.Length; i++)
            {
                newIndex = i;
                if (!inQuotes)
                {
                    if (input[i] == '"')
                    {
                        if (currentWord.Trim() == "")
                        {
                            inQuotes = true;
                            currentWord = "";
                        }
                        else
                        {
                            errors += "Incorect Quote Syntax, ";
                            break;
                        }
                    }
                    else if (input[i] == '(')
                    {
                        if (currentWord.Trim() == "")
                        {
                            SqlKeyWord newParenthesesGroup = new SqlKeyWord("ParenthesesGroup", keyWord, new Range(1, int.MaxValue, false, true), KeyWordTypes.ParenthesesGroup);

                            newParenthesesGroup.Children = GetKeyWordChildren2(newParenthesesGroup, input, i + 1, amountOfParentheses, newParenthesesGroup.Children, out amountOfParentheses, out newIndex, out errors, out nextKeyWord);
                            i = newIndex;
                            if (keyWord.ChildrenAmountRange.CanHaveCommas)
                            {
                                commaGroup.Children.Add(newParenthesesGroup);
                                returnList.Add(commaGroup);
                                commaGroup = new SqlKeyWord("CommaGroup", keyWord, new Range(1, int.MaxValue, true, false), KeyWordTypes.CommaGroup);
                            }
                            else
                            {
                                returnList.Add(newParenthesesGroup);
                            }
                            amountOfParentheses++;
                        }
                        else if (keywords.ContainsKey(currentWord.ToLower()) && !justInQuotes)
                        {
                            if (keyWord.ChildrenAmountRange.CanHaveCommas)
                            {
                                returnList.Add(commaGroup);
                            }
                            nextKeyWord = new SqlKeyWord(currentWord, keyWord.Parent, keywords[currentWord.ToLower()], KeyWordTypes.Command);
                            break;
                        }
                        else
                        {
                            errors += "Incorect Parentheses Syntax, ";
                            break;
                        }
                    }
                    else if (input[i] == ')')
                    {
                        amountOfParentheses--;
                        if ((!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes) && currentWord.Trim() != "")
                        {
                            if (keyWord.ChildrenAmountRange.CanHaveCommas)
                            {
                                commaGroup.Children.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                                returnList.Add(commaGroup);
                                break;
                            }
                            else
                            {
                                returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                            }
                            break;
                        }
                        else
                        {
                            errors += "Incorect Use of " + currentWord + ", ";
                            break;
                        }
                    }
                    else if (input[i] == ',')
                    {
                        if ((!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes))
                        {
                            if (keyWord.ChildrenAmountRange.CanHaveCommas)
                            {
                                if (currentWord.Trim() != "")
                                {
                                    if (keyWord.KeyWordType != KeyWordTypes.CommaGroup)
                                    {

                                        commaGroup.Children.Add(new SqlCustomInput(currentWord, commaGroup, justInQuotes));

                                        returnList.Add(commaGroup);
                                    }
                                    else
                                    {
                                        returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                                    }
                                }
                                currentWord = "";
                            }
                            else
                            {
                                errors += "Incorect Comma Syntax, ";
                                break;
                            }
                        }
                        else
                        {
                            errors += "Incorect Use of " + currentWord + ", ";
                            break;
                        }
                        currentWord = "";
                        justHadComma = true;
                        commaGroup = new SqlKeyWord("CommaGroup", keyWord, new Range(1, int.MaxValue, true, false), KeyWordTypes.CommaGroup);
                    }
                    else if (input[i] == ' ')
                    {
                        if (currentWord.Trim() != "" && !justHadComma)
                        {
                            if (keywords.ContainsKey(currentWord.ToLower()) && !justInQuotes)
                            {
                                if (keyWord.ChildrenAmountRange.CanHaveCommas && commaGroup.Children.Count > 0)
                                {
                                    returnList.Add(commaGroup);
                                }
                                nextKeyWord = new SqlKeyWord(currentWord, keyWord.Parent, keywords[currentWord.ToLower()], KeyWordTypes.Command);
                                break;
                            }
                            else
                            {
                                if (keyWord.ChildrenAmountRange.CanHaveCommas)
                                {
                                    SqlCustomInput newCustomInput = new SqlCustomInput(currentWord, commaGroup, justInQuotes);
                                    commaGroup.Children.Add(newCustomInput);
                                }
                                else
                                {
                                    returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                                }
                            }
                        }
                        justHadComma = false;
                        currentWord = "";
                    }
                    else
                    {
                        currentWord += input[i];
                    }
                    justInQuotes = false;
                }
                else
                {
                    //in Quotes
                    if (input[i] == '"')
                    {
                        inQuotes = false;
                        justInQuotes = true;
                    }
                    else
                    {
                        currentWord += input[i];
                    }
                }

                if (i + 1 >= input.Length)
                {
                    if (keyWord.ChildrenAmountRange.CanHaveCommas)
                    {
                        if (currentWord.Trim() != "")
                        {
                            if (!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes)
                            {
                                commaGroup.Children.Add(new SqlCustomInput(currentWord, commaGroup, justInQuotes));
                                returnList.Add(commaGroup);
                            }
                            else
                            {
                                errors += "Incorect Use of " + currentWord + ", ";
                                break;
                            }
                        }
                        else if (commaGroup.Children.Count > 0)
                        {
                            returnList.Add(commaGroup);
                        }
                    }
                    else
                    {
                        if (currentWord.Trim() != "")
                        {
                            if (!keywords.ContainsKey(currentWord.ToLower()) || justInQuotes)
                            {
                                returnList.Add(new SqlCustomInput(currentWord, keyWord, justInQuotes));
                            }
                            else
                            {
                                errors += "Incorect Use of " + currentWord + ", ";
                                break;
                            }
                        }
                    }
                }
            }
            if (returnList.Count < keyWord.ChildrenAmountRange.Min)
            {
                errors += "Incorect Use of " + keyWord.Input + ", ";
            }
            if (returnList.Count >= keyWord.ChildrenAmountRange.Max)
            {
                errors += "Incorect Use of " + keyWord.Input + ", ";
            }
            return returnList;
        }

        public static void LoadXML()
        {
            foreach (XElement tableElement in xdoc.Root.Elements())
            {
                Table table = new Table(tableElement);
                tables.Add(table.Name, table);
            }
            xdoc.Root.RemoveAll();
        }

        public static void SaveXML()
        {
            List<KeyValuePair<string, Table>> tableList = tables.ToList();
            for (int i = 0; i < tableList.Count; i++)
            {
                Table table = tableList[i].Value;
                XElement tableElement = new XElement("Table");
                tableElement.Add(new XAttribute("name", tableList[i].Key));
                tableElement.Add(new XAttribute("currentID", table.CurrentID));

                XElement collumnsElement = new XElement("Collumns");
                foreach (SqlColumn collumn in table.SqlColumns)
                {
                    XElement currentCollumn = new XElement("Collumn");
                    currentCollumn.Add(new XAttribute(collumn.Name, collumn.VarType));
                    collumnsElement.Add(currentCollumn);
                }
                tableElement.Add(collumnsElement);

                XElement binaryTree = new XElement("BinaryTree");
                if (table.Tree.BaseNode != null)
                {
                    binaryTree = FillXMLBinaryTree(table, table.Tree.BaseNode, binaryTree);
                }

                tableElement.Add(binaryTree);
                xdoc.Root.Add(tableElement);
            }
            xdoc.Save(xmlFile);
        }

        public static XElement FillXMLBinaryTree(Table table, Node<SqlRow> node, XElement currentElement)
        {
            XElement newNode = new XElement("Node", new XAttribute("id", node.Value.Id));
            XElement cells = new XElement("Cells");
            for (int i = 0; i < node.Value.Cells.Count; i++)
            {
                XElement cell = new XElement("Cell", new XAttribute("Value", node.Value.Cells[i].Value));
                cells.Add(cell);
            }
            XElement left = new XElement("Left");
            XElement right = new XElement("Right");
            if (node.Left != null)
            {
                left = FillXMLBinaryTree(table, node.Left, left);
            }
            if (node.Right != null)
            {
                right = FillXMLBinaryTree(table, node.Right, right);
            }
            newNode.Add(cells);
            newNode.Add(left);
            newNode.Add(right);
            currentElement.Add(newNode);
            return currentElement;
        }

        //Returns a string that displays the table with ASCII
        public static string DisplayTable(Table table, List<SqlRow> rows, List<SqlColumn> columns)
        {
            string returnString = "";
            List<int> columnWidths = new List<int>(); //How wide a column each needs to be in characters to fit all the data in it
            int paddingSize = 1;
            int headerLevel = 0;//The current level of the header being displayed 0 = bottom of Table, 1 = bottom of Header, 2 = header, 3 = top of Header 

            //Characters for the sides and corners of the table
            char cornerChar = '+';
            char verticalChar = '|';
            char horizontalChar = '-';

            for (int i = 0; i < table.SqlColumns.Count; i++)
            {
                columnWidths.Add(table.SqlColumns[i].Name.Length);
            }
            //displays table without header
            if (rows.Count > 0)
            {
                returnString += DisplayColumns(table, rows, columns, 0, 0, paddingSize, verticalChar, columnWidths, out columnWidths);
                returnString += Environment.NewLine;
            }
            //loops through collumns to add header
            for (int x = columns.Count - 1; x >= 0; x--)
            {
                //if header level is not the bottom of the table
                if (headerLevel != 0)
                {
                    //if on last collumn new Line
                    if (x + 1 >= columns.Count)
                    {
                        returnString = Environment.NewLine + returnString;
                    }
                    //if header level is not header display a solid line
                    if (headerLevel != 2)
                    {
                        returnString = RepeatChar(horizontalChar, columnWidths[x] + (paddingSize * 2)) + cornerChar + returnString;
                    }
                    else
                    {
                        returnString = RepeatChar(' ', paddingSize) + columns[x].Name + RepeatChar(' ', columnWidths[x] + paddingSize - columns[x].Name.Length) + verticalChar + returnString;
                    }
                    //if on first collumn display cornerChar
                    if (x == 0)
                    {
                        //if header level is the header display verticalChar else display cornerChar
                        if (headerLevel == 2)
                        {
                            returnString = verticalChar + returnString;
                        }
                        else
                        {
                            returnString = cornerChar + returnString;
                        }

                    }
                }
                else
                {
                    int oppositeX = columns.Count - 1 - x;

                    //if on first collumn display cornerChar
                    if (oppositeX == 0)
                    {
                        returnString += cornerChar;
                    }
                    returnString += RepeatChar(horizontalChar, columnWidths[oppositeX] + (paddingSize * 2)) + cornerChar;
                    //if on last collumn new Line
                    if (oppositeX + 1 >= columns.Count)
                    {
                        returnString += Environment.NewLine;
                    }
                }
                //if first collumn and current header level is not the top display the next header level
                if (x == 0 && headerLevel < 3)
                {
                    headerLevel++;
                    x = columns.Count;
                }

            }

            return returnString;
        }

        //Displays the value for each column
        public static string DisplayColumns(Table table, List<SqlRow> rows, List<SqlColumn> columns, int rowIndex, int columnIndex, int paddingSize, char verticalChar, List<int> currentColumnWidths, out List<int> columnWidths)
        {
            string returnString = "";
            int amountOfExtraPaddingForRow;
            columnWidths = currentColumnWidths;
            string nextCollumn = "";
            string nextRow = "";
            //if on first collumn display wall
            if (columnIndex == 0)
            {
                returnString += verticalChar;
            }

            //if this rows data is longer than the collumn width set the collumn width to the length of this rows data
            if (columnWidths[columnIndex] < rows[rowIndex].Cells[columnIndex].Value.ToString().Length)
            {
                columnWidths[columnIndex] = rows[rowIndex].Cells[columnIndex].Value.ToString().Length;
            }

            returnString += RepeatChar(' ', paddingSize) + rows[rowIndex].Cells[columnIndex].Value.ToString();//Dislays current row and collumn value

            //if not on last collumn get next collumn
            if (columnIndex + 1 < columns.Count)
            {
                nextCollumn = DisplayColumns(table, rows, columns, rowIndex, columnIndex + 1, paddingSize, verticalChar, columnWidths, out columnWidths);
            }
            //if not on last row get next row
            else if (rowIndex + 1 < rows.Count)
            {
                nextRow = DisplayColumns(table, rows, columns, rowIndex + 1, 0, paddingSize, verticalChar, columnWidths, out columnWidths);
            }
            //add padding for collumn
            amountOfExtraPaddingForRow = columnWidths[columnIndex] - rows[rowIndex].Cells[columnIndex].Value.ToString().Length;
            returnString += RepeatChar(' ', paddingSize + amountOfExtraPaddingForRow) + verticalChar;
            //display next collumn and row
            returnString += nextCollumn;
            //if there is a next row new line and display it
            if (nextRow != "")
            {
                returnString += Environment.NewLine + nextRow;
            }


            return returnString;
        }

        //Returns a string of a character repeated for a certain amount of time
        public static string RepeatChar(char letter, int amountOfTimes)
        {
            string returnString = "";
            for (int i = 0; i < amountOfTimes; i++)
            {
                returnString += letter;
            }
            return returnString;
        }
    }
}
