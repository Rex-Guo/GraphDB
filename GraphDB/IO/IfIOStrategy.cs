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
        Graph ReadFile(string sPath, ref ErrorCode err);
        void SaveFile(XmlDocument doc, string sPath, ref ErrorCode err);
    }
}
