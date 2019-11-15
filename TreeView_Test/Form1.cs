using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace TreeView_Test
{
    public partial class Form1 : Form
    {
        // 用于记录在treeView当前选定的目录的信息
        private Entity myCurrentDirectory = null;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗口加载页面处理函数：
        /// 初始化listView的列名；
        /// 在treeView中添加驱动器的结点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // 给listview增加列名
            this.listView1.View = View.Details;
            this.listView1.FullRowSelect = true;    // 点击每行的item，选中一整行
            this.listView1.GridLines = true;
            this.listView1.SmallImageList = this.imageList1;   // 绑定图标
            ColumnHeader ch = new ColumnHeader();   // 添加列名：“名称”、“修改日期”、“类型”、“大小”
            ch.Text = "名称";
            ch.Width = 240;
            ch.TextAlign = HorizontalAlignment.Left;
            this.listView1.Columns.Add(ch);
            this.listView1.Columns.Add("修改日期", 240, HorizontalAlignment.Left);
            this.listView1.Columns.Add("类型", 240, HorizontalAlignment.Left);
            this.listView1.Columns.Add("大小", 180, HorizontalAlignment.Left);

            string[] drivers = Environment.GetLogicalDrives();     // 获取到当前计算机的所有盘符，此时的盘符是string
            for (int i = 0; i < drivers.Length; i++)        // 对于每一个驱动器，进行左侧树添加结点
            {
                this.treeView1.Nodes.Add(new TreeNode(drivers[i]));
            }
        }

        /// <summary>
        /// 获取路径下面的所有子元素
        /// </summary>
        /// <param name="path">目录的绝对路径</param>
        /// <param name="papaNode">路径的父母</param>
        /// <returns>子元素的集合（数组）</returns>
        private FileSystemInfo[] getAllChildren(string path)
        {            
            DriveInfo driveInfo = new DriveInfo(path);
            if (!driveInfo.IsReady)
            {
                return null;
            }
            if (!Directory.Exists(path))
            {
                return null;
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] myChildren = null;
            try
            {
                myChildren = dir.GetFileSystemInfos(); // 盘符下面的所有子文件+子目录
            }
            catch (Exception e)
            {
                return null;
            }
            return myChildren;
        }

        /// <summary>
        /// treeView的结点单击事件处理函数：
        /// 在treeView中获取到目录的绝对路径
        /// 获取到所有的子元素，在treeView和listView中显示出来
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)  // 用于处理单击事件
        {
            if (this.myCurrentDirectory == null)
            {
                this.myCurrentDirectory = new Entity();
            }
            TreeNode testNode = this.treeView1.SelectedNode;        // 在treeView中点击相关的目录
            string path = testNode.FullPath;                        //获取目录所在的绝对路径
            this.myCurrentDirectory.setNode(testNode);
            this.myCurrentDirectory.setPath(path + @"\");

            FileSystemInfo[] children = this.getAllChildren(path);    // 获取路径下面的所有子元素
            this.toAddToTree(testNode, children);                   // 添加子目录的treeNode到treeView中            
            this.toShowInTheWin(children);                          // 在listView中显示出所有的子元素            
            this.Result.Text += "此时打开的文件的路径是：" + testNode.FullPath + "\r\n";        // 添加操作结果信息
            this.CurrentDir.Text = this.myCurrentDirectory.getPath();
        }
       
        /// <summary>
        /// 判断treeView的目录是否为空
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool ifCanExpand(TreeNode node) // 如果是可以展开返回true，如果是不可展开返回false
        {
            string path = node.FullPath;
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] myChildren = null;
            try
            {
                myChildren = dir.GetFileSystemInfos(); // 盘符下面的所有子文件+子目录
            }
            catch (Exception e)
            {
                return false;
            }
            if (myChildren.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 当单击了左侧的目录之后，在右侧的ListView中显示出该目录的所有子文件，包含子目录和子普通文件
        /// </summary>
        /// <param name="children">所有子文件的句柄</param>
        private void toShowInTheWin(FileSystemInfo[] children)
        {
            this.listView1.Items.Clear();
            if (children == null || children.Length == 0)
            {
                return;
            }
            this.listView1.BeginUpdate();       //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度                
            // listview的列是：文件名，最后修改日期，类型，大小
            for (int i = 0; i < children.Length; i++)
            {                 
                ListViewItem lvi = new ListViewItem();
                lvi.ImageIndex = 0;
                lvi.Text = children[i].Name;
                lvi.SubItems.Add(children[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                //lvi.SubItems.Add(children[i].GetType().ToString());   // 先用GetType来判断是目录还是文件，如果是目录，就显示目录类型
                lvi.SubItems.Add(children[i].Extension.ToString());     // 如果不是目录，则需要通过判断后缀名来判断是什么类型，并且选择相应的图标
                lvi.SubItems.Add("未知？？？");
                this.listView1.Items.Add(lvi);
                
            }
            this.listView1.EndUpdate();   

        }

        /// <summary>
        /// 给展开后的结点添加子节点
        /// </summary>
        /// <param name="papa">当前要展开的目录</param>
        /// <param name="children">当前要展开的目录的所有子元素</param>
        private void toAddToTree(TreeNode papa, FileSystemInfo[] children)
        {
            if(children != null)
            foreach (FileSystemInfo child in children)
            {
                string tyep = child.GetType().ToString();
                if (child.GetType().ToString() == "System.IO.DirectoryInfo")    // 只添加目录到treeView中，非目录不添加
                {
                    papa.Nodes.Add(new TreeNode(child.Name));                   
                }
            }
        }

        /// <summary>
        /// listView的双击事件的处理函数：
        /// 用于处理进入相应的目录
        /// 用于打开对应的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            string curr = this.myCurrentDirectory.getPath();
            string name = this.listView1.SelectedItems[0].Text.ToString();
            string path = curr + name;            
            // 如果是目录：则打开目录
            // 如果是文件：则打开文件
            if (Directory.Exists(path)) // 是目录
            {
                this.Result.Text += "当前选定的父目录：" + curr + "\r\n";
                this.Result.Text += "T当前选定是目录：" + path + "\r\n";
                FileSystemInfo[] children = this.getAllChildren(path);    // 获取路径下面的所有子元素               
                TreeNode papaNode = this.myCurrentDirectory.getNode();     // 选定目录的父目录的结点
                this.Result.Text += papaNode.Nodes.Count + "\r\n";
                this.Result.Text += papaNode.Name +"\r\n";
                for (int i = 0; i < papaNode.Nodes.Count; i++ )
                {
                    if(name == papaNode.Nodes[i].Text){   // 找到选定文件的的Node: papaNode.Nodes[i]
                        this.toAddToTree(papaNode.Nodes[i], children);
                        papaNode.Nodes[i].Expand();
                        this.toShowInTheWin(children);
                        this.myCurrentDirectory.setPath(path + @"\");
                        this.myCurrentDirectory.setNode(papaNode.Nodes[i]);
                        this.CurrentDir.Text = this.myCurrentDirectory.getPath();
                    }
                }
            }
            else  // 不是目录
            {
                Process ardProcess = new Process();
                ardProcess.StartInfo.FileName = path;
                ardProcess.StartInfo.CreateNoWindow = true;
                //ardProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                ardProcess.Start();
            }
            
            
        }
    }
}
