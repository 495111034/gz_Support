using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

namespace WeatherSystem
{
    [CustomEditor(typeof(WeatherSystemScript))]
    public class WeatherSystemScriptInspector : Editor
    {
        // Editor only
        public Texture2D logoTexture;
        private WeatherSystemScript m_target;
        private Rect m_controlRect;
        private readonly Color m_greenColor = new Color(0.85f, 1.0f, 0.85f);
        private readonly Color m_redColor = new Color(1.0f, 0.75f, 0.75f);
        private GUIStyle m_textBarStyle;

        // GUIContents
        private readonly GUIContent[] m_guiContent = new[]
        {
            new GUIContent("Sun Transform", "The Transform used to simulate the sun position in the sky."),
            new GUIContent("Moon Transform", "The Transform used to simulate the moon position in the sky."),
            new GUIContent("Directional Light", "The directional light used to apply the lighting of the sun and moon to the scene."),
            new GUIContent("Main Camera", "The main camera used to the scene."),
            new GUIContent("Reflection Probe", "The reflection probe used to compute the sky reflection."),
            new GUIContent("Default Day Profiles", "Stores the default day profiles. A random profile from this list will be used by sky system every time the next day starts."),
            new GUIContent("Go", "Changes the global weather to this specific profile in the list."),
            new GUIContent("Global Weather Profiles", "Stores the profiles used to control the global climate."),
            new GUIContent("Local Weather Zones", "Place here all the local weather zones and arrange according to its priorities."),
            new GUIContent("Trigger", "Transform that will drive the local weather zone blending feature. Setting this field to 'null' will disable local weather zones (global one will still work)."),
            new GUIContent("Mie Depth", "Sets the Mie distance range."),
            new GUIContent("Fog Material", "The material used to render the fog scattering."),
            new GUIContent("Empty Sky Shader", "The shader used to render the empty sky."),
            new GUIContent("Static Cloud Shader", "The shader used to render the static clouds."),
            new GUIContent("Dynamic Cloud Shader", "The shader used to render the dynamic clouds."),
            new GUIContent("Scattering Mode", "Sets how the scattering color will be performed. You can set a custom scattering color by editing the 'Scattering Color' property from the 'Scattering' tab of each day profile."),
            new GUIContent("Cloud Mode", "Sets the cloud mode."),
            new GUIContent("Day Transition Time", "Sets the duration in seconds of the default day profiles transition when the time changes to the next calendar day at 24 o'clock." +
                                                  " If a global weather profile is in use, the transition will not be performed."),
            new GUIContent("Output Profile", "Place here the output profile that stores the extra properties you want to include in this sky controller. Note that the day profiles must be using this same output profile."),
            new GUIContent("Shader Update Mode", "How should shader uniforms be updated? Select 'By Material' if you want to use multiple views showing different sky settings."),
            new GUIContent("FlareLen Profile", "FlareLen Profile."),
            new GUIContent("Terrain Center", "The Terrain Center for raining."),
            new GUIContent("Terrain Size", "The Terrain Size for raining."),
            new GUIContent("Terrain Min Height", "The Min Terrain Height for raining."),
            new GUIContent("Terrain Max Height", "The Max Terrain Height for raining."),
            new GUIContent("Terrain HeightMap", "The Terrain HeightMap for raining."),
    };

        private Vector3 m_starFieldPosition = Vector3.zero;
        private Vector3 m_starFieldColor = Vector3.one;

        // Serialized properties
        private SerializedProperty m_showReferencesHeaderGroup;
        private SerializedProperty m_showProfilesHeaderGroup;
        private SerializedProperty m_showOptionsHeaderGroup;
        private SerializedProperty m_directionalLight;
        private SerializedProperty m_mainCamera;
        
        private SerializedProperty m_defaultProfileList;
        private SerializedProperty m_globalWeatherList;

        private SerializedProperty m_timeOfDay;
        private SerializedProperty m_global;

        // Reorderable lists
        private ReorderableList m_reorderableDefaultProfileList;
        private ReorderableList m_reorderableGlobalWeatherList;
        private ReorderableList m_reorderableWeatherZoneList;
           

        private T GetDragObject<T>(Rect rect, Event @event, int index ) where T : UnityEngine.Object
        {
            T @object = default;
            if (rect.Contains(@event.mousePosition))
            {
                if (DragAndDrop.objectReferences.Length > 0)
                {
                    if (DragAndDrop.objectReferences[0].GetType() == typeof(T))
                    {
                        EventType eventType = Event.current.type;
                        if (eventType == EventType.DragUpdated ||
                            eventType == EventType.DragPerform)
                        {
                            // Show a copy icon on the drag
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (eventType == EventType.DragPerform)
                            {
                                DragAndDrop.activeControlID = index;
                                DragAndDrop.AcceptDrag();
                                GUI.changed = true;
                                @object = (T)DragAndDrop.objectReferences[0];
                            }
                            Event.current.Use();
                        }
                    }
                    else
                    {
                        
                        DragAndDrop.visualMode = DragAndDropVisualMode.None;
                    }
                }
            }
            return @object;
        }
        private void OnEnable()
        {
            // Get target
            m_target = (WeatherSystemScript)target;

            // Find the serialized properties
            m_showReferencesHeaderGroup = serializedObject.FindProperty("ShowReferencesHeaderGroup");
            m_showProfilesHeaderGroup = serializedObject.FindProperty("ShowProfilesHeaderGroup");
            m_showOptionsHeaderGroup = serializedObject.FindProperty("ShowOptionsHeaderGroup");            
            m_directionalLight = serializedObject.FindProperty("DirectionalLight");
            m_mainCamera = serializedObject.FindProperty("MainCamera");

            m_defaultProfileList = serializedObject.FindProperty("DefaultProfileList");
            m_globalWeatherList = serializedObject.FindProperty("GlobalWeatherList");
            

            m_timeOfDay = serializedObject.FindProperty("TimeOfDay");
            m_global = serializedObject.FindProperty("Global");


            // Create default profile list
            m_reorderableDefaultProfileList = new ReorderableList(serializedObject, m_defaultProfileList, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    Rect fieldRect = new Rect(rect.x + 65, rect.y, rect.width - 65, EditorGUIUtility.singleLineHeight);

                    // Profile index
                    EditorGUI.LabelField(rect, "Profile " + index.ToString());

                    // Object field
                    GUI.color = m_greenColor;
                    if (!m_target.DefaultProfileList[index]) GUI.color = m_redColor;
                    EditorGUI.PropertyField(fieldRect, m_defaultProfileList.GetArrayElementAtIndex(index), GUIContent.none);
                    GUI.color = Color.white;
                },

                onAddCallback = (ReorderableList l) =>
                {
                    var index = l.serializedProperty.arraySize;
                    l.serializedProperty.arraySize++;
                    l.index = index;
                },

                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, m_guiContent[5], EditorStyles.boldLabel);
                },

                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    if (active)
                        GUI.Box(new Rect(rect.x + 2, rect.y, rect.width - 4, rect.height + 1), "", "selectionRect");
                }
            };



            // Create global weather list
            m_reorderableGlobalWeatherList = new ReorderableList(serializedObject, m_globalWeatherList, true, false, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    Rect fieldRect = new Rect(rect.x + 65, rect.y, rect.width - 100 - 28- 28 - 28, EditorGUIUtility.singleLineHeight);
                    var element = m_reorderableGlobalWeatherList.serializedProperty.GetArrayElementAtIndex(index);
                    var profile = element.FindPropertyRelative("profile");
                    var profilePath = element.FindPropertyRelative("profilePath");
                    var transition = element.FindPropertyRelative("transitionTime");
                    GUI.enabled = false;
                    // Profile index
                    EditorGUI.LabelField(rect, "Profile  " + index.ToString());
                    

                    // Object field
                    GUI.color = m_greenColor;
                    //if (!m_target.GlobalWeatherList[index].profile) GUI.color = m_redColor;
                    if (m_target.GlobalWeatherList[index].profile.Length == 0) GUI.color = m_redColor;
                    

                    var c_event = Event.current;
                    GUI.enabled = false;
                    profile.stringValue = EditorGUI.TextField(fieldRect, profile.stringValue);
                    GUI.enabled = true;

                    var c_audioClip = GetDragObject<WeatherSystemProfileScript>(fieldRect, c_event, 200 + index);
 
                     if (DragAndDrop.activeControlID == 200 + index && c_audioClip != null)
                     {
                         profile.stringValue =  c_audioClip.name;
                         profilePath.stringValue =  AssetDatabase.GetAssetPath(c_audioClip);
                         GUI.changed = true;
                     }


                    fieldRect = new Rect(rect.x + rect.width - 121, rect.y, 28, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(fieldRect, GUIContent.none, "IN ObjectField"))
                    {
                        EditorGUIUtility.ShowObjectPicker<WeatherSystemProfileScript>(null, false, "t:WeatherSystemProfileScript", 100 + index);
                    }

                    if (EditorGUIUtility.GetObjectPickerControlID() == 100 + index)
                    {
                        if (EditorGUIUtility.GetObjectPickerObject() != null)
                        {
                            profile.stringValue = EditorGUIUtility.GetObjectPickerObject().name;
                            profilePath.stringValue = AssetDatabase.GetAssetPath(EditorGUIUtility.GetObjectPickerObject());
                            GUI.changed = true;
                        }
                    }

                    // Ref button
                    fieldRect = new Rect(rect.x + rect.width - 91, rect.y, 28, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(fieldRect, "R"))
                    {
                        var playProfile = AssetDatabase.LoadAssetAtPath<WeatherSystemProfileScript>(profilePath.stringValue);
                        EditorGUIUtility.PingObject(playProfile);
                    }

                    

                    GUI.color = Color.white;

                    // Transition time field
                    fieldRect = new Rect(rect.x + rect.width - 61, rect.y, 28, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(fieldRect, transition, GUIContent.none);
                   
                    // Go button
                    fieldRect = new Rect(rect.x + rect.width - 30, rect.y, 30, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(fieldRect, m_guiContent[6]))
                    {                       
                        var playProfile = AssetDatabase.LoadAssetAtPath<WeatherSystemProfileScript>(profilePath.stringValue);
                        m_target.SetNewWeatherProfile(playProfile, transition.floatValue);                       
                    }
                },

                onAddCallback = (ReorderableList l) =>
                {
                    var index = l.serializedProperty.arraySize;
                    l.serializedProperty.arraySize++;
                    l.index = index;

                    var element = l.serializedProperty.GetArrayElementAtIndex(index);
                    element.FindPropertyRelative("transitionTime").floatValue = 10.0f;
                },

                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    //if (active)
                    //    GUI.Box(new Rect(rect.x + 2, rect.y, rect.width - 4, rect.height + 1), "", "selectionRect");
                }
            };

        }

        public override void OnInspectorGUI()
        {
            m_textBarStyle = new GUIStyle("WhiteMiniLabel")
            {
                alignment = TextAnchor.MiddleCenter,
                contentOffset = new Vector2(0, -3)
            };

            // Start custom Inspector
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // Logo
            m_controlRect = EditorGUILayout.GetControlRect();
            
            // References header group
            GUILayout.Space(2);
            m_showReferencesHeaderGroup.isExpanded = EditorGUILayout.Foldout(m_showReferencesHeaderGroup.isExpanded, "References");
            if (m_showReferencesHeaderGroup.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Light transform
                GUI.color = m_greenColor;
                if (!m_target.DirectionalLight) GUI.color = m_redColor;
                EditorGUILayout.PropertyField(m_directionalLight, m_guiContent[2]);

                GUI.color = m_greenColor;
                if (!m_target.MainCamera) GUI.color = m_redColor;
                EditorGUILayout.PropertyField(m_mainCamera, m_guiContent[3]);

                GUI.color = m_greenColor;
          
                EditorGUI.indentLevel--;
                GUI.color = Color.white;
            }
            //EditorGUILayout.EndFadeGroup();

            // Profiles header group
            GUILayout.Space(2);
            m_showProfilesHeaderGroup.isExpanded = EditorGUILayout.Foldout(m_showProfilesHeaderGroup.isExpanded, "Profiles");
            if (m_showProfilesHeaderGroup.isExpanded)
            {
                // Draw the default reorderable lists
                m_reorderableDefaultProfileList.DoLayoutList();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                // Progress bar
                GUILayout.Space(-5);
                m_controlRect = EditorGUILayout.GetControlRect();
                EditorGUI.ProgressBar(new Rect(m_controlRect.x + 1, m_controlRect.y - 2, m_controlRect.width - 2, m_controlRect.height - 4), m_target.GlobalWeatherTransitionProgress, "");
                EditorGUI.LabelField(new Rect(m_controlRect.x, m_controlRect.y - 2, m_controlRect.width, m_controlRect.height), "Transition Progress", m_textBarStyle);

                // Draw custom header for the global climate reorderable list
                GUILayout.Space(-6);
                EditorGUILayout.BeginHorizontal("RL Header");
                m_controlRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(new Rect(m_controlRect.x + 2, m_controlRect.y, m_controlRect.width, m_controlRect.height), m_guiContent[7], EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                // Draw the global climate reorderable list
                GUILayout.Space(2);
                m_reorderableGlobalWeatherList.DoLayoutList();
                EditorGUILayout.Space();
                GUI.color = m_greenColor;

               
                if (m_target.CurrentProfile)
                {
                    EditorGUILayout.LabelField(new GUIContent("Current Weather Profile :  " + m_target.CurrentProfile.name));
                }
                else if(m_target.DefaultProfile)
                {
                    EditorGUILayout.LabelField(new GUIContent("Current Weather Profile :  " + m_target.DefaultProfile.name));
                }
                else 
                {
                    EditorGUILayout.LabelField(new GUIContent("Current Weather Profile :  "));
                }

                GUI.color = Color.white;

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();


                // Draw the weather zone list
                GUILayout.Space(2);

                
                GUI.color = Color.white;
            }
            //EditorGUILayout.EndFoldoutHeaderGroup();

            // Events header group
            GUILayout.Space(2);
            // Options header group
            GUILayout.Space(2);
            m_showOptionsHeaderGroup.isExpanded = EditorGUILayout.Foldout(m_showOptionsHeaderGroup.isExpanded, "Options");
            if (m_showOptionsHeaderGroup.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_global, new GUIContent("Global"));

                EditorGUILayout.Slider(m_timeOfDay, 0, 24, new GUIContent("Time Of Day"));

                EditorGUI.indentLevel--;
            }
            //EditorGUILayout.EndFoldoutHeaderGroup();


            // End custom Inspector
            if (EditorGUI.EndChangeCheck() || GUI.changed)
            {
                Undo.RecordObject(m_target, "Undo Azure Sky Controller");
                serializedObject.ApplyModifiedProperties();
                //m_target.starFieldPosition = m_starFieldPosition;
                // m_target.starFieldColor = m_starFieldColor;
                //m_target.UpdateMaterialSettings();

                m_target.UpdateInEditor();
            }
        }

    }


}