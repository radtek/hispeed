﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoDo.RSS.Core.DF;
using GeoDo.RSS.DI;
using GeoDo.RSS.MIF.Core;
using GeoDo.RSS.Core.UI;
using System.Windows.Forms;
using GeoDo.RSS.Core.RasterDrawing;
using System.IO;

namespace GeoDo.RSS.UI.AddIn.Theme
{
    public class ManualImportData
    {
        private ISmartSession _smartSession = null;
        private IMonitoringSession _monitoringSession = null;
        private string _defaultImpotFileDir;
        private string _argsFile;
        private string _defaultSubPro;
        private bool _defaultManual = false;

        public ManualImportData(ISmartSession smartSession,string arg)
        {
            _smartSession = smartSession;
            _monitoringSession = _smartSession.MonitoringSession == null ? null : _smartSession.MonitoringSession as MonitoringSession;
            _argsFile = arg;
        }

        public string Do()
        {
            if (_monitoringSession == null)
                return "未定义待导入哪个产品!";
            IRasterDataProvider dataProvider = GetRasterDataProvider();
            if (dataProvider == null)
                return "未选择待导入影像数据!";
            GetDefaultInfo();
            if (_monitoringSession.ActiveMonitoringSubProduct == null)
            {
                if (!string.IsNullOrEmpty(_defaultSubPro))
                    ChangedSubPro();
                else
                    return "未定义待导入哪个子产品!";
            }
            ImportFilesObj[] objs = GetImportFileObjByForm(dataProvider);
            if (objs == null)
                return "无可导入数据!";
            ApplayExtractResult(dataProvider, objs);
            return string.Empty;
        }

        private void ChangedSubPro()
        {
            _monitoringSession.ChangeActiveSubProduct(_defaultSubPro);
            ExecuteCommd(6602);
        }

        private void ExecuteCommd(int commdID)
        {
            ICommand commd = _smartSession.CommandEnvironment.Get(commdID);
            if (commd != null)
                commd.Execute();
        }

        private void ApplayExtractResult(IRasterDataProvider dataProvider, ImportFilesObj[] objs)
        {
            IDataImportDriver driver = null;
            string error = null;
            foreach (ImportFilesObj obj in objs)
            {
                if (string.IsNullOrEmpty(obj.ProIdentify) || string.IsNullOrEmpty(obj.SubProIdentify))
                {
                    obj.ProIdentify = _monitoringSession.ActiveMonitoringProduct.Identify;
                    obj.SubProIdentify = _monitoringSession.ActiveMonitoringSubProduct.Identify;
                }
                driver = DataImport.GetDriver(obj.ProIdentify, obj.SubProIdentify, obj.FullFilename, null);
                if (driver == null)
                    continue;
                IExtractResult result = driver.Do(obj.ProIdentify, obj.SubProIdentify, dataProvider, obj.FullFilename, out error);
                if (result == null)
                    continue;
                DisplayResultClass.DisplayResult(_smartSession, _monitoringSession.ActiveMonitoringSubProduct, result, true);
            }
        }

        private ImportFilesObj[] GetImportFileObjByForm(IRasterDataProvider dataProvider)
        {
            ImportFilesObj[] objs = null;
            if (objs == null || objs.Length > 1)
                using (frmDataImport frm = new frmDataImport())
                {
                    if (!string.IsNullOrEmpty(_defaultImpotFileDir))
                        frm.SetFilesDir(_defaultImpotFileDir);
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        objs = frm.ImportObjs;
                    }
                }
            return objs;
        }

        private void GetDefaultInfo()
        {
            _defaultImpotFileDir = string.Empty;
            _defaultSubPro = string.Empty;
            _defaultManual = false;;
            if (!File.Exists(_argsFile))
                return;
            string[] contexts = File.ReadAllLines(_argsFile, Encoding.Default);
            if (contexts == null || contexts.Length == 0 || contexts.Length > 3 || !Directory.Exists(contexts[0]))
                return;
            _defaultImpotFileDir = contexts[0];
            if (contexts.Length > 1)
                _defaultSubPro = contexts[1];
            if (contexts.Length > 2)
                _defaultManual = bool.Parse(contexts[2]);
        }

        private IRasterDataProvider GetRasterDataProvider()
        {
            if (_smartSession.SmartWindowManager == null)
                return null;
            ICanvasViewer viewer = _smartSession.SmartWindowManager.ActiveCanvasViewer;
            if (viewer == null)
                return null;
            IRasterDrawing drawing = viewer.ActiveObject as IRasterDrawing;
            if (drawing == null || drawing.DataProvider == null)
                return null;
            return drawing.DataProvider;
        }
    }
}
