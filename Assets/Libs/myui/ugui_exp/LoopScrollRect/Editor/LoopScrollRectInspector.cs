using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(LoopScrollRect), true)]
public class LoopScrollRectInspector : Editor
{
    SerializedProperty childPrefab, animaCellInterval, poolSize, m_Content, m_Horizontal, m_Vertical, m_MovementType, m_Elasticity, m_Inertia, m_DecelerationRate, m_ScrollSensitivity, m_Viewport,
            m_HorizontalScrollbar, m_VerticalScrollbar, m_HorizontalScrollbarVisibility, m_VerticalScrollbarVisibility, m_HorizontalScrollbarSpacing, m_VerticalScrollbarSpacing, m_onValueChanged, m_circleLayout,
         m_baseDir, m_radian, m_scaleCircle, m_circleScaleTimes, m__reverseDirection;

    GUIContent CchildPrefab, CanimaCellInterval, CpoolSize, Cm_Content, Cm_Horizontal, Cm_Vertical, Cm_MovementType, Cm_Elasticity, Cm_Inertia, Cm_DecelerationRate, Cm_ScrollSensitivity, Cm_Viewport,
        Cm_HorizontalScrollbar, Cm_VerticalScrollbar, Cm_HorizontalScrollbarVisibility, Cm_VerticalScrollbarVisibility, Cm_HorizontalScrollbarSpacing, Cm_VerticalScrollbarSpacing, Cm_onValueChanged,
         C_totalCount, Cthreshold, CreverseDirection, CrubberScale, CcircleLayout, CBaseDir, Cradian, CscaleCircle, CcircleScaleTimes;
    protected virtual void OnEnable()
    {


        childPrefab = serializedObject.FindProperty("childPrefab");
        animaCellInterval = serializedObject.FindProperty("cellAnimaInterval");
        poolSize = serializedObject.FindProperty("poolSize");
        m_Content = serializedObject.FindProperty("m_Content");
        m_Horizontal = serializedObject.FindProperty("m_Horizontal");
        m_Vertical = serializedObject.FindProperty("m_Vertical");
        m_MovementType = serializedObject.FindProperty("m_MovementType");
        m_Inertia = serializedObject.FindProperty("m_Inertia");
        m_Elasticity = serializedObject.FindProperty("m_Elasticity");
        m_DecelerationRate = serializedObject.FindProperty("m_DecelerationRate");
        m_ScrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
        m_Viewport = serializedObject.FindProperty("m_Viewport");
        m_HorizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
        m_VerticalScrollbar = serializedObject.FindProperty("m_VerticalScrollbar");
        m_HorizontalScrollbarVisibility = serializedObject.FindProperty("m_HorizontalScrollbarVisibility");
        m_VerticalScrollbarVisibility = serializedObject.FindProperty("m_VerticalScrollbarVisibility");
        m_HorizontalScrollbarSpacing = serializedObject.FindProperty("m_HorizontalScrollbarSpacing");
        m_VerticalScrollbarSpacing = serializedObject.FindProperty("m_VerticalScrollbarSpacing");
        m_onValueChanged = serializedObject.FindProperty("m_onValueChanged");
        m_circleLayout = serializedObject.FindProperty("circleLayout");
        m_baseDir = serializedObject.FindProperty("baseDir");
        m_radian = serializedObject.FindProperty("radian");
        m_scaleCircle = serializedObject.FindProperty("scaleCircle");
        m_circleScaleTimes = serializedObject.FindProperty("circleScaleTimes");
        m__reverseDirection = serializedObject.FindProperty("_reverseDirection");



        CchildPrefab = new GUIContent("节点资源");
        CanimaCellInterval = new GUIContent("节点动画间隔");
        CpoolSize = new GUIContent("缓冲数");
        Cm_Content = new GUIContent("内容区");
        Cm_Horizontal = new GUIContent("水平滚动");
        Cm_Vertical = new GUIContent("垂直滚动");
        Cm_MovementType = new GUIContent("滚动类型");
        Cm_Elasticity = new GUIContent("弹性");
        Cm_Inertia = new GUIContent("惯性");
        Cm_DecelerationRate = new GUIContent("减速率");
        Cm_ScrollSensitivity = new GUIContent("灵敏度");
        Cm_Viewport = new GUIContent("Viewport");
        Cm_HorizontalScrollbar = new GUIContent("水平滚动条");
        Cm_VerticalScrollbar = new GUIContent("垂直滚动条");
        Cm_HorizontalScrollbarVisibility = new GUIContent("水平滚动条显示方式");
        Cm_VerticalScrollbarVisibility = new GUIContent("垂直滚动条显示方式");
        Cm_HorizontalScrollbarSpacing = new GUIContent("水平滚动条间距");
        Cm_VerticalScrollbarSpacing = new GUIContent("垂直滚动条间距");
        Cm_onValueChanged = new GUIContent("滚动事件通知");
        C_totalCount = new GUIContent("节点总数");
        Cthreshold = new GUIContent("边缘缓冲(像素)");
        CreverseDirection = new GUIContent("反向");
        CrubberScale = new GUIContent("速度缩放");
        CcircleLayout = new GUIContent("弧形排列");
        CBaseDir = new GUIContent("圆心在：");
        Cradian = new GUIContent("弧度");
        CscaleCircle = new GUIContent("中段缩放");
        CcircleScaleTimes = new GUIContent("缩放比例");



    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();  //start       

        EditorGUILayout.ObjectField(childPrefab, CchildPrefab, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(animaCellInterval, CanimaCellInterval, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(poolSize, CpoolSize, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_Content, Cm_Content, new GUILayoutOption[0]);


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Cm_Horizontal, new GUILayoutOption[0]);
        m_Horizontal.boolValue = EditorGUILayout.Toggle(m_Horizontal.boolValue, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Cm_Vertical, new GUILayoutOption[0]);
        m_Vertical.boolValue = EditorGUILayout.Toggle(m_Vertical.boolValue, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(m_MovementType, Cm_MovementType, new GUILayoutOption[0]);
        ++EditorGUI.indentLevel;
        EditorGUILayout.HelpBox("Unrestricted:无尽模式\nElastic:尽头回弹\nClamped:尽头停止", MessageType.Info);
        --EditorGUI.indentLevel;

        EditorGUILayout.PropertyField(m_circleLayout, CcircleLayout, new GUILayoutOption[0]);
        if (m_circleLayout.boolValue)
        {
            ++EditorGUI.indentLevel;

            EditorGUILayout.LabelField(CBaseDir);
            ++EditorGUI.indentLevel;
            if (target is LoopVerticalScrollRect)
            {
                if ((target as MonoBehaviour).gameObject.GetRectTransform().pivot.y != 1)
                {
                    EditorGUILayout.HelpBox("垂直弧形排列时请将pivot.y设置为1", MessageType.Error);
                }

                bool isbiggerzero = m_baseDir.floatValue > 0;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("右侧");
                isbiggerzero = EditorGUILayout.Toggle(isbiggerzero, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("左侧");
                isbiggerzero = !EditorGUILayout.Toggle(!isbiggerzero, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
                m_baseDir.floatValue = isbiggerzero ? 1 : -1;
            }
            else if (target is LoopHorizontalScrollRect)
            {
                if ((target as MonoBehaviour).gameObject.GetRectTransform().pivot.x != 0)
                {
                    EditorGUILayout.HelpBox("垂直弧形排列时请将pivot.x设置为0", MessageType.Error);
                }
                bool isbiggerzero = m_baseDir.floatValue > 0;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("下侧");
                isbiggerzero = EditorGUILayout.Toggle(isbiggerzero, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("上侧");
                isbiggerzero = !EditorGUILayout.Toggle(!isbiggerzero, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
                m_baseDir.floatValue = isbiggerzero ? 1 : -1;
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(m_radian, Cradian, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_scaleCircle, CscaleCircle, new GUILayoutOption[0]);
            if (m_scaleCircle.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_circleScaleTimes, CcircleScaleTimes, new GUILayoutOption[0]);
                --EditorGUI.indentLevel;
            }

            --EditorGUI.indentLevel;
        }

        EditorGUILayout.PropertyField(m_Inertia, Cm_Inertia, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_Elasticity, Cm_Elasticity, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_DecelerationRate, Cm_DecelerationRate, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_ScrollSensitivity, Cm_ScrollSensitivity, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_Viewport, Cm_Viewport, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_HorizontalScrollbar, Cm_HorizontalScrollbar, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_VerticalScrollbar, Cm_VerticalScrollbar, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_HorizontalScrollbarVisibility, Cm_HorizontalScrollbarVisibility, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_VerticalScrollbarVisibility, Cm_VerticalScrollbarVisibility, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_HorizontalScrollbarSpacing, Cm_HorizontalScrollbarSpacing, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_VerticalScrollbarSpacing, Cm_VerticalScrollbarSpacing, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_onValueChanged, Cm_onValueChanged, new GUILayoutOption[0]);


        EditorGUILayout.Space();

        LoopScrollRect scroll = (LoopScrollRect)target;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(C_totalCount);
        scroll.totalCount = EditorGUILayout.IntField(scroll.totalCount, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Cthreshold);
        scroll.threshold = EditorGUILayout.FloatField(scroll.threshold, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        // EditorGUILayout.LabelField(CreverseDirection);
        // scroll.reverseDirection = EditorGUILayout.Toggle(scroll.reverseDirection,  new GUILayoutOption[0]);
        //  EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(m__reverseDirection, CreverseDirection, new GUILayoutOption[0]);

        if (m__reverseDirection.boolValue && scroll.content && ((scroll is LoopHorizontalScrollRect && scroll.content.pivot.x != 0) || (scroll is LoopVerticalScrollRect && scroll.content.pivot.y != 0)))
        {
            EditorGUILayout.HelpBox("反向滚动请将content.pivot设为0", MessageType.Error);
        }
        else if (!m__reverseDirection.boolValue && scroll.content && ((scroll is LoopHorizontalScrollRect && scroll.content.pivot.x != 1) || (scroll is LoopVerticalScrollRect && scroll.content.pivot.y != 1)))
        {
            // EditorGUILayout.HelpBox("正向滚动请将content.pivot设为1", MessageType.Error);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(CrubberScale);
        scroll.rubberScale = EditorGUILayout.FloatField(scroll.rubberScale, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("清除"))
        {
            scroll.ClearCells();
        }

        if (GUILayout.Button("填充RefillCells"))
        {
            scroll.RefillCells(null,null, scroll.totalCount);
        }
        if (GUILayout.Button("从尾部填充"))
        {
            scroll.RefillCellsFromEnd(null,null,scroll.totalCount);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新Refresh"))
        {
            scroll.RefreshCells();
        }
        if (GUILayout.Button("滑到尾部"))
        {
            scroll.MoveToEnd(true);
        }
        if (GUILayout.Button("滑到尾部-无动画"))
        {
            scroll.MoveToEnd(false);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Update Bounds"))
        {
            scroll.UpdateBounds(true);
        }
        
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();//end
        base.OnInspectorGUI();
    }
}