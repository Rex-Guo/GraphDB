using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphDB.Core;
using System.Xml;

namespace GraphDB.IO
{
    public interface IfIOStrategy//�ļ���д�㷨�ӿ�
    {
        string Path { get; set; }
        Graph ReadFile(ref ErrorCode err);
        void SaveFile(XmlDocument doc, ref ErrorCode err);
    }
}
