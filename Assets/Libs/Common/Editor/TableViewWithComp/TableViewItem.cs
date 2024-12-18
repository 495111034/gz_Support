using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSupportEditor
{
    class TableViewItem
    {
        bool _isToggleOn = false;
        public bool IsToggleOn
        {
            get {
                return _isToggleOn;
            }
            set {
                _isToggleOn = value;
            }
        }

        bool _isBtn1Show = true;
        public bool IsBtn1Show 
        {
            get {
                return _isBtn1Show;
            }
        }

        bool _isBtn2Show = true;
        public bool IsBtn2Show
        {
            get {
                return _isBtn2Show;
            }
        }

        object _itemObj = null;
        public object ItemObj
        {
            get {
                return _itemObj;
            }
        }

        public TableViewItem(object obj , bool btn1Show , bool btn2Show)
        {
            this._itemObj = obj;
            this._isBtn1Show = btn1Show;
            this._isBtn2Show = btn2Show;
        }
    }
}
