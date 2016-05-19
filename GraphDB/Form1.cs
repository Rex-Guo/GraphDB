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
            gdb.OpenDataBase("1.xml", ref err);

            gdb.DataQueryExecute("START node('*-国家') MATCH (Kingdom)-[:统治]->(District)<-[:连通 5..5]-(Neibhour) WHERE * RETURN Kingdom.Name, District.*", ref err);//
        }
    }
}
