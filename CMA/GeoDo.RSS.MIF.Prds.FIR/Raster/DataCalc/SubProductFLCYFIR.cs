﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoDo.RSS.MIF.Prds.Comm;
using GeoDo.RSS.MIF.Core;
using System.IO;
using GeoDo.RSS.Core.DF;
using System.Drawing;

namespace GeoDo.RSS.MIF.Prds.FIR
{
    public class SubProductFLCYFIR : CmaMonitoringSubProduct
    {
        private IContextMessage _contextMessage = null;

        public SubProductFLCYFIR()
            : base()
        {

        }

        public SubProductFLCYFIR(SubProductDef subProductDef)
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
            if (algorith != "FLCY")
            {
                PrintInfo("指定的算法\"" + algorith + "\"没有实现。");
                return null;
            }
            return CalcAnmi(progressTracker);
        }

        private IExtractResult CalcAnmi(Action<int, string> progressTracker)
        {
            if (_argumentProvider.GetArg("FIFLFile") == null)
            {
                PrintInfo("请选择LST数据。");
                return null;
            }
            string fiflFile = _argumentProvider.GetArg("FIFLFile").ToString();
            if (!File.Exists(fiflFile))
            {
                PrintInfo("所选择的数据:\"" + fiflFile + "\"不存在。");
                return null;
            }
            if (_argumentProvider.GetArg("FIFLBandNo") == null)
            {
                PrintInfo("参数\"FIFLBandNo\"为空。");
                return null;
            }
            int fiflBand = (int)_argumentProvider.GetArg("FIFLBandNo");

            if (_argumentProvider.GetArg("AvgFile") == null)
            {
                PrintInfo("请选择FIFL基准数据。");
                return null;
            }
            string avg = _argumentProvider.GetArg("AvgFile").ToString();
            if (!File.Exists(avg))
            {
                PrintInfo("所选择的数据:\"" + avg + "\"不存在。");
                return null;
            }
            if (_argumentProvider.GetArg("AvgBandNo") == null)
            {
                PrintInfo("参数\"FIFLAvgCH\"为空。");
                return null;
            }
            int avgBand = (int)_argumentProvider.GetArg("AvgBandNo");
            if (fiflBand < 1 || avgBand < 1)
            {
                PrintInfo("获取波段序号失败,可能是波段映射表配置错误或判识算法波段参数配置错误。");
                return null;
            }
            //无效值
            Int16 defNanValue = (Int16)_argumentProvider.GetArg("defNanValue");
            Int16 defCloudy = (Int16)_argumentProvider.GetArg("defCloudy");
            //输入文件准备
            List<RasterMaper> rms = new List<RasterMaper>();
            try
            {
                IRasterDataProvider inRaster = RasterDataDriver.Open(fiflFile) as IRasterDataProvider;
                if (inRaster.BandCount < fiflBand)
                {
                    PrintInfo("请选择正确的数据进行差异计算。");
                    return null;
                }
                RasterMaper brm = new RasterMaper(inRaster, new int[] { fiflBand });
                rms.Add(brm);
                IRasterDataProvider iRaster = RasterDataDriver.Open(avg) as IRasterDataProvider;
                if (iRaster.BandCount < avgBand)
                {
                    PrintInfo("请选择正确的数据进行距差异计算。");
                    return null;
                }
                RasterMaper rm = new RasterMaper(iRaster, new int[] { avgBand });
                rms.Add(rm);
                //输出文件准备（作为输入栅格并集处理）
                RasterIdentify ri = GetRasterIdentifyID(fiflFile, avg);
                string outFileName = ri.ToWksFullFileName(".dat");
                using (IRasterDataProvider outRaster = CreateOutRaster(outFileName, rms.ToArray()))
                {
                    //栅格数据映射
                    RasterMaper[] fileIns = rms.ToArray();
                    RasterMaper[] fileOuts = new RasterMaper[] { new RasterMaper(outRaster, new int[] { 1 }) };
                    //创建处理模型
                    RasterProcessModel<short, Int16> rfr = null;
                    rfr = new RasterProcessModel<short, Int16>(progressTracker);
                    rfr.SetRaster(fileIns, fileOuts);
                    short[] nanValues = GetNanValues("CloudyValue");
                    short[] waterValues = GetNanValues("WaterValue");
                    rfr.RegisterCalcModel(new RasterCalcHandler<short, Int16>((rvInVistor, rvOutVistor, aoi) =>
                    {
                        int dataLength = rvOutVistor[0].SizeY * rvOutVistor[0].SizeX;
                        if (rvInVistor[0].RasterBandsData[0] != null && rvInVistor[1].RasterBandsData[0] != null)
                        {
                            for (int index = 0; index < dataLength; index++)
                            {
                                Int16 data1 = rvInVistor[0].RasterBandsData[0][index];
                                Int16 data2 = rvInVistor[1].RasterBandsData[0][index];
                                if (data1 == 0 || data2 == 0 || data1 == defNanValue || data2 == defNanValue)
                                {
                                    rvOutVistor[0].RasterBandsData[0][index] = defNanValue;
                                    continue;
                                }
                                if (CloudyProcess.isNanValue(data1, nanValues) ||
                                    CloudyProcess.isNanValue(data2, nanValues))
                                {
                                    rvOutVistor[0].RasterBandsData[0][index] = defCloudy;
                                    continue;
                                }
                                if (CloudyProcess.isNanValue(data1, waterValues) ||
                                    CloudyProcess.isNanValue(data2, waterValues))
                                {
                                    rvOutVistor[0].RasterBandsData[0][index] = waterValues[0];
                                    continue;
                                }
                                rvOutVistor[0].RasterBandsData[0][index] = (Int16)(data1 - data2);
                            }
                        }
                    }));
                    //执行
                    rfr.Excute(defNanValue);
                    FileExtractResult res = new FileExtractResult(_subProductDef.Identify, outFileName, true);
                    res.SetDispaly(false);
                    return res;
                }
            }
            finally
            {
                foreach (RasterMaper rm in rms)
                {
                    rm.Raster.Dispose();
                }
            }
        }

        private IRasterDataProvider CreateOutRaster(string outFileName, RasterMaper[] inrasterMaper)
        {
            IRasterDataDriver raster = RasterDataDriver.GetDriverByName("MEM") as IRasterDataDriver;
            CoordEnvelope outEnv = null;
            foreach (RasterMaper inRaster in inrasterMaper)
            {
                if (outEnv == null)
                    outEnv = inRaster.Raster.CoordEnvelope;
                else
                    outEnv = outEnv.Union(inRaster.Raster.CoordEnvelope);
            }
            float resX = inrasterMaper[0].Raster.ResolutionX;
            float resY = inrasterMaper[0].Raster.ResolutionY;
            int width = (int)(Math.Round(outEnv.Width / resX));
            int height = (int)(Math.Round(outEnv.Height / resY));
            string mapInfo = outEnv.ToMapInfoString(new Size(width, height));
            RasterDataProvider outRaster = raster.Create(outFileName, width, height, 1, enumDataType.Int16, mapInfo) as RasterDataProvider;
            return outRaster;
        }

        private RasterIdentify GetRasterIdentifyID(string lstFile, string avg)
        {
            RasterIdentify rst = new RasterIdentify(new string[] { lstFile, avg });
            rst.ProductIdentify = _subProductDef.ProductDef.Identify;
            rst.SubProductIdentify = _subProductDef.Identify;

            object obj = _argumentProvider.GetArg("OutFileIdentify");
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                rst.SubProductIdentify = obj.ToString();

            rst.IsOutput2WorkspaceDir = true;
            return rst;
        }

        protected short[] GetNanValues(string argumentName)
        {
            string nanValuestring = _argumentProvider.GetArg(argumentName) as string;
            if (!string.IsNullOrEmpty(nanValuestring))
            {
                string[] valueStrings = nanValuestring.Split(new char[] { ',', '，' });
                if (valueStrings != null && valueStrings.Length > 0)
                {
                    List<short> values = new List<short>();
                    short value;
                    for (int i = 0; i < valueStrings.Length; i++)
                    {
                        if (Int16.TryParse(valueStrings[i], out value))
                            values.Add(value);
                    }
                    if (values.Count > 0)
                    {
                        return values.ToArray();
                    }
                }
            }
            return null;
        }

        private void PrintInfo(string info)
        {
            if (_contextMessage != null)
                _contextMessage.PrintMessage(info);
            else
                Console.WriteLine(info);
        }

    }
}
