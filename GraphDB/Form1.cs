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
            gdb.AddNodeData("关中", "地区", ref err, "Population=1000000");
            gdb.AddEdgeData("秦国", "国家", "关中", "地区", "统治", ref err);
        }
    }
}
