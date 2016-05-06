using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GraphDB
{
    public partial class Form1 : Form
    {
        GraphDataBase gdb;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            gdb = new GraphDataBase();
            gdb.OpenDataBase("0.xml", ref err);

            gdb.AddNodeData("秦国", "国家", ref err, "King=YingZheng");
            gdb.AddNodeData("关中", "地区", ref err, "Population=1000000, Army=10000");
            gdb.AddNodeData("陇西", "地区", ref err, "Population=1000000, Army=10000");
            gdb.AddNodeData("河西", "地区", ref err, "Population=500000, Army=5000");
            gdb.AddEdgeData("秦国", "国家", "关中", "地区", "统治", ref err);
            gdb.AddEdgeData("秦国", "国家", "陇西", "地区", "统治", ref err);
            gdb.AddEdgeData("秦国", "国家", "河西", "地区", "统治", ref err);
            gdb.AddEdgeData("陇西", "地区", "关中", "地区", "连通", ref err);
            gdb.AddEdgeData("关中", "地区", "陇西", "地区", "连通", ref err);
            gdb.AddEdgeData("河西", "地区", "关中", "地区", "连通", ref err);
            gdb.AddEdgeData("关中", "地区", "河西", "地区", "连通", ref err);
            //gdb.ModifyNodeData("关中", "地区", ModifyOperation.Delete, "Population=2000000, Army=20000", ref err);
            //gdb.ModifyEdgeData("秦国", "国家", "关中", "地区", "统治", "0.8", ref err);
            //gdb.RemoveEdgeData("秦国", "国家", "关中", "地区", "统治", ref err);
            gdb.RemoveNodeData("关中", "地区", ref err);
        }
    }
}
