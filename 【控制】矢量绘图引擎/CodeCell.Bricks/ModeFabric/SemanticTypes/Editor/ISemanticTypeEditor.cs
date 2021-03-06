﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCell.Bricks.ModelFabric
{
    public interface ISemanticTypeEditor
    {
        object GetValue(object sender);
        bool TryParse(string text,out object value);
        bool IsNeedInput { get; }
        string ToString(object value);
        Type DataType { get; set; }
    }
}
