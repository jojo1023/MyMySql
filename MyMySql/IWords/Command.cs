using MyMySql.ICustomWords;
using MyMySql.IWords.ICustomWords;
using MyMySql.IWords.ILanguageWords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql.IWords
{
    public class Command
    {
        public List<List<CommandKeywordInfo>> KeywordsInCommand { get; set; }
        public List<TableWord> TablesInCommand { get; set; }
        public List<ColumnWord> ColumnsInCommand { get; set; }
        public List<CustomCustomWord> CustomCustomsInCommand { get; set; }

        public InputInfo Input { get; set; }
        public Type Output { get; set; }
        public Command ChildCommand { get; set; }
        public Func<ICommandReturn, List<IWord>, Command, ICommandReturn> BeforeChildCommandFunction { get; set; }
        public Func<ICommandReturn, Command, ICommandReturn> AfterChildCommandFunction { get; set; }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, InputInfo input, Type output, Func<ICommandReturn, List<IWord>, Command, ICommandReturn> beforeChildCommandFucntion, Func<ICommandReturn, Command, ICommandReturn> afterChildCommandFunction)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = input;
            Output = output;
            ChildCommand = null;
            BeforeChildCommandFunction = beforeChildCommandFucntion;
            AfterChildCommandFunction = afterChildCommandFunction;
        }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, Command dictionaryCommand)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = dictionaryCommand.Input;
            Output = dictionaryCommand.Output;
            ChildCommand = null;
            BeforeChildCommandFunction = dictionaryCommand.BeforeChildCommandFunction;
            AfterChildCommandFunction = dictionaryCommand.AfterChildCommandFunction;
        }
        public Command(List<List<CommandKeywordInfo>> keywordsInCommand, Command childCommand, Command dictionaryCommand)
        {
            KeywordsInCommand = keywordsInCommand;
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            Input = dictionaryCommand.Input;
            Output = dictionaryCommand.Output;
            ChildCommand = childCommand;
            BeforeChildCommandFunction = dictionaryCommand.BeforeChildCommandFunction;
            AfterChildCommandFunction = dictionaryCommand.AfterChildCommandFunction;
        }
        public void GetCustomWordsInCommand()
        {
            TablesInCommand = new List<TableWord>();
            ColumnsInCommand = new List<ColumnWord>();
            CustomCustomsInCommand = new List<CustomCustomWord>();
            foreach (List<CommandKeywordInfo> keywords in KeywordsInCommand)
            {
                if (keywords.Count == 1)
                {
                    GetCustomWordsInCommandRecursive(keywords[0].CommandKeyword, false);
                }
                else
                {
                    throw new Exception("Command Group Compiler Failed Exception");
                }
            }
            if (ChildCommand != null)
            {
                ChildCommand.GetCustomWordsInCommand();
                ColumnsInCommand.AddRange(ChildCommand.ColumnsInCommand);
                CustomCustomsInCommand.AddRange(ChildCommand.CustomCustomsInCommand);
                TablesInCommand.AddRange(ChildCommand.TablesInCommand);
            }
        }
        void GetCustomWordsInCommandRecursive(IWord currentWord, bool parentIsLogicOpperation)
        {
            if (currentWord.AllWordType == AllWordTypes.Table && !currentWord.Initializing && !parentIsLogicOpperation)
            {
                TablesInCommand.Add((TableWord)currentWord);
            }
            else if (currentWord.AllWordType == AllWordTypes.Column && !currentWord.Initializing && !parentIsLogicOpperation)
            {
                ColumnsInCommand.Add((ColumnWord)currentWord);
            }
            else if (currentWord.AllWordType == AllWordTypes.CustomCustom && !currentWord.Initializing && !parentIsLogicOpperation)
            {
                CustomCustomsInCommand.Add((CustomCustomWord)currentWord);
            }

            foreach (IWord child in currentWord.Children)
            {
                GetCustomWordsInCommandRecursive(child, currentWord.AllWordType == AllWordTypes.LogicOperation);
            }
        }
        public ICommandReturn RunCommand(ICommandReturn parentCommandReturn)
        {
            List<FunctionWord> functions = new List<FunctionWord>();
            List<IWord> userInfo = new List<IWord>();
            ICommandReturn commandReturn = null;
            foreach (List<CommandKeywordInfo> keyword in KeywordsInCommand)
            {
                if (keyword.Count > 0)
                {
                    userInfo.AddRange(GetAllUserInfo(keyword[0].CommandKeyword));
                }
            }
            for (int i = 0; i < userInfo.Count; i++)
            {
                if (userInfo[i].AllWordType == AllWordTypes.Function)
                {
                    FunctionWord currentFunction = (FunctionWord)userInfo[i];
                    functions.Add(currentFunction);
                    List<IWord> functionReplacement = currentFunction.BeforeCommandFunction.Invoke(currentFunction, TablesInCommand);
                    userInfo.RemoveAt(i);
                    userInfo = AddRangetAtIndex(userInfo, functionReplacement, i);
                    i = i + functionReplacement.Count;
                }
            }
            commandReturn = BeforeChildCommandFunction.Invoke(parentCommandReturn, userInfo, this);
            if (ChildCommand != null)
            {
                commandReturn = ChildCommand.RunCommand(commandReturn);
            }
            commandReturn = AfterChildCommandFunction.Invoke(commandReturn, this);
            foreach (FunctionWord function in functions)
            {
                commandReturn = function.AfterCommandFunction.Invoke(function, commandReturn);
            }
            return commandReturn;
        }
        List<IWord> GetAllUserInfo(IWord currentWord)
        {
            List<IWord> returnList = new List<IWord>();
            if (currentWord.UserInfo)
            {
                returnList.Add(currentWord);
            }
            else
            {
                foreach (IWord child in currentWord.Children)
                {
                    returnList.AddRange(GetAllUserInfo(child));
                }
            }
            return returnList;
        }
        List<IWord> AddRangetAtIndex(List<IWord> list, List<IWord> words, int index)
        {
            List<IWord> returnList = new List<IWord>();
            bool addedWords = false;
            for (int i = 0; i < list.Count + words.Count; i++)
            {
                if (i >= index && i < index + words.Count)
                {
                    returnList.Add(words[i - index]);
                    addedWords = true;
                }
                else if (addedWords)
                {
                    returnList.Add(list[i - words.Count]);
                }
                else
                {
                    returnList.Add(list[i]);
                }
            }
            return returnList;
        }
    }
    public struct CommandKeywordInfo
    {
        public Keyword CommandKeyword;
        public List<List<WordRange>> KeywordRangesThatDontWork;
    }
    public struct InputInfo
    {
        public Type InputType;
        public List<string> KeywordsNotAllowedAsInput;
    }
}
