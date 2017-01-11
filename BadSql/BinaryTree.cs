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

        public BinaryTree()
        {
        }

        public BinaryTree(T baseNode)
        {
            BaseNode = new Node<T>(baseNode);
        }

        public Node<T> AddNode(T valueOfNewNode)
        {
            Node<T> newNode = new Node<T>(valueOfNewNode);
            //if base node isn't node find where to put node else set base node to the new node
            if (BaseNode != null)
            {
                AddNodeToTree(newNode);
            }
            else
            {
                BaseNode = newNode;
            }
            return newNode;
        }

        //Delete a node in the tree without deleteing its children
        public void DeleteNode(T valueOfNodeToDelete)
        {
            Node<T> nodeToDelete = GetNode(valueOfNodeToDelete, BaseNode);
            //if node to delete was found countinue
            if (nodeToDelete != null)
            {
                //if node to delete has a left child
                if (nodeToDelete.Left != null)
                {
                    //finds the node with the greatest value to the left of the node to delete
                    Node<T> greatestNodeInLeftSubtree = FindBottomRightNode(nodeToDelete.Left);

                    //if the greatest node in left subtree has a left child (it won't ever have a right child)
                    if (greatestNodeInLeftSubtree.Left != null)
                    {
                        //if greatest node in left subtree is right of its parent set its parent right child to greatest node in left subtree left child
                        if (greatestNodeInLeftSubtree.Parent.isRight(greatestNodeInLeftSubtree))
                        {
                            greatestNodeInLeftSubtree.Parent.Right = greatestNodeInLeftSubtree.Left;
                        }
                        //set greatest node in left subtree parents right child to greatest node in left subtree left child
                        else
                        {
                            greatestNodeInLeftSubtree.Parent.Left = greatestNodeInLeftSubtree.Left;
                        }
                        //set greatest node in left subtree left child to its old parent
                        greatestNodeInLeftSubtree.Left.Parent = greatestNodeInLeftSubtree.Parent;
                    }
                    //if greatest node in left subtree doesn't have children delete it from its parent
                    else
                    {
                        greatestNodeInLeftSubtree.Parent.DeleteChild(greatestNodeInLeftSubtree);
                    }
                    //set node to delete value to greatest node in left subtree value
                    nodeToDelete.Value = greatestNodeInLeftSubtree.Value;
                }
                //if node to delete doesn't have left child and has right child
                else if (nodeToDelete.Right != null)
                {
                    Node<T> nodeToDeleteParent = nodeToDelete.Parent;
                    //if node to delete has a parent
                    if (nodeToDeleteParent != null)
                    {
                        //if node to delete is the right child of its parent then set its parents right child to node to deletes right child
                        if (nodeToDeleteParent.isRight(nodeToDelete))
                        {
                            nodeToDeleteParent.Right = nodeToDelete.Right;
                        }
                        //else set node to deletes parent left child to node to deletes right child
                        else
                        {
                            nodeToDeleteParent.Left = nodeToDelete.Right;
                        }
                    }
                    //set node to deletes right childs parent to node to delete parent
                    nodeToDelete.Right.Parent = nodeToDeleteParent;
                }
                //if node to delete doesn't have any children
                else
                {
                    //if node to delete has parent delete it from its parent else node to delete is the base node so set the base node to null
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
        
        //Gets nodes in order of their values asending
        public List<T> GetNodesInOrder()
        {
            return GetNodesSortedBetweenToNodes(GetBottomLeftNode(BaseNode), BaseNode, new List<T>());
        }

        //Sorts the nodes in the tree into a more balanced orientation
        public void Sort()
        {
            List<T> nodesInOrder = GetNodesInOrder();
            List<T> balanceList = balanceSort(nodesInOrder, new List<T>());
            BaseNode = null;//Delete the entire tree

            //Adds every node back into the tree but in a balanced order
            for (int i = 0; i < balanceList.Count; i++)
            {
                AddNode(balanceList[i]);
            }
        }

        //Adds a node to tree in the proper place
        void AddNodeToTree(Node<T> newNode)
        {
            Node<T> currentNode = BaseNode;

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

        //Gets a node in the table from its value
        Node<T> GetNode(T value, Node<T> currentNode)
        {
            if (currentNode != null)
            {
                //if current nodes value is equal to value return current node
                if (currentNode.Value.CompareTo(value) == 0)
                {
                    return currentNode;
                }
                //if current nodes left childs value is equal to value return current nodes left child
                if (currentNode.Left != null && currentNode.Left.Value.CompareTo(value) == 0)
                {
                    return currentNode.Left;
                }
                //else if value is greater than current nodes left childs value then seach there
                else if (currentNode.Left != null && value.CompareTo(currentNode.Left.Value) == -1)
                {
                    Node<T> possibleReturn = GetNode(value, currentNode.Left);
                    //if found value return possible return
                    if (possibleReturn != null)
                    {
                        return possibleReturn;
                    }
                }
                //if current nodes right childs value is equal to value return current nodes right child
                if (currentNode.Right != null && currentNode.Right.Value.CompareTo(value) == 0)
                {
                    return currentNode.Right;
                }
                //else if value is greater than current nodes right childs value then seach there
                else if (currentNode.Right != null && value.CompareTo(currentNode.Right.Value) == 1)
                {
                    Node<T> possibleReturn = GetNode(value, currentNode.Right);
                    //if found value return possible return
                    if (possibleReturn != null)
                    {
                        return possibleReturn;
                    }
                }
            }
            return null;
        }

        //Gets the node that is all the way to the left (the node with the smallest value) starting from the current node
        Node<T> GetBottomLeftNode(Node<T> currentNode)
        {
            //if current node has a left keep searching to the left
            if (currentNode != null && currentNode.Left != null)
            {
                return GetBottomLeftNode(currentNode.Left);
            }
            return currentNode;
        }

        //Gets the node that is all the way to the right (the node with the greatest value) starting from the current node
        Node<T> FindBottomRightNode(Node<T> currentNode)
        {
            //if current node has a left keep searching to the right
            if (currentNode != null && currentNode.Right != null)
            {
                return FindBottomRightNode(currentNode.Right);
            }
            return currentNode;
        }
        
        //Gets all the nodes from the current node to the stop node
        List<T> GetNodesSortedBetweenToNodes(Node<T> currentNode, Node<T> stopNode, List<T> currentList)
        {
            if (currentNode != null)
            {
                currentList.Add(currentNode.Value);

                //if current node has a right child add its descendants
                if (currentNode.Right != null)
                {
                    currentList = GetAllDescendants(currentNode.Right, currentList);
                }
                //if current node has a parent and it is not the stop node countinue finding nodes
                if (currentNode.Parent != null && currentNode != stopNode)
                {
                    currentList = GetNodesSortedBetweenToNodes(currentNode.Parent, stopNode, currentList);
                }
            }
            return currentList;
        }

        //Gets all the descendants of the current node
        List<T> GetAllDescendants(Node<T> currentNode, List<T> currentList)
        {
            if (currentNode != null)
            {
                //if current node has a left child get nodes from current nodes left ost node to current nodes left child
                if (currentNode.Left != null)
                {
                    currentList = GetNodesSortedBetweenToNodes(GetBottomLeftNode(currentNode.Left), currentNode.Left, currentList);
                }

                currentList.Add(currentNode.Value);

                //if current node has a right child get all of current nodes right child descendants
                if (currentNode.Right != null)
                {
                    currentList = GetAllDescendants(currentNode.Right, currentList);
                }
            }
            return currentList;
        }

        //returns a list of nodes in a balanced order
        List<T> balanceSort(List<T> sortedList, List<T> currentList)
        {
            //if the sorted tlist isn't empty
            if (sortedList.Count > 0)
            {
                List<List<T>> nextLists = new List<List<T>>();//a list of the next lists to be added to the current list

                //Removes the middle item from the sorted list and adds the elements to the left and right of it to next lists
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

                //balances the next lists to get a complete balanced list
                foreach (List<T> list in nextLists)
                {
                    currentList = (balanceSort(list, currentList));
                }
            }
            return currentList;
        }
    }
}
