﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GeoDo.RSS.MIF.Core;

namespace GeoDo.RSS.MIF.Prds.DRT
{
    public partial class UCVTIExpCoefficient : UserControl,IArgumentEditorUI
    {
        public UCVTIExpCoefficient()
        {
            InitializeComponent();
            ucExpCoefficientBase1.SetExpType("VTI");
        }

        public object GetArgumentValue()
        {
            return ucExpCoefficientBase1.GetArgumentValue();
        }

        public void SetChangeHandler(Action<object> handler)
        {
            ucExpCoefficientBase1.SetChangeHandler(handler);
        }


        public object ParseArgumentValue(System.Xml.Linq.XElement ele)
        {
            return null;
        }
    }
}
