using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameSupportEditor
{
    public class TableViewWithComp : IDisposable
    {
        public struct BtnData
        { 
            public bool btn1Show;
            public bool btn2Show;
            public BtnData(bool btn1Show ,bool btn2Show)
            {
                this.btn1Show = btn1Show;
                this.btn2Show = btn2Show;
            }
        }

        private readonly Color LogColor = Color.white;
        private readonly Color WarningColor = Color.yellow;
        private readonly Color ErrorColor = Color.red;

        public delegate void SelectionHandler(object selected, int col);
        public delegate void ButtonClickedHander(object selected);
        private bool _ctrlKeyDown = false;
        private bool _shiftKeyDown = false;

        private event SelectionHandler _OnSelected;
        private event ButtonClickedHander _OnBtn1Clicked;
        private event ButtonClickedHander _OnBtn2Clicked;
        private event ButtonClickedHander _OnIgnoreClicked;
        private string _btn1Title = "1";
        private string _btn2Title = "2";

        private bool _bBtn1Show = true;
        private bool _bBtn2Show = true;


        private bool _bFoldout = true;//详情展开
        public bool Foldout
        {
            get {
                return _bFoldout;
            }
            set {
                _bFoldout = value;
            }
        }
        private WarnType _warnType = WarnType.Log;

        private Type _itemType = null;
        private EditorWindow _hostWindow = null;

        private string _title = "TableView";

        //列描述
        private List<TableViewColDesc> _descArray = new List<TableViewColDesc>();
        TableViewAppr _appearance = new TableViewAppr();
        //所有列表对象
        private List<TableViewItem> _lines = new List<TableViewItem>();
        //选中的列表对象
        private List<object> _selected = new List<object>();
        private Dictionary<object, Color> _specialTextColors;
        private int _sortSlot = 0;
        private bool _descending = true;
        private int _selectedCol = -1;
        public bool ShowInternalSeqID = false;

        GUIContent[] menuOptions = new GUIContent[] {
            new GUIContent("忽略")
        };

        public TableViewWithComp(EditorWindow hostWindow, Type itemType , WarnType warnType = WarnType.Log)
        {
            _hostWindow = hostWindow;
            _itemType = itemType;
            _warnType = warnType;
        }

        public void Dispose()
        {

        }

        private void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {

        }

        public void SetTitle(string title)
        {
            _title = title;
        }

        public void RegisterSelected(SelectionHandler handle)
        {
            _OnSelected = handle;
        }

        public void RegisterIgnore(ButtonClickedHander handle)
        {
            _OnIgnoreClicked = handle;
        }

        public void RegisterColBtn1(string btnTitle, ButtonClickedHander handle)
        {
            _btn1Title = btnTitle;
            _OnBtn1Clicked = handle;
        }

        public void RegisterColBtn2(string btnTitle, ButtonClickedHander handle)
        {
            _btn2Title = btnTitle;
            _OnBtn2Clicked = handle;
        }


        public bool AddColumn(string colDataPropertyName, string colTitleText, float widthByPercent, TextAnchor alignment = TextAnchor.MiddleCenter, string fmt = "")
        {
            TableViewColDesc desc = new TableViewColDesc();
            desc.PropertyName = colDataPropertyName;
            desc.TitleText = colTitleText;
            desc.Alignment = alignment;
            desc.WidthInPercent = widthByPercent;
            desc.Format = string.IsNullOrEmpty(fmt) ? null : fmt;
            desc.MemInfo = _itemType.GetField(desc.PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            if (desc.MemInfo == null)
            {
                desc.MemInfo = _itemType.GetProperty(desc.PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
                if (desc.MemInfo == null)
                {
                    Debug.LogWarningFormat("Field '{0}' accessing failed.", desc.PropertyName);
                    return false;
                }
            }

            _descArray.Add(desc);
            return true;
        }

        public void RefreshData(List<object> entries, List<BtnData> itemBtnData = null, Dictionary<object, Color> specialTextColors = null)
        {
            _lines.Clear();

            if (entries != null && entries.Count > 0)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    bool btn1Show = true;
                    bool btn2Show = true;
                    if (itemBtnData != null && itemBtnData.Count >= entries.Count)
                    {
                        btn1Show = itemBtnData[i].btn1Show;
                        btn2Show = itemBtnData[i].btn2Show;
                    }
                    TableViewItem tableViewItem = new TableViewItem(entries[i] , btn1Show, btn2Show);
                    _lines.Add(tableViewItem);
                }
                SortData();
            }

            _specialTextColors = specialTextColors;
        }

        private void SortData()
        {
            _lines.Sort((s1, s2) =>
            {
                if (_sortSlot >= _descArray.Count)
                    return 0;

                return _descArray[_sortSlot].Compare(s1.ItemObj, s2.ItemObj) * (_descending ? -1 : 1);
            });
        }

        #region DrawUI

        public Rect DrawUI(Rect lastRect)
        {
            _ctrlKeyDown = Event.current.control;
            _shiftKeyDown = Event.current.shift;
            float fadeLabelHeight = 20;
            float tableViewHeight = _appearance.LineHeight * (_lines.Count + 3);
            float totalHeight = _bFoldout ?  fadeLabelHeight + tableViewHeight : fadeLabelHeight;
            lastRect = GS_GUILayoutUtils.GetNextRect(lastRect, totalHeight, 5);
            GUILayout.BeginArea(lastRect);

            GUIStyle tableTitleStyle = GetFoldoutHeaderStyle();

            _bFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_bFoldout , _title , tableTitleStyle);
            if (_bFoldout)
            {
                Rect toolHeadRect = new Rect(lastRect.x, fadeLabelHeight, lastRect.width, _appearance.LineHeight);
                Rect titleRect = DrawToolHead(toolHeadRect);
                Rect lineRect = DrawTitle(titleRect);
                for (int i = 0; i < _lines.Count; i++)
                {
                    lineRect = DrawLine(lineRect, i);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndArea();
            return lastRect;
        }

        private Rect DrawToolHead(Rect rect)
        {
            Rect rectOffset = new Rect(rect.x , rect.y , 80 , rect.height);
            if (GUI.Button(rectOffset, "全选"))
            {
                OnSetAllToggleOn();
            }
            rectOffset.x = rectOffset.x + 100;
            if (GUI.Button(rectOffset, "反选"))
            {
                OnResverAllToggle();
            }
            rect.y = rect.y + rect.height;
            return rect;
        }

        private Rect DrawTitle(Rect rect)
        {
            //toggle
            float compWidth = 40;
            Rect colRect = new Rect(rect.x, rect.y, compWidth, rect.height);
            GUI.Label(colRect , "" , _appearance.GetTitleStyle(false));
            colRect.x = colRect.x + compWidth;

            if (rect.width > colRect.xMax)
            {
                float leaveWidth = rect.width - colRect.xMax;
                leaveWidth -= (compWidth *2);

                for (int i = 0; i < _descArray.Count; i++)
                {
                    var desc = _descArray[i];

                    float width = leaveWidth * desc.WidthInPercent;
                    colRect.width = width;

                    bool selected = _sortSlot == i;
                    GUI.Label(colRect, desc.TitleText + (selected ? _appearance.GetSortMark(_descending) : ""), _appearance.GetTitleStyle(selected));   
                    if (Event.current.type == EventType.MouseDown && colRect.Contains(Event.current.mousePosition))
                    {
                        if (_sortSlot == i)
                        {
                            _descending = !_descending;
                        }
                        else
                        {
                            _sortSlot = i;
                        }

                        SortData();
                        _hostWindow.Repaint();
                    }
                    colRect.x = colRect.x + width;
                }
            }

            //button
            if (_bBtn1Show)
            {
                colRect.width = compWidth;
                GUI.Label(colRect, _btn1Title, _appearance.GetTitleStyle(false));
                colRect.x = colRect.x + compWidth;
            }
            if (_bBtn2Show)
            {
                colRect.width = compWidth;
                GUI.Label(colRect, _btn2Title, _appearance.GetTitleStyle(false));
                colRect.x = colRect.x + compWidth;
            }

            rect.y = rect.y + rect.height;
            return rect;
        }

        private GUIStyle GetFoldoutHeaderStyle()
        {
            GUIStyle tableTitleStyle = new GUIStyle(EditorStyles.foldoutHeader);
            Color color;
            switch (_warnType)
            {
                case WarnType.Warining:
                    color = WarningColor;
                    break;
                case WarnType.Error:
                    color = ErrorColor;
                    break;
                default:
                    color = LogColor;
                    break;
            }
            tableTitleStyle.normal.textColor = color;
            tableTitleStyle.hover.textColor = color;
            tableTitleStyle.onNormal.textColor = color;
            tableTitleStyle.onHover.textColor = color;
            tableTitleStyle.onActive.textColor = color;
            tableTitleStyle.focused.textColor = color;
            tableTitleStyle.onFocused.textColor = color;
            tableTitleStyle.active.textColor = color;
            return tableTitleStyle;
        }

        private Rect DrawLine(Rect rect , int lineIndex)
        {
            TableViewItem item = _lines[lineIndex];

            float compWidth = 40;
       
            //toggle
            Rect colRect = new Rect(rect.x, rect.y, compWidth, rect.height);
            float originLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0;
            item.IsToggleOn = GUI.Toggle(colRect, item.IsToggleOn , "");
            EditorGUIUtility.labelWidth = originLabelWidth;
            colRect.x = colRect.x + compWidth;
            
            if (rect.width > colRect.xMax)
            {
                float leaveWidth = rect.width - colRect.xMax;
                leaveWidth -= (compWidth * 2);

                Rect contentRect = rect;
                contentRect.width = leaveWidth;
                bool rightKeyDown = Event.current.button == 1 && Event.current.type == EventType.MouseDown && contentRect.Contains(Event.current.mousePosition);
                if (rightKeyDown)
                {
                    Vector2 mousePosition = Event.current.mousePosition;
                    EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), menuOptions, -1, delegate (object data, string[] opt, int select)
                    {
                        if (select < 0)
                        {
                            return;
                        }
                        string keyName = opt[select];
                        if (keyName == "忽略")
                        {
                            _OnIgnoreClicked?.Invoke(data);
                        }
                
                    }, item.ItemObj);//在鼠标的位置弹出菜单，菜单的路径
                }

                bool selectionHappens = Event.current.type == EventType.MouseDown && contentRect.Contains(Event.current.mousePosition);
                GUIStyle style = new GUIStyle((lineIndex % 2 != 0) ? _appearance.Style_Line : _appearance.Style_LineAlt);
                if (selectionHappens)
                {
                    if (_ctrlKeyDown)
                    {
                        if (_selected.Contains(item.ItemObj))
                            _selected.Remove(item.ItemObj);
                        else
                            _selected.Add(item.ItemObj);
                    }
                    else if (_shiftKeyDown && _selected.Count > 0)
                    {
                        bool foundstart = false;
                        bool foundend = false;
                        var selectedstart = _selected[0];
                        foreach (var line in _lines)
                        {
                            if (line == selectedstart)
                                foundstart = true;
                            if (line == item.ItemObj)
                                foundend = true;
                            if (foundstart || foundend)
                            {
                                if (!_selected.Contains(line))
                                    _selected.Add(line);
                            }
                            if (foundstart && foundend)
                                break;

                        }
                    }
                    else
                    {
                        _selected.Clear();
                        _selected.Add(item.ItemObj);
                    }
                }
                if (_selected.Contains(item.ItemObj))
                {
                    style = _appearance.Style_Selected;
                }
                else
                {
                    Color specialColor;
                    if (_specialTextColors != null &&
                        _specialTextColors.TryGetValue(item.ItemObj, out specialColor))
                    {
                        style.normal.textColor = specialColor;
                    }
                }
                for (int i = 0; i < _descArray.Count; i++)
                {
                    TableViewColDesc desc = _descArray[i];

                    float width = leaveWidth * desc.WidthInPercent;
                    colRect.width = width;
                    DrawLineCol(colRect , i , item.ItemObj , style , selectionHappens);
                    colRect.x = colRect.x + width;
                }
            }

            //button
            if (_bBtn1Show)
            {
                colRect.width = compWidth;
                if (item.IsBtn1Show)
                {
                    if (GUI.Button(colRect, _btn1Title))
                    {
                        _OnBtn1Clicked?.Invoke(item.ItemObj);
                    }

                }
                colRect.x = colRect.x + compWidth;
            }

            if (_bBtn2Show)
            {
                colRect.width = compWidth;
                if (item.IsBtn2Show)
                {
                    if (GUI.Button(colRect, _btn2Title))
                    {
                        _OnBtn2Clicked?.Invoke(item.ItemObj);
                    }
                }
                colRect.x = colRect.x + compWidth;
            }

            rect.y = rect.y + rect.height;
            return rect;
        }

        private void DrawLineCol(Rect rect, int col , object obj, GUIStyle style, bool selectionHappens = false)
        {
            var desc = _descArray[col];
            string text = desc.FormatObject(obj);

            if (selectionHappens && rect.Contains(Event.current.mousePosition))
            {
                _selectedCol = col;
                if (_OnSelected != null)
                    _OnSelected(obj, col);

                EditorGUIUtility.systemCopyBuffer = text;
                _hostWindow.Repaint();
            }

            // internal sequential id
            if (ShowInternalSeqID && col == 0)
                text = col.ToString() + ". " + text;

            // note that the 'selected-style' assignment below should be isolated from the if-conditional statement above
            // since the above if is a one-time event, on the contrary, the 'selected-style' assignment below should be done every time in the drawing process
            if (_selectedCol == col && _selected == obj)
            {
                style = _appearance.Style_SelectedCell;
            }

            style.alignment = desc.Alignment;
            GUI.Label(rect, new GUIContent(text, text), style);
        }

        public List<object> GetSelected()
        {
            return _selected;
        }

        public List<object> GetAllToggleOn()
        {
            List<object> lstObj = new List<object>();
            for (int i = 0; i < _lines.Count; i++)
            {
                TableViewItem item = _lines[i];
                lstObj.Add(item.ItemObj);
                item.IsToggleOn = !item.IsToggleOn;
            }
            return lstObj;
        }

        public void OnSetAllToggleOn()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i].IsToggleOn = true;
            }
        }

        public void OnResverAllToggle()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                TableViewItem item = _lines[i];
                item.IsToggleOn = !item.IsToggleOn;
            }
        }

        public void SetBtn1Active(bool active)
        {
            _bBtn1Show = active;
        }

        public void SetBtn2Active(bool active)
        {
            _bBtn2Show = active;
        }

        #endregion

    }
}
