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
        static BinaryTree<Student> tree = new BinaryTree<Student>(2);
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

            do
            {
                //input = Console.ReadLine().Split(' ');
                string errors;
                originalInput = Console.ReadLine();
                //input = Split(originalInput, out errors);
                input = Split2(originalInput, out errors);
                if (input.Count > 0 && errors == "")
                {
                    switch (input[0].Input.ToLower())
                    {
                        #region Select
                        case ("select"):
                            if (input.Count >= 2)
                            {
                                List<SqlColumn> collums = new List<SqlColumn>();
                                whereCollum = null;
                                whereOpperation = Opperations.EqualTo;
                                whereValue = null;
                                bool whereWorked = false;
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
                                if (input.Count >= 3)
                                {
                                    SqlKeyWord whereKeyWord = input[2];
                                    whereWorked = GetWhereInfo(whereKeyWord, table, out whereCollum, out whereOpperation, out whereValue);
                                    if (!whereWorked)
                                    {
                                        errors += "Incorect Where Syntax, ";
                                        break;
                                    }
                                }

                                List<SqlRow> rows;
                                if (whereWorked)
                                {
                                    rows = table.Select(whereCollum.Name, whereOpperation, whereValue);
                                }
                                else
                                {
                                    rows = table.Select();
                                }
                                DisplayRows(table, rows, collums);
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
                                if (input.Count >= 3 && input[1].Input.ToLower() == "into" && input[2].Input.ToLower() == "values")
                                {
                                    if (tables.ContainsKey(input[1].Children[0].Input))
                                    {
                                        table = tables[input[1].Children[0].Input];
                                        Values = input[2];
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
                                                    catch (Exception e)
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
                                Console.WriteLine(values.Count.ToString() + " Row(s) Inserted");

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
                                                Console.WriteLine("Table Added");
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
                            whereCollum = null;
                            whereOpperation = Opperations.EqualTo;
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

                                    int amountOfRows;
                                    if (!hasWhere)
                                    {
                                        amountOfRows = table.Delete();
                                    }
                                    else
                                    {
                                        amountOfRows = table.Delete(whereCollum.Name, whereOpperation, whereValue);
                                    }

                                    Console.WriteLine(amountOfRows.ToString() + " Rows " + "Affected");
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
                                Console.WriteLine("Table Sorted");
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
                                        Console.WriteLine("1 Table Droped");
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
                        //fix with new parseing
                        #region Update
                        case ("update"):
                            whereCollum = null;
                            whereOpperation = Opperations.EqualTo;
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
                                    List<SetPair> setPairs = new List<SetPair>();
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
                                                    setPairs.Add(new SetPair(collumnToSet, value));
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
                                    
                                    if(input.Count >= 3 && input[2].Input.ToLower() == "where")
                                    {
                                        hasWhere = true;
                                        if(!GetWhereInfo(input[2], table, out whereCollum, out whereOpperation, out whereValue))
                                        {
                                            errors += "Incorect Syntax Near Where, ";
                                            break;
                                        }
                                    }
                                    int amountOfUpdatedRows;
                                    if (!hasWhere)
                                    {
                                        amountOfUpdatedRows = table.Update(setPairs, out errors);
                                    }
                                    else
                                    {
                                        amountOfUpdatedRows = table.Update(setPairs, whereCollum.Name, whereOpperation, whereValue, out errors);
                                    }
                                    if (errors == "")
                                    {
                                        Console.WriteLine(amountOfUpdatedRows + " Rows Updated");
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
                }
                if (errors.Length > 2)
                {
                    errors = errors.Remove(errors.Length - 2, 2);
                    Console.WriteLine(errors);
                }
            } while (originalInput.ToLower() != "exit");
            SaveXML();
        }
        public static bool GetWhereInfo(SqlKeyWord WhereKeyWord, Table table, out SqlColumn whereCollum, out Opperations whereOpperation, out IComparable whereValue)
        {
            whereCollum = null;
            whereOpperation = Opperations.EqualTo;
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
                            whereOpperation = Opperations.EqualTo;
                            break;
                        case (">"):
                            whereOpperation = Opperations.GreaterThan;
                            break;
                        case ("<"):
                            whereOpperation = Opperations.LessThan;
                            break;
                        case (">="):
                            whereOpperation = Opperations.GreaterThanOrEqualTo;
                            break;
                        case ("<="):
                            whereOpperation = Opperations.LessThanOrEqualTo;
                            break;
                        case ("!="):
                            whereOpperation = Opperations.NotEqualTo;
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

                XElement binaryTree = new XElement("BinaryTree", new XAttribute("BalanceValue", table.Tree.BalanceValue));
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

        public static void DisplayRows(Table table, List<SqlRow> rows, List<SqlColumn> collumns)
        {
            int startY = Console.CursorTop;
            int currentY = startY;
            int currentX = 0;
            char cornerChar = '+';
            char verticalChar = '|';
            char horizontalChar = '-';
            int currentYBefore;
            for (int x = 0; x < table.SqlColumns.Count; x++)
            {
                if (collumns.Contains(table.SqlColumns[x]))
                {
                    int CollumnWidth = 0;
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(cornerChar);
                    currentY++;
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(verticalChar + " " + table.SqlColumns[x].Name + " ");
                    CollumnWidth = table.SqlColumns[x].Name.Length;
                    currentY++;
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(cornerChar);
                    currentY++;
                    currentYBefore = currentY;
                    for (int y = currentYBefore; y < currentYBefore + rows.Count; y++)
                    {
                        Console.SetCursorPosition(currentX, currentY);
                        Console.Write(verticalChar + " " + rows[y - currentYBefore].Cells[x].Value.ToString() + " ");
                        if (CollumnWidth < rows[y - currentYBefore].Cells[x].Value.ToString().Length)
                        {
                            CollumnWidth = rows[y - currentYBefore].Cells[x].Value.ToString().Length;
                        }
                        currentY++;
                    }
                    Console.SetCursorPosition(currentX, currentY);
                    Console.Write(cornerChar + RepeatChar(horizontalChar, CollumnWidth + 2));

                    Console.SetCursorPosition(currentX, currentYBefore - 1);
                    Console.Write(cornerChar + RepeatChar(horizontalChar, CollumnWidth + 2));

                    Console.SetCursorPosition(currentX, startY);
                    Console.Write(cornerChar + RepeatChar(horizontalChar, CollumnWidth + 2));

                    currentX += CollumnWidth + 3;
                    currentY = startY;
                }
            }
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(cornerChar);
            currentY++;
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(verticalChar);

            currentY++;
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(cornerChar);
            currentY++;
            currentYBefore = currentY;
            for (int y = currentYBefore; y < currentYBefore + rows.Count; y++)
            {
                Console.SetCursorPosition(currentX, currentY);
                Console.Write(verticalChar);
                currentY++;
            }
            Console.SetCursorPosition(currentX, currentY);
            Console.Write(cornerChar);

            Console.SetCursorPosition(0, currentY + 1);
        }

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
