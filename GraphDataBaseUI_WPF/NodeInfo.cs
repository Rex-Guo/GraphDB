using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GraphDB.Core;

namespace GraphDataBaseUI_WPF
{
    class NodeInfo:INotifyPropertyChanged
    {
        string nodeName;
        string nodeType;
        List<NodeProperty> Attribute;

        public string Name
        {
            get
            {
                return nodeName;
            }
        }
        public string Type
        {
            get
            {
                return nodeType;
            }
        }
        public List<NodeProperty> Properties
        {
            get
            {
                return Attribute;
            }
        }

        public NodeInfo()
        {
            this.nodeName = "";
            this.nodeType = "";
            Attribute = new List<NodeProperty>();
        }

        public NodeInfo(Node oriNode)
        {
            this.nodeName = string.Copy(oriNode.Name);
            this.nodeType = string.Copy(oriNode.Type);
            Attribute = new List<NodeProperty>();
            foreach (NodeProperty np in oriNode.Properties)
            {
                Attribute.Add(new NodeProperty(string.Copy(np.Key), string.Copy(np.Value)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }
}
