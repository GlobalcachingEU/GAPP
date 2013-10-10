﻿using System;
using System.Collections.Generic;
using System.Text;

namespace QiHe.CodeLib
{
    public class BinaryTreeNodeBase<TItem, TTreeNode>
        where TTreeNode : BinaryTreeNodeBase<TItem, TTreeNode>
    {
        public TItem Data;

        public TTreeNode Parent;

        public TTreeNode Left;

        public TTreeNode Right;

        public bool IsLeftChild
        {
            get { return this == Parent.Left; }
        }

        public bool IsRightChild
        {
            get { return this == Parent.Right; }
        }
    }
}
