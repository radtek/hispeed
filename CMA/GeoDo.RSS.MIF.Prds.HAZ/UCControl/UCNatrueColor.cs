﻿using System;
using System.Text;
using System.Windows.Forms;
using GeoDo.RSS.MIF.Core;
using System.IO;
using GeoDo.RSS.MIF.Prds.HAZ.UCControl;
using GeoDo.RSS.MIF.UI;

namespace GeoDo.RSS.MIF.Prds.HAZ
{
    public partial class UCNatrueColor : UserControl, IArgumentEditorUI2
    {
        private Action<object> _handle = null;
        private string txtFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MonitoringProductArgs", "HAZ", "真彩图数据路径.txt");
        string _compareDateStr = "yyMMddHH";
        private bool _isInit = false;

        public UCNatrueColor()
        {
            InitializeComponent();
            Init();
        }

        private void GetDefaultPathFromTxt()
        {
            if (!File.Exists(txtFile))
                return;
            _isInit = true;
            using (var sr = new StreamReader(txtFile, Encoding.GetEncoding("GB2312")))
            {
                string dirPath = sr.ReadLine();
                if (Directory.Exists(dirPath))
                {
                    SetDir(dirPath);
                }
            }
            _isInit = false;
        }

        private void SetDir(string dirPath)
        {
            string day = DateTime.Now.ToString("yyyyMMdd");
            txtNatrueColorDir.Text = Path.Combine(dirPath, day);
        }

        private void Init()
        {
            GetDefaultPathFromTxt();
        }

        private string NatrueColorDir { set { txtNatrueColorDir.Text = value; } get { return txtNatrueColorDir.Text; } }

        private SimpleValue FileName
        {
            set
            {
                if (value == null) return;
                txtNatrueColorFile.Text = value.ToString();
                txtNatrueColorFile.Tag = value;
            }
            get
            {
                return txtNatrueColorFile.Tag as SimpleValue;
            }
        }

        private string FileFullName
        {
            get
            {
                SimpleValue sv = FileName;
                if (sv == null) return null;
                string source = string.Format("{0}.tif", Path.Combine(NatrueColorDir, sv.ID));
                string target = string.Format("{0}.tif", Path.Combine(sv.ToString()));
                return string.Format("{0},{1}", source, target);
            }
        }

        public object GetArgumentValue()
        {
            string argument = FileFullName;
            return argument;
        }

        public void InitControl(IExtractPanel panel, ArgumentBase arg)
        {
            _isInit = true;
            txtNatrueColorDir.Tag = null;
            if (panel == null)
                return;
            UCExtractPanel ucPanel = panel as UCExtractPanel;
            if (ucPanel == null)
                return;
            IMonitoringSubProduct subProduct = ucPanel.MonitoringSubProduct;
            if (subProduct == null)
                return;
            IArgumentProvider arp = subProduct.ArgumentProvider;
            if (arp == null)
                return;
            SaveArgsFile(null,null);
            //string filename = arp.DataProvider.fileName;
            //if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
            //    return;
            //txtNatrueColorDir.Tag = filename;
            //txtDir_TextChanged(null, null);
            _isInit = false;
        }

        public bool IsExcuteArgumentValueChangedEvent
        {
            get
            {
                return false;
            }
            set
            {
                ;
            }
        }

        public object ParseArgumentValue(System.Xml.Linq.XElement ele)
        {
            return null;
        }

        public void SetChangeHandler(Action<object> handler)
        {
            _handle = handler;
            //if (_handle != null)
            //    _handle(GetArgumentValue());
        }

        //private void btnNatrueColorFile_Click(object sender, EventArgs e)
        //{
        //    using (OpenFileDialog dlg = new OpenFileDialog())
        //    {
        //        dlg.Filter = "真彩色图(*.tif)|*.tif|所有文件(*.*)|*.*";
        //        dlg.Multiselect = false;
        //        if (dlg.ShowDialog() == DialogResult.OK)
        //        {
        //            FileName = Path.GetFileName(dlg.FileName);
        //            //txtNatrueColorFile.Tag = dlg.FileName;
        //        }
        //    }
        //}

        private void SaveArgsFile(object sender, EventArgs e)
        {
            if (_handle != null)
                _handle(GetArgumentValue());
            if (!_isInit)
            {
                //保存文件路径
                SaveDefaultFile();
            }
        }

        private void SaveDefaultFile()
        {
            if (!File.Exists(txtFile))
            {
                string dir = Path.GetDirectoryName(txtFile);
                Directory.CreateDirectory(dir);
            }
            using (StreamWriter sw = new StreamWriter(txtFile, false, Encoding.GetEncoding("GB2312")))
            {
                string dir = txtNatrueColorDir.Text;
                sw.WriteLine(Path.GetDirectoryName(dir));
            }
        }

        private void btnNatrueColorDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                SetDir(folderDialog.SelectedPath);
            }
        }

        //private void SearchNatrueColorFile()
        //{
        //    if (txtNatrueColorDir.Tag == null || string.IsNullOrEmpty(txtNatrueColorDir.Text) || !Directory.Exists(txtNatrueColorDir.Text))
        //        return;
        //    RasterIdentify rid = new RasterIdentify(Path.GetFileName(txtNatrueColorDir.Tag.ToString()));
        //    if (rid == null)
        //        return;
        //    //获取最新真彩图数据的逻辑
        //    string[] files = Directory.GetFiles(txtNatrueColorDir.Text, "*" + rid.OrbitDateTime.AddHours(8).AddMinutes(30).ToString(_compareDateStr) + "*.tif", SearchOption.AllDirectories);
        //    if (files == null || files.Length == 0)
        //        return;
        //    FileName = Path.GetFileName(files[0]);
        //    //txtNatrueColorFile.Tag = files[0];
        //}

        //private void txtDir_TextChanged(object sender, EventArgs e)
        //{
        //    SearchNatrueColorFile();
        //    SaveArgsFile(null, null);
        //}

        private void txtObservationData_TextChanged(object sender, EventArgs e)
        {
            SaveArgsFile(null, null);
        }

        private void txtNatrueColorDir_TextChanged(object sender, EventArgs e)
        {
            string dir = txtNatrueColorDir.Text;
            ClearFileLst();
            if (!Directory.Exists(dir))
                return;

            var filter = "*.tif";
            var files = Directory.GetFiles(dir, filter);
            foreach (var file in files)
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                //var  item = new ListViewItem(filename) {ToolTipText = filename};
                SimpleValue sv = new SimpleValue { ID = filename, Name = filename };
                lstFiles.Items.Add(sv);
            }
            if (lstFiles.Items.Count > 0)
                lstFiles.SelectedIndex = 0;
        }

        private class SimpleValue
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        private void ClearFileLst()
        {
            lstFiles.Items.Clear();
            FileName = null;
        }

        private void lstFiles_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            foreach (var selectedItem in lstFiles.SelectedItems)
            {
                SimpleValue sv = selectedItem as SimpleValue;
                FileName = sv;
                SaveArgsFile(null,null);
                break;
            }
        }

        private void lstFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            var sv = lstFiles.SelectedItem as SimpleValue;
            if (sv == null) return;
            FormNewFileName formNewFile = new FormNewFileName();
            formNewFile.FileName = sv.Name;
            if (formNewFile.ShowDialog() == DialogResult.OK)
            {
                sv.Name = formNewFile.FileName;
                //lstFiles.SelectedItem = sv;
                //FileName = sv;

                int index = lstFiles.SelectedIndex;
                lstFiles.Items.RemoveAt(index);
                lstFiles.Items.Insert(index, sv);
                lstFiles.SelectedIndex = index;
            }
        }
    }
}
