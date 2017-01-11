using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    //A Node in a binary tree
    public class Node<T> 
    {
        public T Value { get; set; }
        public Node<T> Left { get; set; }//Child to Left (A Node with a value less than this nodes value)
        public Node<T> Right { get; set; }//Child to Right (A Node with a value greater than this nodes value)
        public Node<T> Parent { get; set; }
        public bool IsVisited { get; set; }

        public Node(T value)
        {
            Value = value;
            Left = null;
            Right = null;
            IsVisited = false;
            Parent = null;
        }
        public Node(T value, Node<T> parent)
        {
            Value = value;
            Left = null;
            Right = null;
            IsVisited = false;
            Parent = parent;
        }
        public Node(T value, Node<T> left, Node<T> right)
        {
            Value = value;
            Left = left;
            Right = Right;
            Parent = null;
        }

        //if a node is this nodes left child or right child
        public bool isRight(Node<T> node)
        {
            if(Right != null && node.Value.Equals(Right.Value))
            {
                return true;
            }
            return false;
        }
        
        //Deletes right or left child
        public void DeleteChild(bool right)
        {
            if (right)
            {
                Right = null;
            }
            else
            {
                Left = null;
            }
        }
        //Deletes child
        public void DeleteChild(Node<T> child)
        {
            //if child is the right child delete right child else delete left child
            if (isRight(child))
            {
                DeleteChild(true);
            }
            else
            {
                DeleteChild(false);
            }
        }
        
        //Gets the length of the longest path of nodes from this node
        public int LongestPathCount()
        {
            return GetLengthOfPath(this, 0);
        }
        //Gets the length of the longest path of nodes from current node
        int GetLengthOfPath(Node<T> currentNode, int currenyLength)
        {
            int leftLength = currenyLength;
            int rightLength = currenyLength;
            //if left child is not null find the length of its longest path
            if (currentNode.Left != null)
            {
                leftLength = GetLengthOfPath(currentNode.Left, leftLength);
                leftLength++;
            }
            //if right child is not null find the length of its longest path
            if (currentNode.Right != null)
            {
                rightLength = GetLengthOfPath(currentNode.Right, rightLength);
                rightLength++;
            }
            //return the longer of the to paths
            if(leftLength > rightLength)
            {
                return leftLength;
            }
            return rightLength;
        }
    }
}
