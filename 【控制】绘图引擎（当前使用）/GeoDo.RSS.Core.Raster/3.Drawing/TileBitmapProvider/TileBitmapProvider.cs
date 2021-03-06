﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoDo.RSS.Core.DrawEngine;
using GeoDo.RSS.Core.DF;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GeoDo.RSS.Core.RasterDrawing
{
    internal unsafe class TileBitmapProvider : ITileBitmapProvider, IDisposable
    {
        private INearestTilesLocator _tilesLocator = null;
        private IRasterDataProvider _dataProvider;
        private int[] _selectedBandNos = null;
        private ITileBitmapCacheManager _tileCacheManager = null;
        private ITileBitmapCacheManager _sleepTileCacheManager = null;
        private IDataProviderReader _dataProviderReader = null;
        private TileSetting _tileSetting = null;
        private LevelDef[] _levels = null;
        private Stack<TileIdentify> _loadStack = new Stack<TileIdentify>();
        private BackgroundWorker _loadWorker = null;
        private ICanvas _canvas = null;
        private delegate void AsyncRefreshCanvasHandler(TileIdentify tile);
        private AsyncRefreshCanvasHandler _asyncRefreshCanvas = null;
        private LevelDef _currentLevel;
        private int _bRowAtViewWnd, _bColAtViewWnd, _widthAtViewWnd, _heightAtViewWnd;
        private int _totalTileCount = 0;
        private int _loadedTitleCount = 0;
        private bool _disposed = false;
        private bool _loadWorkerIsCompleted = false;
        private bool _loadWorkerIsStarted = false;
        private bool _isNotifyRefreshing = false;
        private IRasterDrawing _drawing = null;
        private string _loadingName = null;
        private bool _isActive = true;
        private EventHandler _canvasEnvelopeChanged = null;
        private IRgbStretcherProvider _stretcherProvider;
        private string _colorTableName;

        public TileBitmapProvider(IRasterDrawing drawing, ICanvas canvas, 
            IRasterDataProvider dataProvider, 
            TileSetting tileSetting, int[] selectedBandNos, 
            GeoDo.RSS.Core.DrawEngine.CoordEnvelope originalEnvelope, 
            float originalResolutionX, float originalResolutionY,
            IRgbStretcherProvider stretcherProvider,string colortablename)
        {
            _drawing = drawing;
            _canvas = canvas;
            _dataProvider = dataProvider;
            _tileSetting = tileSetting;
            _selectedBandNos = selectedBandNos;
            _loadingName = _dataProvider.fileName;
            _colorTableName = colortablename;
            _tilesLocator = new NearestTilesLocator(_dataProvider, originalEnvelope, originalResolutionX, originalResolutionY);
            _tileCacheManager = new TileBitmapCacheManager();
            _sleepTileCacheManager = new TileBitmapCacheManager();
            _canvasEnvelopeChanged = new EventHandler(CanvasEnvelopeChanged);
            _stretcherProvider = stretcherProvider;
            CreateDataProviderReader();
            CreateTileLoadWorker();
        }

        public int TileCountOfLoading
        {
            get 
            {
                if (_loadWorkerIsCompleted)
                    return 0;
                return  _loadStack.Count;
            }
        }

        public IDataProviderReader DataProviderReader
        {
            get { return _dataProviderReader; }
        }

        public ITileBitmapCacheManager TileCacheManager
        {
            get { return _tileCacheManager; }
        }

        public INearestTilesLocator TilesLocator
        {
            get { return _tilesLocator; }
        }

        public ITileComputer TileComputer
        {
            get { return _tilesLocator.TileComputer; }
        }

        private void CreateDataProviderReader()
        {
            switch (_dataProvider.DataType)
            {
                case enumDataType.Byte:
                    _dataProviderReader = new DataProviderReaderByte(_tileSetting.TileSize, _selectedBandNos, _dataProvider,_stretcherProvider,_colorTableName);
                    break;
                case enumDataType.UInt16:
                    _dataProviderReader = new DataProviderReaderUInt16(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
                case enumDataType.Int16:
                    _dataProviderReader = new DataProviderReaderInt16(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
                case enumDataType.UInt32:
                    _dataProviderReader = new DataProviderReaderUInt32(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
                case enumDataType.Int32:
                    _dataProviderReader = new DataProviderReaderInt32(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
                case enumDataType.UInt64:
                    _dataProviderReader = new DataProviderReaderUInt64(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
                case enumDataType.Int64:
                    _dataProviderReader = new DataProviderReaderInt64(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
                case enumDataType.Float:
                    _dataProviderReader = new DataProviderReaderFloat(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
                case enumDataType.Double:
                    _dataProviderReader = new DataProviderReaderDouble(_tileSetting.TileSize, _selectedBandNos, _dataProvider, _stretcherProvider, _colorTableName);
                    break;
            }
        }

        public void UpdateSelectedBandNos(int[] selectedBandNos)
        {
            if (_dataProviderReader == null)
                return;
            _selectedBandNos = selectedBandNos;
            _dataProviderReader.UpdateSelectedBandNos(_selectedBandNos);
            if (_levels != null)
                LoadTileBitmaps(_levels,true);
        }

        private void CreateTileLoadWorker()
        {
            _canvas.OnEnvelopeChanged += _canvasEnvelopeChanged;
            _asyncRefreshCanvas = new AsyncRefreshCanvasHandler(TryRefreshCanvas);
            _loadWorker = new BackgroundWorker();
            _loadWorker.DoWork += new DoWorkEventHandler(_loadWorker_DoWork);
            _loadWorker.WorkerSupportsCancellation = true;
            _loadWorker.WorkerReportsProgress = true;
            _loadWorker.ProgressChanged += new ProgressChangedEventHandler(_loadWorker_ProgressChanged);
        }

        void _loadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_drawing == null || _drawing.LoadingSubscribers == null)
                return;
            foreach (ILoadingPrecentSubscriber sub in _drawing.LoadingSubscribers)
                sub.Percent(_loadingName, e.ProgressPercentage);
        }

        void CanvasEnvelopeChanged(object sender, EventArgs e)
        {
            TileIdentify[] visibleTiles;
            LevelDef suitableLevel;
            _tilesLocator.Compute(_canvas, out suitableLevel, out visibleTiles);
            if (visibleTiles == null)
                return;
            _currentLevel = suitableLevel;
            _tilesLocator.GetRasterRowColOfViewWnd(_canvas, out _bRowAtViewWnd, out _bColAtViewWnd, out _widthAtViewWnd, out _heightAtViewWnd);
            foreach (TileIdentify tile in visibleTiles)
            {
                if (!_tileCacheManager.IsExist(tile))
                    _loadStack.Push(tile);
            }
        }
        
        //private object _lockDoWork = new object();
        //void _loadWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    try
        //    {
        //        _loadWorkerIsCompleted = false;
        //        while (_loadStack.Count > 0)
        //        {
        //            lock (_lockDoWork)
        //            {
        //                TileIdentify tile = _loadStack.Pop();
        //                if (!_tileCacheManager.IsExist(tile))
        //                {
        //                    bool memoryIsNotEnough = false;
        //                    LoadTileBitmap(tile, out memoryIsNotEnough);
        //                    if (memoryIsNotEnough)
        //                    {
        //                        Console.WriteLine("_loadWorker_DoWork:内存不足，无法读取请求的瓦片！");
        //                        _loadWorkerIsCompleted = true;//这里的处理是错误的，应该是瓦片交换
        //                        return;
        //                    }
        //                    _loadWorker.ReportProgress((int)(_loadedTitleCount * 100f / _totalTileCount));
        //                    _asyncRefreshCanvas.BeginInvoke(tile, new AsyncCallback((ret) => { _isNotifyRefreshing = false; }), null);
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        _loadWorkerIsCompleted = true;
        //    }
        //}

        private object _lockDoWork = new object();
        void _loadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool isGDAL = _dataProvider is GeoDo.RSS.DF.GDAL.IGDALRasterDataProvider;
            try
            {
                _loadWorkerIsCompleted = false;
                if (isGDAL)
                {
                    while (_loadStack.Count > 0)
                    {
                        lock (_lockDoWork)
                        {
                            TileIdentify tile = _loadStack.Pop();
                            DoLoadTile(tile);
                        }
                    }
                }
                else
                {
                    while (_loadStack.Count > 0)
                    {
                        TileIdentify tile = _loadStack.Pop();
                        DoLoadTile(tile);
                    }
                }
            }
            finally
            {
                _loadWorkerIsCompleted = true;
            }
        }

        private void DoLoadTile(TileIdentify tile)
        {
            if (!_tileCacheManager.IsExist(tile))
            {
                bool memoryIsNotEnough = false;
                LoadTileBitmap(tile, out memoryIsNotEnough);
                if (memoryIsNotEnough)
                {
                    Console.WriteLine("_loadWorker_DoWork:内存不足，无法读取请求的瓦片！");
                    _loadWorkerIsCompleted = true;//这里的处理是错误的，应该是瓦片交换
                    return;
                }
                _loadWorker.ReportProgress((int)(_loadedTitleCount * 100f / _totalTileCount));
                _asyncRefreshCanvas.BeginInvoke(tile, new AsyncCallback((ret) => { _isNotifyRefreshing = false; }), null);
            }
        }

        private void TryRefreshCanvas(TileIdentify tile)
        {
            _isNotifyRefreshing = true;
            if (_currentLevel.LevelNo == tile.Level.LevelNo)
            {
                int beginRow = 0, beginCol = 0, width = 0, height = 0;
                tile.Level.GetOriginalRowCol(_tileSetting.TileSize, tile, ref beginRow, ref beginCol, ref width, ref height);
                if (beginRow > (_bRowAtViewWnd + _heightAtViewWnd) ||
                    beginCol > (_bColAtViewWnd + _widthAtViewWnd) ||
                    (beginRow + height) < _bRowAtViewWnd ||
                    (beginCol + width) < _bColAtViewWnd)
                    return;
                _canvas.Refresh(enumRefreshType.All);
                if (_canvas.SomeTileIsArrived != null)
                    _canvas.SomeTileIsArrived();
            }
        }

        public void Reset()
        {
            _disposed = true;
            try
            {
                SetToSleep();
                _sleepTileCacheManager.Clear();
                CreateTileLoadWorker();
            }
            finally
            {
                _disposed = false;
            }
        }

        void SetToSleep()
        {
            _loadStack.Clear();
            while (_isNotifyRefreshing)
                Thread.Sleep(10);
            if (_loadWorkerIsStarted)
            {
                while (!_loadWorkerIsCompleted)
                    Thread.Sleep(10);
                _loadWorker.Dispose();
                _loadWorkerIsStarted = false;
            }
            _tileCacheManager.Clear();
            _totalTileCount = 0;
            _loadedTitleCount = 0;
            _canvas.OnEnvelopeChanged -= _canvasEnvelopeChanged;
        }

        public bool IsActive
        {
            get { return _isActive; }
        }

        public void Active()
        {
            if (_isActive)
                return;
            try
            {
                CreateTileLoadWorker();
                if (_sleepTileCacheManager.Count > 0)
                {
                    foreach (TileBitmap tb in _sleepTileCacheManager.TileBitmaps.ToArray())
                    {
                        _tileCacheManager.Put(tb);
                        _sleepTileCacheManager.Remove(tb);
                    }
                }
                ReLoadTileBitmapsBySync(_levels);
            }
            finally
            {
                _isActive = true;
            }
        }

        //只留当前屏幕显示的几个瓦片，其余全部释放
        public void Deactive()
        {
            if (!_isActive)
                return;
            try
            {
                TileIdentify[] visibleTiles;
                LevelDef suitableLevel;
                _tilesLocator.Compute(_canvas, out suitableLevel, out visibleTiles);
                if (visibleTiles == null)
                    return;
                foreach (TileIdentify tile in visibleTiles)
                {
                    TileBitmap tb = _tileCacheManager.Get(suitableLevel.LevelNo, tile.Row, tile.Col);
                    if (tb != null)
                    {
                        _sleepTileCacheManager.Put(tb);
                        _tileCacheManager.Remove(tb);
                    }
                }
            }
            finally
            {
                SetToSleep();
                _isActive = false;
            }
        }

        public void ReLoadTileBitmapsBySync(LevelDef[] levels)
        {
            _levels = levels;
            ComputeTotalTileCount();
            for (int i = levels.Length - 1; i >= 0; i--)
            {
                LevelDef lv = levels[i];
                foreach (TileIdentify tile in lv.TileIdentities)
                    _loadStack.Push(tile);
            }
            _loadWorker.RunWorkerAsync();
            _loadWorkerIsStarted = true;
        }

        public int LoadTileBitmaps(LevelDef[] levels, bool isSync)
        {
            _levels = levels;
            ComputeTotalTileCount();
            //同步加载最适合的级别(修改为异步5.15)
            TileIdentify[] visibleTiles;
            LevelDef nearestLevel;
            _tilesLocator.Compute(_canvas, out nearestLevel, out visibleTiles);
            if (nearestLevel.IsEmpty())
                return 0;
            _currentLevel = nearestLevel;
            if (visibleTiles != null)
            {
                foreach (TileIdentify tile in visibleTiles)
                {
                    bool memoryIsNotEnough = false;
                    if (isSync)//同步
                    {
                        LoadTileBitmap(tile, out memoryIsNotEnough);
                        if (memoryIsNotEnough)
                        {
                            Console.WriteLine("LoadTileBitmaps：内存不足，无法完成请求的瓦片！");
                            return 0;
                        }
                    }
                    else       //异步    
                        _loadStack.Push(tile);
                }
                foreach (TileIdentify tile in nearestLevel.TileIdentities)
                    if (Array.IndexOf(visibleTiles, tile) < 0)
                        _loadStack.Push(tile);
            }
            else
            {
                foreach (TileIdentify tile in nearestLevel.TileIdentities)
                    _loadStack.Push(tile);
            }
            //MessageBox.Show("最小层加载完毕");
            //将剩余级别插入异步装载队列(从低分辨率开始)
            for (int i = levels.Length - 1; i >= 0; i--)
            {
                LevelDef lv = levels[i];
                if (lv.LevelNo != nearestLevel.LevelNo)
                    foreach (TileIdentify tile in lv.TileIdentities)
                        _loadStack.Push(tile);
            }
            //MessageBox.Show("剩余层已添加到队列");
            _loadWorker.RunWorkerAsync();
            _loadWorkerIsStarted = true;
            //MessageBox.Show("异步加载已启动");
            return visibleTiles != null ? visibleTiles.Length : 0;
        }

        private void ComputeTotalTileCount()
        {
            if (_levels == null)
                return;
            foreach (LevelDef lv in _levels)
                _totalTileCount += lv.TileIdentities.Length;
        }

        private TileBitmap LoadTileBitmap(TileIdentify tile,out bool memoryIsNotEnough)
        {
            memoryIsNotEnough = false;
            int beginRow = 0, beginCol = 0, width = 0, height = 0;
            LevelDef level = tile.Level;
            level.GetOriginalRowColByDataBlock(tile, ref beginRow, ref beginCol, ref width, ref height);
            TileBitmap tb = _dataProviderReader.CreateBitmapByTile(level, beginRow, beginCol, width, height, tile,out memoryIsNotEnough);
            if (!_disposed && tb != null)
            {
                _tileCacheManager.Put(tb);
                _loadedTitleCount++;
            }
            return tb;
        }


        private void ComputeRasterRect(LevelDef level, TileIdentify[] visibleTiles, out Rectangle rasterRect)
        {
            rasterRect = new Rectangle();
            if (visibleTiles == null)
                return;
            int x = 0, y = 0, w = 0, h = 0;
            int tileSize = _tileSetting.TileSize;
            int top = int.MaxValue, bottom = int.MinValue;
            int left = int.MaxValue, right = int.MinValue;
            foreach (TileIdentify tile in visibleTiles)
            {
                level.GetOriginalRowCol(tileSize, tile, ref y, ref x, ref w, ref h);
                if (x < left)
                    left = x;
                if ((x + w) > right)
                    right = x + w;
                if (y < top)
                    top = y;
                if ((y + h) > bottom)
                    bottom = y + h;
            }
            rasterRect.X = left;
            rasterRect.Y = top;
            rasterRect.Width = right - left;
            rasterRect.Height = bottom - top;
        }

        public TileIdentify[] GetVisibleTiles(out LevelDef level, out Rectangle rasterRect, out int bRow, out int bCol, out int rows, out int cols)
        {
            bRow = bCol = rows = cols = 0;
            rasterRect = new Rectangle();
            level = new LevelDef();
            if (_levels == null)
                return null;
            TileIdentify[] visibleTiles;
            _tilesLocator.Compute(_canvas, out level, out visibleTiles);
            goto firtTimes;
        toUpLevel:
            _tilesLocator.ComputeByLevel(_canvas, level, out visibleTiles);
        firtTimes:
            if (visibleTiles != null)
            {
                if (AllTilesIsExist(visibleTiles))
                {
                    ComputeRasterRect(level, visibleTiles, out rasterRect);
                    GetRowColRange(visibleTiles, out bRow, out bCol, out rows, out cols);
                }
                else
                {
                    if (level.LevelNo < _levels.Length - 1)
                    {
                        level = _levels[level.LevelNo + 1];
                        goto toUpLevel;
                    }
                }
            }
            return visibleTiles;
        }

        private bool AllTilesIsExist(TileIdentify[] visibleTiles)
        {
            foreach (TileIdentify t in visibleTiles)
                if (!_tileCacheManager.IsExist(t))
                    return false;
            return true;
        }

        public TileBitmap GetTileBitmap(TileIdentify tile)
        {
            LevelDef level = tile.Level;
            return _tileCacheManager.Get(level.LevelNo, tile.Row, tile.Col);
        }

        private void GetRowColRange(TileIdentify[] visibleTiles, out int bRow, out int bCol, out int rows, out int cols)
        {
            int minR = int.MaxValue;
            int maxR = int.MinValue;
            int minC = int.MaxValue;
            int maxC = int.MinValue;
            if (visibleTiles == null)
            {
                bRow = bCol = rows = cols = 0;
                return;
            }
            foreach (TileIdentify id in visibleTiles)
            {
                if (id.Row < minR)
                    minR = id.Row;
                if (id.Row > maxR)
                    maxR = id.Row;
                if (id.Col < minC)
                    minC = id.Col;
                if (id.Col > maxC)
                    maxC = id.Col;
            }
            bRow = minR;
            bCol = minC;
            rows = maxR - minR + 1;
            cols = maxC - minC + 1;
        }

        public void Dispose()
        {
            _disposed = true;
            //clear load queue for thread exit
            _loadStack.Clear();
            while (!_loadWorkerIsCompleted && _loadWorkerIsStarted)
                Thread.Sleep(10);
            _loadWorker.Dispose();
            //
            if (_loadWorker != null)
            {
                _loadWorker.Dispose();
                _loadWorker = null;
            }
            if (_tilesLocator != null)
            {
                _tilesLocator.Dispose();
                _tilesLocator = null;
            }
            if (_tileCacheManager != null)
            {
                _tileCacheManager.Dispose();
                _tileCacheManager = null;
            }
            if (_sleepTileCacheManager != null)
            {
                _sleepTileCacheManager.Dispose();
                _sleepTileCacheManager = null;
            }
            if (_dataProviderReader != null)
            {
                _dataProviderReader.Dispose();
                _dataProviderReader = null;
            }
            _dataProvider = null;
            _asyncRefreshCanvas = null;
            _canvas.OnEnvelopeChanged -= _canvasEnvelopeChanged;
            _canvas = null;
        }
    }
}
