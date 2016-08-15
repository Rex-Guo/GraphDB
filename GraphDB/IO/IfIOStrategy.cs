using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphDB.Core;
using System.Xml;

namespace GraphDB.IO
{
    public interface IfIOStrategy//文件读写算法接口
    {
        string Path { get; set; }
        Graph ReadFile(ref ErrorCode err);
        void SaveFile(XmlDocument doc, ref ErrorCode err);
    }
}
