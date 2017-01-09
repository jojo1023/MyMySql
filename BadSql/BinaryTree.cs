using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    public class BinaryTree<T> where T : IComparable
    {
        public Node<T> BaseNode { get; set; }
        public int BalanceValue { get; set; }
        public BinaryTree(T baseNode, int balanceValue)
        {
            BaseNode = new Node<T>(baseNode);
            BalanceValue = balanceValue;
        }
        public BinaryTree(int balanceValue)
        {
            BalanceValue = balanceValue;
        }
        public Node<T> AddNode(T value)
        {
            return AddNodePrivate(new Node<T>(value), false);
        }
        Node<T> AddNodePrivate(Node<T> node, bool delteting)
        {
            if (BaseNode != null)
            {
                return AddNodeRecursive(node, BaseNode, delteting);
            }
            else
            {
                BaseNode = node;
                return BaseNode;
            }
        }
        Node<T> AddNodeRecursive(Node<T> newNode, Node<T> currentNode, bool delteting)
        {
            Node<T> node = currentNode;
            while (true)
            {
                if (newNode.Value.CompareTo(node.Value) > 0)
                {
                    if (node.Right != null)
                    {
                        node = node.Right;
                        //AddNodeRecursive(newNode, node.Right, delteting);
                    }
                    else
                    {
                        newNode.Parent = node;
                        node.Right = newNode;
                        if (node != null && !delteting)
                        {
                            //BalanceNode(node);
                        }
                        break;
                    }
                }
                else if (newNode.Value.CompareTo(node.Value) < 0)
                {
                    if (node.Left != null)
                    {
                        node = node.Left;
                        //AddNodeRecursive(newNode, node.Left, delteting);
                    }
                    else
                    {
                        newNode.Parent = node;
                        node.Left = newNode;
                        if (node != null && !delteting)
                        {
                            //BalanceNode(node);
                        }
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return node;
        }

        Node<T> GetNode(T value, Node<T> currentNode)
        {
            if (currentNode != null)
            {
                if (currentNode.Value.CompareTo(value) == 0)
                {
                    return currentNode;
                }
                if (currentNode.Left != null && currentNode.Left.Value.CompareTo(value) == 0)
                {
                    return currentNode.Left;
                }
                else if (currentNode.Left != null && value.CompareTo(currentNode.Left.Value) == -1)
                {
                    Node<T> possibleReturn = GetNode(value, currentNode.Left);
                    if (possibleReturn != null)
                    {
                        return possibleReturn;
                    }
                }
                if (currentNode.Right != null && currentNode.Right.Value.CompareTo(value) == 0)
                {
                    return currentNode.Right;
                }
                else if (currentNode.Right != null && value.CompareTo(currentNode.Right.Value) == 1)
                {
                    Node<T> possibleReturn = GetNode(value, currentNode.Right);
                    if (possibleReturn != null)
                    {
                        return possibleReturn;
                    }
                }
            }
            return null;
        }
        public void DeleteNode(Node<T> node, bool priorityLeft, bool AddNodeBack, bool dontAddChildren, bool userDelete)
        {
            if (node != null)
            {
                List<T> nodesToBalance = new List<T>();
                if (node.Left != null && userDelete)
                {
                    nodesToBalance.Add(node.Left.Value);
                }
                if (node.Right != null && userDelete)
                {
                    nodesToBalance.Add(node.Right.Value);
                }

                if (node.Parent != null)
                {
                    node.Parent.DeleteChild(node);
                }
                else
                {
                    BaseNode = null;
                }
                if (node != null && !dontAddChildren)
                {
                    if (priorityLeft)
                    {
                        if (node.Left != null)
                        {
                            AddChildren(node.Left);
                        }
                        if (AddNodeBack)
                        {
                            AddNodePrivate(new Node<T>(node.Value), true);
                        }
                        if (node.Right != null)
                        {
                            AddChildren(node.Right);
                        }
                    }
                    else
                    {
                        if (node.Right != null)
                        {
                            AddChildren(node.Right);
                        }
                        if (AddNodeBack)
                        {
                            AddNodePrivate(new Node<T>(node.Value), true);
                        }
                        if (node.Left != null)
                        {
                            AddChildren(node.Left);
                        }
                    }

                }
                foreach (T TNode in nodesToBalance)
                {
                    //BalanceNode(GetNode(TNode, BaseNode));
                }
            }
        }
        public Node<T> DeleteNode(T value, bool priorityLeft, bool AddNodeBack, bool dontAddChildren, bool userDelete)
        {
            Node<T> node = GetNode(value, BaseNode);
            DeleteNode(node, priorityLeft, AddNodeBack, dontAddChildren, userDelete);
            return node;
        }

        public void UserDelete(T value)
        {
            Node<T> node = GetNode(value, BaseNode);
            if (node != null)
            {
                if (node.Left != null)
                {
                    Node<T> greatestLeftSubtree = FindGreatestInLeftSubtree(node.Left);
                    if (greatestLeftSubtree != null)
                    {
                        if (greatestLeftSubtree.Left != null)
                        {
                            if (greatestLeftSubtree.Parent.isRight(greatestLeftSubtree))
                            {
                                greatestLeftSubtree.Parent.DeleteChild(greatestLeftSubtree);
                                greatestLeftSubtree.Parent.Right = greatestLeftSubtree.Left;
                            }
                            else
                            {
                                greatestLeftSubtree.Parent.DeleteChild(greatestLeftSubtree);
                                greatestLeftSubtree.Parent.Left = greatestLeftSubtree.Left;
                            }

                            greatestLeftSubtree.Left.Parent = greatestLeftSubtree.Parent;
                        }
                        else
                        {
                            greatestLeftSubtree.Parent.DeleteChild(greatestLeftSubtree);
                        }
                        node.Value = greatestLeftSubtree.Value;
                        //BalanceNode(greatestLeftSubtree.Parent);
                    }
                }
                else if (node.Right != null)
                {
                    Node<T> Parent = node.Parent;
                    if (Parent != null)
                    {
                        if (Parent.isRight(node))
                        {
                            Parent.Right = node.Right;
                        }
                        else
                        {
                            Parent.Left = node.Right;
                        }
                    }
                    
                    node = node.Right;
                    node.Parent = Parent;
                    //BalanceNode(node);
                }
                else
                {
                    if (node.Parent != null)
                    {
                        node.Parent.DeleteChild(node);
                        //BalanceNode(node.Parent);
                    }
                    else
                    {
                        BaseNode = null;
                    }
                }
            }
        }

        Node<T> FindGreatestInLeftSubtree(Node<T> currentNode)
        {
            if (currentNode != null && currentNode.Right != null)
            {
                return FindGreatestInLeftSubtree(currentNode.Right);
            }
            return currentNode;
        }

        public void AddChildren(Node<T> node)
        {
            AddNodePrivate(new Node<T>(node.Value), true);
            if (node.Left != null)
            {
                AddChildren(node.Left);
            }
            if (node.Right != null)
            {
                AddChildren(node.Right);
            }
        }
        public void BalanceNode(Node<T> node)
        {
            int leftPathCount = 0;
            int rightPathCount = 0;
            if (node.Left != null)
            {
                leftPathCount = node.Left.LongestPathCount();
                leftPathCount++;
            }
            if (node.Right != null)
            {
                rightPathCount = node.Right.LongestPathCount();
                rightPathCount++;
            }

            int differnece = Math.Abs(leftPathCount - rightPathCount);
            if (leftPathCount - rightPathCount >= BalanceValue)//left larger
            {
                //for(int i = 0; i < differnece; i++)
                //{
                Node<T> rightDeletedNode = null;
                if (node.Left != null && node.Left.Right != null)
                {
                    rightDeletedNode = DeleteNode(node.Left.Right.Value, true, false, true, false);
                }

                if (rightDeletedNode != null)
                {
                    if (node.Left.Left != null)
                    {
                        Node<T> deletedNode = DeleteNode(node.Value, true, true, false, false);
                        AddNodePrivate(rightDeletedNode, true);
                    }
                    else
                    {
                        DeleteNode(node, true, false, false, false);
                        AddNodePrivate(rightDeletedNode, true);
                        AddNodePrivate(node, true);
                    }
                }
                else
                {
                    Node<T> deletedNode = DeleteNode(node.Value, true, false, false, false);
                    AddNodePrivate(new Node<T>(deletedNode.Value), true);
                }
                //}
            }
            else if (rightPathCount - leftPathCount >= BalanceValue)//right larger
            {
                //for (int i = 0; i < differnece; i++)
                //
                Node<T> leftDeletedNode = null;
                if (node.Right != null && node.Right.Left != null)
                {
                    leftDeletedNode = DeleteNode(node.Right.Left.Value, false, false, true, false);
                }

                if (leftDeletedNode != null)
                {
                    if (node.Right.Right != null)
                    {
                        Node<T> deletedNode = DeleteNode(node.Value, false, true, false, false);
                        AddNodePrivate(leftDeletedNode, true);
                    }
                    else
                    {
                        DeleteNode(node, false, false, false, false);
                        AddNodePrivate(leftDeletedNode, true);
                        AddNodePrivate(node, true);
                    }
                }
                else
                {
                    Node<T> deletedNode = DeleteNode(node.Value, false, false, false, false);
                    AddNodePrivate(new Node<T>(deletedNode.Value), true);
                }
                //}
            }
            if (node.Parent != null)
            {
                BalanceNode(node.Parent);
            }
        }
        public int GetNodeBalance(Node<T> node)
        {
            int rightWeight = 0;
            int leftWeight = 0;
            if (node.Right != null)
            {
                rightWeight = node.Right.LongestPathCount() + 1;
            }
            if (node.Left != null)
            {
                leftWeight = node.Left.LongestPathCount() + 1;
            }
            return rightWeight - leftWeight;
        }
        public int amountOfLeafs()
        {
            return amountOfLeafsRecursive(BaseNode, 0);
        }
        public int amountOfLeafs(Node<T> node)
        {
            return amountOfLeafsRecursive(node, 0);
        }
        int amountOfLeafsRecursive(Node<T> currentNode, int currentAmount)
        {
            int amount = currentAmount;

            if (currentNode != null)
            {
                if (currentNode.Left != null || currentNode.Right != null)
                {
                    if (currentNode.Left != null)
                    {
                        amount = amountOfLeafsRecursive(currentNode.Left, amount);
                    }
                    if (currentNode.Right != null)
                    {
                        amount = amountOfLeafsRecursive(currentNode.Right, amount);
                    }
                }
                else
                {
                    amount++;
                }
            }

            return amount;
        }
        public List<T> GetNodes()
        {
            return GetNodesRecursive(BaseNode, new List<T>());
        }
        List<T> GetNodesRecursive(Node<T> currentNode, List<T> currentList)
        {
            List<T> returnList = currentList;

            if (currentNode != null)
            {
                returnList.Add(currentNode.Value);
                if (currentNode.Left != null)
                {
                    returnList = GetNodesRecursive(currentNode.Left, returnList);
                }
                if (currentNode.Right != null)
                {
                    returnList = GetNodesRecursive(currentNode.Right, returnList);
                }
            }

            return returnList;
        }

        public Node<T> GetBottomLeft(Node<T> currentNode)
        {
            if(currentNode != null && currentNode.Left != null)
            {
                return GetBottomLeft(currentNode.Left);
            }
            return currentNode;
        }

        public List<T> GetNodesSorted()
        {
            return GetRowsInOrder(GetBottomLeft(BaseNode), BaseNode, new List<T>());
        }

        List<T> GetRowsInOrder(Node<T> currentNode, Node<T> stopNode, List<T> currentList)
        {
            if (currentNode != null)
            {
                currentList.Add(currentNode.Value);

                if (currentNode.Right != null)
                {
                    currentList = GetNodesToRight(currentNode.Right, currentList);
                }
                if (currentNode.Parent != null && currentNode != stopNode)
                {
                    currentList = GetRowsInOrder(currentNode.Parent, stopNode, currentList);
                }
            }
            return currentList;
        }

        List<T> GetNodesToRight(Node<T> currentNode, List<T> currentList)
        {
            if (currentNode != null)
            {
                if (currentNode.Left != null)
                {
                    currentList = GetRowsInOrder(GetBottomLeft(currentNode.Left), currentNode.Left, currentList);
                }
                currentList.Add(currentNode.Value);
                if (currentNode.Right != null)
                {
                    currentList = GetNodesToRight(currentNode.Right, currentList);
                }
            }
            return currentList;
        }

        #region sort
        public void Sort()
        {
            List<T> input = GetNodesSorted();
            List<T> returnList = new List<T>();
            while (input.Count > 0)
            {
                returnList = insert(input[input.Count - 1], returnList);
                input.Remove(input[input.Count - 1]);
            }
            List<T> balanceList = balanceSort(returnList, new List<T>());
            BaseNode = null;
            for(int i = 0; i < balanceList.Count; i++)
            {
                AddNode(balanceList[i]);
            }
        }

        List<T> balanceSort(List<T> sortedList, List<T> currentList)
        {
            if (sortedList.Count > 0)
            {
                List<List<T>> nextLists = new List<List<T>>();
                if (sortedList.Count % 2 == 0)
                {
                    currentList.Add(sortedList[sortedList.Count / 2 - 1]);
                    sortedList.RemoveAt(sortedList.Count / 2 - 1);

                    nextLists.Add(sortedList.Take((sortedList.Count + 1) / 2 - 1).ToList());
                    nextLists.Add(sortedList.Skip((sortedList.Count + 1) / 2 - 1).ToList());
                }
                else
                {
                    currentList.Add(sortedList[(sortedList.Count + 1) / 2 - 1]);
                    sortedList.RemoveAt((sortedList.Count + 1) / 2 - 1);

                    nextLists.Add(sortedList.Take(sortedList.Count / 2 - 1).ToList());
                    nextLists.Add(sortedList.Skip(sortedList.Count / 2 - 1).ToList());
                }
                foreach (List<T> list in nextLists)
                {
                    currentList = (balanceSort(list, currentList));
                }
            }
            return currentList;
        }



        List<T> insert(T number, List<T> input)
        {
            List<T> returnList = input;
            bool added = false;
            for (int i = 0; i < returnList.Count; i++)
            {
                if (number.CompareTo(returnList[i]) <= 0)  //(number.Id <= returnList[i].Id)
                {
                    added = true;
                    T previousNum = number;
                    for (int j = i; j < returnList.Count; j++)
                    {
                        T temp = returnList[j];
                        returnList[j] = previousNum;
                        previousNum = temp;
                    }
                    returnList.Add(previousNum);
                    break;
                }
            }
            if (!added)
            {
                returnList.Add(number);
            }
            return returnList;
        }
        #endregion
    }
}
