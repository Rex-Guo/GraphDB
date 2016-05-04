using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using GraphDB.Core;
using GraphDB.IO;

namespace GraphDB
{
    public partial class Form1 : Form
    {
        Graph empire;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IfIOStrategy IOhandler = new XMLStrategy();
            empire = IOhandler.ReadFile("0.xml");
            XmlDocument doc;
            doc = empire.ToXML();
            IOhandler.SaveFile(doc, "1.xml");
        }
    }
}
