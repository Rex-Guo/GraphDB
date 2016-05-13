﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphDB
{
    //系统错误码
    public enum ErrorCode
    {
        NoError = 0,
        OpenFileFailed = 1,
        SaveFileFailed = 2,
        NoXmlRoot = 3,
        InvaildIndex = 10,
        NodeExists = 11,
        CreateNodeFailed = 12,
        NodeNotExists = 13,
        EdgeExists = 15,
        CreateEdgeFailed = 16,
        EdgeNotExists = 17,
        AddEdgeFailed = 18,
        CypherInvalid = 50,
        StartSegInvalid = 51,
        MatchSegInvalid = 52,
        WhereSegInvalid = 53,
        ReturnSegInvalid = 54,
    }
    //修改操作选项
    public enum ModifyOperation
    {
        Append = 0,
        Replace = 1,
        ReplaceAll = 2,
        Delete = 3,
    }


}
