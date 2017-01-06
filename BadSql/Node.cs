using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    public class Node<T> 
    {
        public T Value { get; set; }
        public Node<T> Left { get; set; }
        public Node<T> Right { get; set; }
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

        public bool isRight(Node<T> node)
        {
            if(Right != null && node.Value.Equals(Right.Value))
            {
                return true;
            }
            return false;
        }
        
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
        public void DeleteChild(Node<T> child)
        {
            if (isRight(child))
            {
                DeleteChild(true);
            }
            else
            {
                DeleteChild(false);
            }
        }
        public int Weight()
        {
            return GetWeight(this, 0);
        }
        int GetWeight(Node<T> currentNode, int weight)
        {
            int retrunWeight = weight;
            if (currentNode.Left != null)
            {
                retrunWeight = GetWeight(currentNode.Left, retrunWeight);
                retrunWeight++;
            }
            if (currentNode.Right != null)
            {
                retrunWeight = GetWeight(currentNode.Right, retrunWeight);
                retrunWeight++;
            }
            return retrunWeight;
        }
        public int LongestPathCount()
        {
            return GetLengthOfPath(this, 0);
        }
        int GetLengthOfPath(Node<T> currentNode, int length)
        {
            int leftLength = length;
            int rightLength = length;
            if (currentNode.Left != null)
            {
                leftLength = GetLengthOfPath(currentNode.Left, leftLength);
                leftLength++;
            }
            if (currentNode.Right != null)
            {
                rightLength = GetLengthOfPath(currentNode.Right, rightLength);
                rightLength++;
            }
            if(leftLength > rightLength)
            {
                return leftLength;
            }
            return rightLength;
        }

        //public Node<T> GetFarthestChild()
        //{
        //    return GetFarthestChildRecursive(this, 0);
        //}
        //Node<T> GetFarthestChildRecursive(Node<T> currentNode, int length)
        //{
        //    if(cu)
        //}
    }
}
