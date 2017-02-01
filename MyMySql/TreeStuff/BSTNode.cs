using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMySql
{
    //A Node in a binary tree
    public class BSTNode<T> 
    {
        public T Value { get; set; }
        public BSTNode<T> Left { get; set; }//Child to Left (A Node with a value less than this nodes value)
        public BSTNode<T> Right { get; set; }//Child to Right (A Node with a value greater than this nodes value)
        public BSTNode<T> Parent { get; set; }
        public bool IsVisited { get; set; }

        /// <summary>
        /// Default constructor that initializes all the variables and sets value
        /// </summary>
        /// <param name="value">The data of this node</param>
        public BSTNode(T value)
        {
            Value = value;
            Left = null;
            Right = null;
            IsVisited = false;
            Parent = null;
        }

        /// <summary>
        /// Constructor that sets value, parent and initializes the other variables
        /// </summary>
        /// <param name="value">The data of this node</param>
        /// <param name="parent">The parent of this node</param>
        public BSTNode(T value, BSTNode<T> parent)
        {
            Value = value;
            Left = null;
            Right = null;
            IsVisited = false;
            Parent = parent;
        }

        /// <summary>
        /// Constructor that sets value, left, right and initializes the other variables
        /// </summary>
        /// <param name="value">The data of this node</param>
        /// <param name="left">The left child of this node</param>
        /// <param name="right">the right child of this node</param>
        public BSTNode(T value, BSTNode<T> left, BSTNode<T> right)
        {
            Value = value;
            Left = left;
            Right = Right;
            Parent = null;
        }
        
        /// <summary>
        /// if a node is this nodes left child or right child
        /// </summary>
        /// <param name="node">The child of this node</param>
        /// <returns>True if the node is this nodes right child else it returns false</returns>
        public bool isRight(BSTNode<T> node)
        {
            if(Right != null && node.Value.Equals(Right.Value))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Deletes right or left child
        /// </summary>
        /// <param name="right">If the child is the left or right child</param>
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
        
        /// <summary>
        /// Deletes child
        /// </summary>
        /// <param name="child">Child to be deleted</param>
        public void DeleteChild(BSTNode<T> child)
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

        /// <summary>
        /// Gets the length of the longest path of nodes from this node
        /// </summary>
        /// <returns>The length of the longest path of nodes</returns>
        public int LongestPathCount()
        {
            return GetLengthOfPath(this, 0);
        }

        /// <summary>
        /// Recusively gets the length of the longest path of nodes from current node
        /// </summary>
        /// <param name="currentNode">The current node being recused through</param>
        /// <param name="currentLength">Teh current length of the longest path</param>
        /// <returns>The length of the longest path of nodes</returns>
        int GetLengthOfPath(BSTNode<T> currentNode, int currentLength)
        {
            int leftLength = currentLength;
            int rightLength = currentLength;
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
