﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoDo.RSS.MIF.Prds.Comm;
using GeoDo.RSS.MIF.Core;
using GeoDo.RSS.Core.DF;
using System.Drawing;
using System.IO;

namespace GeoDo.RSS.MIF.Prds.VGT
{
    public class SubProductRasterVgtEvi : CmaVgtMonitoringSubProduct
    {
        private IContextMessage _contextMessage;

        public SubProductRasterVgtEvi()
            : base()
        {
        }

        public SubProductRasterVgtEvi(SubProductDef subProductDef)
            : base(subProductDef)
        {
            _identify = subProductDef.Identify;
            _name = subProductDef.Name;
            if (subProductDef.Algorithms != null)
            {
                _algorithmDefs = subProductDef.Algorithms.ToList();
            }
        }

        public override IExtractResult Make(Action<int, string> progressTracker)
        {
            return Make(progressTracker, null);
        }

        public override IExtractResult Make(Action<int, string> progressTracker, IContextMessage contextMessage)
        {
            _contextMessage = contextMessage;
            if (_argumentProvider == null)
                return null;

            if (_argumentProvider.GetArg("AlgorithmName") == null)
            {
                PrintInfo("参数\"AlgorithmName\"为空。");
                return null;
            }
            string algorith = _argumentProvider.GetArg("AlgorithmName").ToString();
            if (algorith != "0EVI1" && algorith != "0EVI2")
            {
                PrintInfo("指定的算法\"" + algorith + "\"没有实现。");
                return null;
            }
            return CalcEVI(algorith, progressTracker);
        }

        private IExtractResult CalcEVI(string algorithmName, Action<int, string> progressTracker)
        {
            //参数准备
            if (string.IsNullOrEmpty(algorithmName))
                return null;
            if (algorithmName == "0EVI1")
                return EVIAlgorithm1(progressTracker);
            else if (algorithmName == "0EVI2")
                return EVIAlgorithm2(progressTracker);
            return null;

        }

        private IExtractResult EVIAlgorithm2(Action<int, string> progressTracker)
        {
            int bands = _argumentProvider.DataProvider.BandCount;
            if (bands == 0 || bands == 1)
            {
                PrintInfo("请选择正确的局地文件进行计算。");
                return null;
            }
            if (_argumentProvider.GetArg("Visible") == null)
            {
                PrintInfo("参数\"Visible\"为空。");
                return null;
            }
            IBandNameRaster bandNameRaster = _argumentProvider.DataProvider as IBandNameRaster;
            int bandV = TryGetBandNo(bandNameRaster, "Visible");
            if (_argumentProvider.GetArg("NearInfrared") == null)
            {
                PrintInfo("参数\"NearInfrared\"为空。");
                return null;
            }
            int bandN = TryGetBandNo(bandNameRaster, "NearInfrared");
            if (_argumentProvider.GetArg("Blue") == null)
            {
                PrintInfo("参数\"Blue\"为空。");
                return null;
            }
            int bandB = TryGetBandNo(bandNameRaster, "Blue");
            if (bandV < 1 || bandB < 1 || bandN < 1 || bandV > bands || bandB > bands || bandN > bands)
            {
                PrintInfo("获取波段序号失败,可能是波段映射表配置错误或判识算法波段参数配置错误。");
                return null;
            }
            if (_argumentProvider.GetArg("Visible_Zoom") == null)
            {
                PrintInfo("参数\"Visible_Zoom\"为空。");
                return null;
            }
            double visibleZoom = (double)_argumentProvider.GetArg("Visible_Zoom");
            if (_argumentProvider.GetArg("NearInfrared_Zoom") == null)
            {
                PrintInfo("参数\"NearInfrared_Zoom\"为空。");
                return null;
            }
            double nearInfrared = (double)_argumentProvider.GetArg("NearInfrared_Zoom");
            if (_argumentProvider.GetArg("Blue_Zoom") == null)
            {
                PrintInfo("参数\"Blue_Zoom\"为空。");
                return null;
            }
            double blueZoom = (double)_argumentProvider.GetArg("Blue_Zoom");
            int[] bandNos = new int[] { bandV, bandN, bandB, };
            double[] bandZoom = new double[] { visibleZoom, nearInfrared, blueZoom };
            Dictionary<string, string> dic = Obj2Dic(_argumentProvider.GetArg("ArgumentSetting"));
            string nearVisiableMinStr = dic.ContainsKey("NearVisableMin") ? dic["NearVisableMin"] : string.Empty;
            string nearVisiableMaxStr = dic.ContainsKey("NearVisableMax") ? dic["NearVisableMax"] : string.Empty;
            string visibleMinStr = dic.ContainsKey("VisibleMin") ? dic["VisibleMin"] : string.Empty;
            float nearVisiableMin = float.Parse(nearVisiableMinStr);
            float nearVisiableMax = float.Parse(nearVisiableMaxStr);
            float visibleMin = float.Parse(visibleMinStr);
            float[] cloudyArgs = new float[] { visibleMin, nearVisiableMax, nearVisiableMin };
            Int16 defCloudy = (Int16)_argumentProvider.GetArg("defCloudy");
            float zoom = (ushort)_argumentProvider.GetArg("resultZoom");
            zoom = zoom == 0f ? 1000 : zoom;
            string[] aois = _argumentProvider.GetArg("AOITemplate") as string[];
            string aoiTemplate = (aois == null || aois.Length == 0) ? null : aois[0];
            string[] inputFiles = GetStringArray("RasterFile");
            if (inputFiles == null || inputFiles.Length == 0)
            {
                PrintInfo("没有设置输入数据");
                return null;
            }
            bool isFitterCloud;
            if (!bool.TryParse(_argumentProvider.GetArg("isFilterCloudy").ToString(), out isFitterCloud))
            {
                PrintInfo("是否过滤云参数设置不正确！");
                return null;
            }
            IExtractResultArray results = new ExtractResultArray("EVI");
            foreach (string inputFile in inputFiles)
            {
                //计算RVI
                IExtractResult ret = CalcEVIAlgorithm2(inputFile, bandNos, zoom, bandZoom, cloudyArgs, defCloudy, isFitterCloud, aoiTemplate, progressTracker);
                if (ret != null)
                    results.Add(ret as IExtractResultBase);
            }
            return results;
        }

        private IExtractResult CalcEVIAlgorithm2(string inputFile, int[] bandNos, float zoom, double[] bandZoom, float[] cloudyArgs, short defCloudy, bool isFitterCloud, string aoiTemplate, Action<int, string> progressTracker)
        {
            string cloudFileName = FindCloudExtractResult(inputFile);
            float g, c1, c2, l;
            try
            {
                g = (float)_argumentProvider.GetArg("G");
                c1 = (float)_argumentProvider.GetArg("C1");
                c2 = (float)_argumentProvider.GetArg("C2");
                l = (float)_argumentProvider.GetArg("L");
            }
            catch (Exception)
            {
                PrintInfo("计算参数获取失败，请重新设置！");
                return null;
            }
            bool isAppCloud;
            if (!bool.TryParse(_argumentProvider.GetArg("isAppCloud").ToString(), out isAppCloud))
            {
                PrintInfo("合并交互云参数设置不正确！");
                return null;
            }
            IRasterDataProvider clmPrd = null;
            RasterProcessModel<short, short> rfr = null;
            List<RasterMaper> fileIns = new List<RasterMaper>();
            RasterMaper[] fileOuts = null;
            try
            {
                //输入数据(LDF)
                IRasterDataProvider inRaster = RasterDataDriver.Open(inputFile) as IRasterDataProvider;
                if (inRaster == null)
                {
                    PrintInfo("读取栅格文件失败：" + inRaster);
                    return null;
                }
                //输出数据(EVI)
                string outFileName = GetFileName(new string[] { inRaster.fileName }, _subProductDef.ProductDef.Identify, _identify, ".dat", null);
                IRasterDataDriver dd = RasterDataDriver.GetDriverByName("MEM") as IRasterDataDriver;
                string mapInfo = inRaster.CoordEnvelope.ToMapInfoString(new Size(inRaster.Width, inRaster.Height));
                string[] opts = new string[]{
                "INTERLEAVE=BSQ",
                "VERSION=MEM",
                "WITHHDR=TRUE",
                "SPATIALREF=" + inRaster.SpatialRef.ToProj4String(),
                mapInfo};
                RasterDataProvider outRaster = dd.Create(outFileName, inRaster.Width, inRaster.Height, 1, enumDataType.Int16, opts) as RasterDataProvider;
                string clmFile = GetClmFile(inputFile);
                int cloudCH = GetCloudCHNO();
                //栅格数据映射
                fileIns.Add(new RasterMaper(inRaster, bandNos));
                if (isAppCloud)
                {
                    if (!string.IsNullOrEmpty(clmFile) && File.Exists(clmFile))
                    {
                        clmPrd = GeoDataDriver.Open(clmFile) as IRasterDataProvider;
                        if (clmPrd.BandCount < cloudCH)
                        {
                            PrintInfo("请选择正确的云数据通道进行计算.");
                            isAppCloud = false;
                        }
                        else
                            fileIns.Add(new RasterMaper(clmPrd, new int[] { cloudCH }));
                    }
                    else
                        isAppCloud = false;
                }
                RasterMaper fileOut = new RasterMaper(outRaster, new int[] { 1 });
                fileOuts = new RasterMaper[] { fileOut };
                //创建处理模型
                rfr = new RasterProcessModel<short, short>(progressTracker);
                rfr.SetRaster(fileIns.ToArray(), fileOuts);
                rfr.SetTemplateAOI(aoiTemplate);
                rfr.RegisterCalcModel(new RasterCalcHandler<short, short>((rvInVistor, rvOutVistor, aoi) =>
                {
                    if (rvInVistor[0].RasterBandsData != null)
                    {
                        if (rvInVistor == null)
                            return;
                        short[] inBand0 = rvInVistor[0].RasterBandsData[0];//第1个输入文件的第1个波段的各像素值
                        short[] inBand1 = rvInVistor[0].RasterBandsData[1];//第1个输入文件的第2个波段的各像素值
                        short[] inBand2 = rvInVistor[0].RasterBandsData[2];//第1个输入文件的第3个波段的各像素值
                        short[] inBand3 = rvInVistor[1].RasterBandsData[0];//第2个输入文件的第1个波段的各像素值
                        short[] ndvi = new short[inBand0.Length];
                        if (string.IsNullOrWhiteSpace(aoiTemplate))
                        {
                            for (int index = 0; index < inBand0.Length; index++)
                            {
                                if ((isAppCloud && inBand3[index] == 1) || (isFitterCloud && inBand0[index] / bandZoom[0] >= cloudyArgs[0] && inBand1[index] / inBand0[index] < cloudyArgs[1] && inBand1[index] / inBand0[index] >= cloudyArgs[2]))
                                {
                                    rvOutVistor[0].RasterBandsData[0][index] = defCloudy;
                                    continue;
                                }
                                //第1个输出文件的第1个波段存储EVI值
                                rvOutVistor[0].RasterBandsData[0][index] = (short)(g * (float)(inBand1[index] / bandZoom[1] - inBand0[index] / bandZoom[0]) / (inBand1[index] / bandZoom[1] + c1
                          * inBand0[index] / bandZoom[0] - c2 * inBand2[index] / bandZoom[2] + l) * zoom);
                            }
                        }
                        else if (aoi != null && aoi.Length != 0)
                        {
                            int index;
                            for (int i = 0; i < aoi.Length; i++)
                            {
                                index = aoi[i];
                                if ((isAppCloud && inBand3[index] == 1) || (isFitterCloud && inBand0[index] / bandZoom[0] >= cloudyArgs[0] && inBand1[index] / inBand0[index] < cloudyArgs[1] && inBand1[index] / inBand0[index] >= cloudyArgs[2]))
                                {
                                    rvOutVistor[0].RasterBandsData[0][index] = defCloudy;
                                    continue;
                                }
                                //第1个输出文件的第1个波段存储EVI值
                                rvOutVistor[0].RasterBandsData[0][index] = (short)(g * (float)(inBand1[index] / bandZoom[1] - inBand0[index] / bandZoom[0]) / (inBand1[index] / bandZoom[1] + c1
                          * inBand0[index] / bandZoom[0] - c2 * inBand2[index] / bandZoom[2] + l) * zoom);
                            }
                        }
                    }
                }));
                //执行
                rfr.Excute();
                FileExtractResult res = new FileExtractResult(_subProductDef.Identify, outFileName, true);
                res.SetDispaly(false);
                return res;
            }
            finally
            {
                if (fileIns != null)
                    for (int i = 0; i < fileIns.Count; i++)
                        fileIns[i].Raster.Dispose();
                if (fileOuts != null)
                    for (int i = 0; i < fileOuts.Length; i++)
                        fileOuts[i].Raster.Dispose();
                if (clmPrd != null)
                    clmPrd.Dispose();
            }
        }

        private IExtractResult EVIAlgorithm1(Action<int, string> progressTracker)
        {
            int bands = _argumentProvider.DataProvider.BandCount;
            if (bands == 0 || bands == 1)
            {
                PrintInfo("请选择正确的局地文件进行计算。");
                return null;
            }
            if (_argumentProvider.GetArg("Visible") == null)
            {
                PrintInfo("参数\"Visible\"为空。");
                return null;
            }
            IBandNameRaster bandNameRaster = _argumentProvider.DataProvider as IBandNameRaster;
            int bandV = TryGetBandNo(bandNameRaster, "Visible");
            if (_argumentProvider.GetArg("NearInfrared") == null)
            {
                PrintInfo("参数\"NearInfrared\"为空。");
                return null;
            }
            int bandN = TryGetBandNo(bandNameRaster, "NearInfrared");
            if (_argumentProvider.GetArg("Blue") == null)
            {
                PrintInfo("参数\"Blue\"为空。");
                return null;
            }
            int bandB = TryGetBandNo(bandNameRaster, "Blue");
            if (bandV < 1 || bandB < 1 || bandN < 1 || bandV > bands || bandB > bands || bandN > bands)
            {
                PrintInfo("获取波段序号失败,可能是波段映射表配置错误或判识算法波段参数配置错误。");
                return null;
            }
            int bandMid = TryGetBandNo(bandNameRaster, "MiddInfrared"); //中红外
            int bandFar = TryGetBandNo(bandNameRaster, "FarInfrared11");//远红外
            if (_argumentProvider.GetArg("Visible_Zoom") == null)
            {
                PrintInfo("参数\"Visible_Zoom\"为空。");
                return null;
            }
            double visibleZoom = (double)_argumentProvider.GetArg("Visible_Zoom");
            if (_argumentProvider.GetArg("NearInfrared_Zoom") == null)
            {
                PrintInfo("参数\"NearInfrared_Zoom\"为空。");
                return null;
            }
            double nearInfrared = (double)_argumentProvider.GetArg("NearInfrared_Zoom");
            if (_argumentProvider.GetArg("Blue_Zoom") == null)
            {
                PrintInfo("参数\"Blue_Zoom\"为空。");
                return null;
            }
            double blueZoom = (double)_argumentProvider.GetArg("Blue_Zoom");
            double middZoom = (double)_argumentProvider.GetArg("MiddInfrared_Zoom");
            double farZoom = (double)_argumentProvider.GetArg("FarInfrared11_Zoom");
            int[] bandNos = new int[] { bandV, bandN, bandB, bandMid, bandFar };
            double[] bandZoom = new double[] { visibleZoom, nearInfrared, blueZoom, middZoom, farZoom };
            float NearInfraredCLMMin = float.Parse(_argumentProvider.GetArg("NearInfraredCLMMin").ToString());
            float FarInfrared11CLMMax = float.Parse(_argumentProvider.GetArg("FarInfrared11CLMMax").ToString());
            float FarInfrared1112CLMMin = float.Parse(_argumentProvider.GetArg("FarInfrared1112CLMMin").ToString());
            float[] cloudyArgs = new float[] { NearInfraredCLMMin, FarInfrared11CLMMax, FarInfrared1112CLMMin };
            Int16 defCloudy = (Int16)_argumentProvider.GetArg("defCloudy");
            float zoom = (ushort)_argumentProvider.GetArg("resultZoom");
            zoom = zoom == 0f ? 1000 : zoom;
            string[] aois = _argumentProvider.GetArg("AOITemplate") as string[];
            string aoiTemplate = (aois == null || aois.Length == 0) ? null : aois[0];
            string[] inputFiles = GetStringArray("RasterFile");
            if (inputFiles == null || inputFiles.Length == 0)
            {
                PrintInfo("没有设置输入数据");
                return null;
            }
            bool isFitterCloud;
            if (!bool.TryParse(_argumentProvider.GetArg("isFilterCloudy").ToString(), out isFitterCloud))
            {
                PrintInfo("是否过滤云参数设置不正确！");
                return null;
            }
            IExtractResultArray results = new ExtractResultArray("EVI");
            foreach (string inputFile in inputFiles)
            {
                //计算RVI
                IExtractResult ret = CalcEVIAlgorithm1(inputFile, bandNos, zoom, bandZoom, cloudyArgs, defCloudy, isFitterCloud, aoiTemplate, progressTracker);
                if (ret != null)
                    results.Add(ret as IExtractResultBase);
            }
            return results;
        }

        private IExtractResult CalcEVIAlgorithm1(string inputFile, int[] bandNos, float zoom, double[] bandZoom, float[] cloudyArgs, Int16 defCloudy, bool isFitterCloud, string aoiTemplate, Action<int, string> progressTracker)
        {
            string cloudFileName = FindCloudExtractResult(inputFile);
            float g, c1, c2, l;
            try
            {
                g = (float)_argumentProvider.GetArg("G");
                c1 = (float)_argumentProvider.GetArg("C1");
                c2 = (float)_argumentProvider.GetArg("C2");
                l = (float)_argumentProvider.GetArg("L");
            }
            catch (Exception)
            {
                PrintInfo("计算参数获取失败，请重新设置！");
                return null;
            }
            bool isAppCloud;
            if (!bool.TryParse(_argumentProvider.GetArg("isAppCloud").ToString(), out isAppCloud))
            {
                PrintInfo("合并交互云参数设置不正确！");
                return null;
            }
            IRasterDataProvider clmPrd = null;
            RasterProcessModel<short, short> rfr = null;
            List<RasterMaper> fileIns = new List<RasterMaper>();
            RasterMaper[] fileOuts = null;
            try
            {
                //输入数据(LDF)
                IRasterDataProvider inRaster = RasterDataDriver.Open(inputFile) as IRasterDataProvider;
                if (inRaster == null)
                {
                    PrintInfo("读取栅格文件失败：" + inRaster);
                    return null;
                }
                //输出数据(EVI)
                string outFileName = GetFileName(new string[] { inRaster.fileName }, _subProductDef.ProductDef.Identify, _identify, ".dat", null);
                IRasterDataDriver dd = RasterDataDriver.GetDriverByName("MEM") as IRasterDataDriver;
                string mapInfo = inRaster.CoordEnvelope.ToMapInfoString(new Size(inRaster.Width, inRaster.Height));
                string[] opts = new string[]{
                "INTERLEAVE=BSQ",
                "VERSION=MEM",
                "WITHHDR=TRUE",
                "SPATIALREF=" + inRaster.SpatialRef.ToProj4String(),
                mapInfo};
                RasterDataProvider outRaster = dd.Create(outFileName, inRaster.Width, inRaster.Height, 1, enumDataType.Int16, opts) as RasterDataProvider;
                string clmFile = GetClmFile(inputFile);
                int cloudCH = GetCloudCHNO();
                //栅格数据映射
                fileIns.Add(new RasterMaper(inRaster, bandNos));
                if (isAppCloud)
                {
                    if (!string.IsNullOrEmpty(clmFile) && File.Exists(clmFile))
                    {
                        clmPrd = GeoDataDriver.Open(clmFile) as IRasterDataProvider;
                        if (clmPrd.BandCount < cloudCH)
                        {
                            PrintInfo("请选择正确的云数据通道进行计算.");
                            isAppCloud = false;
                        }
                        else
                            fileIns.Add(new RasterMaper(clmPrd, new int[] { cloudCH }));
                    }
                    else
                        isAppCloud = false;
                }

                RasterMaper fileOut = new RasterMaper(outRaster, new int[] { 1 });
                fileOuts = new RasterMaper[] { fileOut };
                //创建处理模型
                rfr = new RasterProcessModel<short, short>(progressTracker);
                rfr.SetRaster(fileIns.ToArray(), fileOuts);
                rfr.SetTemplateAOI(aoiTemplate);
                rfr.RegisterCalcModel(new RasterCalcHandler<short, short>((rvInVistor, rvOutVistor, aoi) =>
                {
                    if (rvInVistor[0].RasterBandsData != null)
                    {
                        if (rvInVistor == null)
                            return;
                        short[] inBand0 = rvInVistor[0].RasterBandsData[0];//第1个输入文件的第1个波段的各像素值
                        short[] inBand1 = rvInVistor[0].RasterBandsData[1];//第1个输入文件的第2个波段的各像素值
                        short[] inBand2 = rvInVistor[0].RasterBandsData[2];//第1个输入文件的第3个波段的各像素值
                        short[] inBand3 = rvInVistor[0].RasterBandsData[3];//第1个输入文件的第4个波段的各像素值
                        short[] inBand4 = rvInVistor[0].RasterBandsData[4];//第1个输入文件的第5个波段的各像素值
                        short[] inBand5 = isAppCloud ? rvInVistor[1].RasterBandsData[0] : null;//第2个输入文件的第1个波段的各像素值
                        short[] ndvi = new short[inBand0.Length];
                        if (string.IsNullOrWhiteSpace(aoiTemplate))
                        {
                            for (int index = 0; index < inBand0.Length; index++)
                            {
                                if ((isAppCloud && inBand5[index] == 1) || (isFitterCloud && inBand1[index] / bandZoom[1] > cloudyArgs[0] && inBand4[index] / bandZoom[4] < cloudyArgs[1] && Math.Abs(inBand3[index] / bandZoom[3] - inBand4[index] / bandZoom[4]) > cloudyArgs[2]))
                                {
                                    rvOutVistor[0].RasterBandsData[0][index] = defCloudy;
                                    continue;
                                }
                                //第1个输出文件的第1个波段存储EVI值
                                rvOutVistor[0].RasterBandsData[0][index] = (short)(g * (float)(inBand1[index] / bandZoom[1] - inBand0[index] / bandZoom[0]) / (inBand1[index] / bandZoom[1] + c1
                          * inBand0[index] / bandZoom[0] - c2 * inBand2[index] / bandZoom[2] + l) * zoom);
                            }
                        }
                        else if (aoi != null && aoi.Length != 0)
                        {
                            int index;
                            for (int i = 0; i < aoi.Length; i++)
                            {
                                index = aoi[i];
                                if ((isAppCloud && inBand5[index] == 1) || (isFitterCloud && inBand1[index] / bandZoom[1] > cloudyArgs[0] && inBand4[index] / bandZoom[4] < cloudyArgs[1] && Math.Abs(inBand3[index] / bandZoom[3] - inBand4[index] / bandZoom[4]) > cloudyArgs[2]))
                                {
                                    rvOutVistor[0].RasterBandsData[0][index] = defCloudy;
                                    continue;
                                }
                                //第1个输出文件的第1个波段存储EVI值
                                rvOutVistor[0].RasterBandsData[0][index] = (short)(g * (float)(inBand1[index] / bandZoom[1] - inBand0[index] / bandZoom[0]) / (inBand1[index] / bandZoom[1] + c1
                          * inBand0[index] / bandZoom[0] - c2 * inBand2[index] / bandZoom[2] + l) * zoom);
                            }
                        }
                    }
                }));
                //执行
                rfr.Excute();
                FileExtractResult res = new FileExtractResult(_subProductDef.Identify, outFileName, true);
                res.SetDispaly(false);
                return res;
            }
            finally
            {
                if (fileIns != null)
                    for (int i = 0; i < fileIns.Count; i++)
                        fileIns[i].Raster.Dispose();
                if (fileOuts != null)
                    for (int i = 0; i < fileOuts.Length; i++)
                        fileOuts[i].Raster.Dispose();
                if (clmPrd != null)
                    clmPrd.Dispose();
            }
        }

        private void PrintInfo(string info)
        {
            if (_contextMessage != null)
                _contextMessage.PrintMessage(info);
            else
                Console.WriteLine(info);
        }

        #region 增加交互云修正
        private string GetClmFile(string fname)
        {
            RasterIdentify rid = new RasterIdentify(Path.GetFileName(fname));
            rid.ProductIdentify = "VGT";
            rid.SubProductIdentify = "0CLM";
            string clmFile = rid.ToWksFullFileName(".dat");
            if (File.Exists(clmFile))
                return clmFile;
            return null;
        }

        private int GetCloudCHNO()
        {
            int cloudCH;
            if (_argumentProvider.GetArg("cloudCH") == null)
                return 1;
            if (string.IsNullOrEmpty(_argumentProvider.GetArg("cloudCH").ToString()))
                return 1;
            if (int.TryParse(_argumentProvider.GetArg("cloudCH").ToString(), out cloudCH))
                return cloudCH;
            else
                return 1;

        }
        #endregion

    }
}
