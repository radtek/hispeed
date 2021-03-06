﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoDo.RSS.Core.DF;

namespace GeoDo.RSS.MIF.Core
{
    public abstract class MonitoringProduct : IMonitoringProduct, IDisposable
    {
        protected string _name;
        protected string _identify;
        protected List<IMonitoringSubProduct> _subProducts = new List<IMonitoringSubProduct>();
        protected ProductDef _productDef;

        public MonitoringProduct()
        {
            _productDef = GetProductDef();
            if (_productDef == null)
                throw new Exception("加载产品失败" + _name);
            Init(_productDef);
        }

        protected abstract ProductDef GetProductDef();

        protected virtual void Init(ProductDef productDef)
        {
            _productDef = productDef;
            _identify = productDef.Identify;
            _name = productDef.Name;
            Dictionary<string, Type> subprds = LoadSubProducts();
            if (subprds != null)
            {
                foreach (string id in subprds.Keys)
                {
                    SubProductDef def = _productDef.GetSubProductDefByIdentify(id);
                    object obj = Activator.CreateInstance(subprds[id], def);
                    if (obj != null)
                        _subProducts.Add(obj as IMonitoringSubProduct);
                }
            }
        }

        protected virtual Dictionary<string, Type> LoadSubProducts() { return null; }

        public ProductDef Definition
        {
            get { return _productDef; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Identify
        {
            get { return _identify; }
        }

        public List<IMonitoringSubProduct> SubProducts
        {
            get { return _subProducts; }
        }

        public IMonitoringSubProduct GetSubProductByIdentify(string identify)
        {
            if (_subProducts == null || _subProducts.Count == 0 || identify == null)
                return null;
            foreach (IMonitoringSubProduct prd in _subProducts)
                if (prd.Identify != null && prd.Identify.ToUpper() == identify.ToUpper())
                    return prd;
            return null;
        }

        public WorkspaceDef GetWorkspaceDef()
        {
            return WorkspaceDefinitionFactory.GetWorkspaceDef(_identify); 
        }

        public virtual void Dispose()
        {
            if (_subProducts != null && _subProducts.Count > 0)
            {
                foreach (IMonitoringSubProduct prd in _subProducts)
                    prd.Dispose();
                _subProducts.Clear();
                _subProducts = null;
            }
            _productDef = null;
        }
    }
}
