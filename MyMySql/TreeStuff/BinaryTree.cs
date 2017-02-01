using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    public class BinaryTree<T> where T : IComparable
    {
        public BSTNode<T> BaseNode { get; set; }

        /// <summary>
        /// Default Constructor to initialize baseNode because root needs to be null to start
        /// </summary>
        public BinaryTree()
        {
            BaseNode = null;
        }

        /// <summary>
        /// Constructor where root is already built in
        /// </summary>
        /// <param name="baseNode">Root of binary tree</param>
        public BinaryTree(T baseNode)
        {
            BaseNode = new BSTNode<T>(baseNode);
        }

        /// <summary>
        /// Adds a new node to the binary tree in sorted position 
        /// </summary>
        /// <param name="value">The value of the node being added</param>
        /// <returns>The new node</returns>
        public BSTNode<T> AddNode(T value)
        {
            BSTNode<T> newNode = new BSTNode<T>(value);

            //if baseNode isn't null find where to put node else set root to the newNode
            if (BaseNode != null)
            {
                //Recursively finds sorted position for newNode and adds newNode
                AddNodeToTree(newNode);
            }
            else
            {
                BaseNode = newNode;
            }
            return newNode;
        }

        /// <summary>
        ///Delete a node in the tree without deleteing its children
        /// </summary>
        /// <param name="value">The value of the node to be deleted</param>
        public void DeleteNode(T value)
        {
            //Finds the nodeToDelete (if it exists)
            BSTNode<T> nodeToDelete = GetNode(value, BaseNode);

            if (nodeToDelete != null)
            {
                //If the nodeToDelete has any value less than it, replace node to delete with next greatest value
                if (nodeToDelete.Left != null)
                {
                    //Get Node to swap with for deletion
                    BSTNode<T> greatestNodeInLeftSubtree = FindBottomRightNode(nodeToDelete.Left);

                    //Remove greatestNodeInLeftSubtree and replace value with nodeToDelete
                    if (greatestNodeInLeftSubtree.Left != null)
                    {
                        if (greatestNodeInLeftSubtree.Parent.isRight(greatestNodeInLeftSubtree))
                        {
                            greatestNodeInLeftSubtree.Parent.Right = greatestNodeInLeftSubtree.Left;
                        }
                        else
                        {
                            greatestNodeInLeftSubtree.Parent.Left = greatestNodeInLeftSubtree.Left;
                        }
                        greatestNodeInLeftSubtree.Left.Parent = greatestNodeInLeftSubtree.Parent;
                    }
                    else
                    {
                        greatestNodeInLeftSubtree.Parent.DeleteChild(greatestNodeInLeftSubtree);
                    }
                    nodeToDelete.Value = greatestNodeInLeftSubtree.Value;
                }

                //Replaces nodeToDelete with its right child
                else if (nodeToDelete.Right != null)
                {
                    BSTNode<T> nodeToDeleteParent = nodeToDelete.Parent;

                    if (nodeToDeleteParent != null)
                    {
                        if (nodeToDeleteParent.isRight(nodeToDelete))
                        {
                            nodeToDeleteParent.Right = nodeToDelete.Right;
                        }
                        else
                        {
                            nodeToDeleteParent.Left = nodeToDelete.Right;
                        }
                    }
                    nodeToDelete.Right.Parent = nodeToDeleteParent;
                }

                //NodeToDelete doesn't have children so it is deleted
                else
                {
                    if (nodeToDelete.Parent != null)
                    {
                        nodeToDelete.Parent.DeleteChild(nodeToDelete);
                    }
                    else
                    {
                        BaseNode = null;
                    }
                }
            }
        }

        /// <summary>
        ///Gets nodes in order of their values asending
        /// </summary>
        /// <returns>Returns a List of values</returns>
        public List<T> GetNodesInOrder()
        {
            //Finds the smallest node in the tree and finds all the nodes between it and the root
            return GetAllAncestors(GetBottomLeftNode(BaseNode), BaseNode, new List<T>());
        }

        /// <summary>
        ///Sorts the nodes in the tree into a more balanced orientation
        /// </summary>
        public void Sort()
        {
            List<T> nodesInOrder = GetNodesInOrder();
            List<T> balanceList = balanceSort(nodesInOrder);//Gets the nodes in an order that when added back will make the tree balanced
            BaseNode = null;//Deletes the entire tree by making nothing point to the nodes

            //Adds every node back into the tree
            for (int i = 0; i < balanceList.Count; i++)
            {
                AddNode(balanceList[i]);
            }
        }

        /// <summary>
        ///Adds a node to the tree in a sorted position
        /// </summary>
        /// <param name="newNode">The node being added</param>
        void AddNodeToTree(BSTNode<T> newNode)
        {
            BSTNode<T> currentNode = BaseNode;

            while (true)
            {
                //if new node is greater than the current node 
                if (newNode.Value.CompareTo(currentNode.Value) > 0)
                {
                    //if current node has a right node then the current node is the current nodes right child else currentNodes right child becomes the new node
                    if (currentNode.Right != null)
                    {
                        currentNode = currentNode.Right;
                    }
                    else
                    {
                        newNode.Parent = currentNode;
                        currentNode.Right = newNode;
                        break;
                    }
                }
                //if new node is equal to the current node
                else if (newNode.Value.CompareTo(currentNode.Value) < 0)
                {
                    //if current node has a left node then the current node is the current nodes left child else currentNodes left child becomes the new node
                    if (currentNode.Left != null)
                    {
                        currentNode = currentNode.Left;
                    }
                    else
                    {
                        newNode.Parent = currentNode;
                        currentNode.Left = newNode;
                        break;
                    }
                }
                //else node is equal to node so break because tree doesn't allow duplicate data
                else
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// Gets a node in the table from its value
        /// </summary>
        /// <param name="value">The value of the node being found</param>
        /// <param name="currentNode">The current node that the function is recusing though</param>
        /// <returns>Returns a node in the binary tree that has the same value</returns>
        BSTNode<T> GetNode(T value, BSTNode<T> currentNode)
        {
            if (currentNode != null)
            {
                //if currentNode or its children is the node being found return the node
                if (currentNode.Value.CompareTo(value) == 0)
                {
                    return currentNode;
                }
                if (currentNode.Left != null && currentNode.Left.Value.CompareTo(value) == 0)
                {
                    return currentNode.Left;
                }
                if (currentNode.Right != null && currentNode.Right.Value.CompareTo(value) == 0)
                {
                    return currentNode.Right;
                }

                //if value is less than currentNode value then countinue searching to the left of currentNode 
                if (currentNode.Left != null && value.CompareTo(currentNode.Left.Value) == -1)
                {
                    return GetNode(value, currentNode.Left);
                }

                //if value is greater than currentNode value then countinue searching to the right of currentNode 
                if (currentNode.Right != null && value.CompareTo(currentNode.Right.Value) == 1)
                {
                    return GetNode(value, currentNode.Right);
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets the node that is all the way to the left (the node with the smallest value)
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns>The current node being recused through</returns>
        BSTNode<T> GetBottomLeftNode(BSTNode<T> currentNode)
        {
            //if current node has a left keep recusing left
            if (currentNode != null && currentNode.Left != null)
            {
                return GetBottomLeftNode(currentNode.Left);
            }
            return currentNode;
        }

        /// <summary>
        /// Gets the node that is all the way to the right (the node with the greatest value)
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns>The current node being recused through</returns>
        BSTNode<T> FindBottomRightNode(BSTNode<T> currentNode)
        {
            //if current node has a left keep recusing left
            if (currentNode != null && currentNode.Right != null)
            {
                return FindBottomRightNode(currentNode.Right);
            }
            return currentNode;
        }

        /// <summary>
        /// Gets a sorted list of the current nodes ancestors and their descenants
        /// </summary>
        /// <param name="currentNode">The current node being recursed through</param>
        /// <param name="lastAncestor">A placeholder root variable that prevents traversal up the tree until the subtrees have been added to the return list</param>
        /// <param name="currentList">The current list of ancestors to be returned</param>
        /// <returns></returns>
        List<T> GetAllAncestors(BSTNode<T> currentNode, BSTNode<T> lastAncestor, List<T> currentList)
        {
            if (currentNode != null)
            {
                currentList.Add(currentNode.Value);

                //if currentNode has a right child get its descendants by traversing down the tree
                if (currentNode.Right != null)
                {
                    currentList = GetAllDescendants(currentNode.Right, currentList);
                }
                //if currentNode has a parent and it is not the last ancestor countinue traversing up the tree
                if (currentNode.Parent != null && currentNode != lastAncestor)
                {
                    currentList = GetAllAncestors(currentNode.Parent, lastAncestor, currentList);
                }
            }
            return currentList;
        }

        /// <summary>
        /// Gets a sorted list of all the descendants of a node
        /// </summary>
        /// <param name="currentNode">The current node being recursed through</param>
        /// <param name="currentList">The current list of descedants</param>
        /// <returns></returns>
        List<T> GetAllDescendants(BSTNode<T> currentNode, List<T> currentList)
        {
            if (currentNode != null)
            {
                //if current node has a left child get all its decendants in order by recusing up from the most bottom left node
                if (currentNode.Left != null)
                {
                    currentList = GetAllAncestors(GetBottomLeftNode(currentNode.Left), currentNode.Left, currentList);
                }

                currentList.Add(currentNode.Value);

                //if current node has a left child get all its decendants in order by recusing down
                if (currentNode.Right != null)
                {
                    currentList = GetAllDescendants(currentNode.Right, currentList);
                }
            }
            return currentList;
        }

        /// <summary>
        /// Puts the nodes in the tree into an order that when added back will put the tree in a more optimal/balanced orientation
        /// </summary>
        /// <param name="sortedList">A sorted list of all the nodes in the tree</param>
        /// <returns></returns>
        List<T> balanceSort(List<T> sortedList)
        {
            List<T> returnList = new List<T>();

            //Splits list around median, adding median to return list and removing it from sortedList, then repeating until all values have been added
            if (sortedList.Count > 0)
            {
                List<T> bottomHalfToBeBalanced;
                List<T> TopHalfToBeBalanced;
                
                if (sortedList.Count % 2 == 0)
                {
                    returnList.Add(sortedList[sortedList.Count / 2 - 1]);
                    sortedList.RemoveAt(sortedList.Count / 2 - 1);

                    bottomHalfToBeBalanced = sortedList.Take((sortedList.Count + 1) / 2 - 1).ToList();
                    TopHalfToBeBalanced = sortedList.Skip((sortedList.Count + 1) / 2 - 1).ToList();
                }
                else
                {
                    returnList.Add(sortedList[(sortedList.Count + 1) / 2 - 1]);
                    sortedList.RemoveAt((sortedList.Count + 1) / 2 - 1);

                    bottomHalfToBeBalanced = sortedList.Take(sortedList.Count / 2 - 1).ToList();
                    TopHalfToBeBalanced = sortedList.Skip(sortedList.Count / 2 - 1).ToList();
                }
                
                returnList.AddRange(balanceSort(bottomHalfToBeBalanced));
                returnList.AddRange(balanceSort(TopHalfToBeBalanced));
            }
            return returnList;
        }
    }
}
