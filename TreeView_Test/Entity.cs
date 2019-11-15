using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TreeView_Test
{
    class Entity
    {
        private TreeNode node;
        private string path;
        public void setNode(TreeNode n){
            this.node = n;
        }
        public void setPath(string p)
        {
            this.path = p;
        }
        public TreeNode getNode()
        {
            return this.node;
        }
        public string getPath()
        {
            return this.path;
        }
    }
}
