
using UnityEngine;
using UnityEditor;

namespace MyParticle
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Spray))]
    public class SprayEditor : Editor
    {
        SerializedProperty _maxParticles;
        SerializedProperty _emitterCenter;
        SerializedProperty _emitterSize;
        SerializedProperty _throttle;
        SerializedProperty _totalSeconds;

        SerializedProperty _life;
        SerializedProperty _lifeRandomness;

        SerializedProperty _initialVelocity;
        SerializedProperty _directionSpread;
        SerializedProperty _speedRandomness;

        SerializedProperty _acceleration;
        SerializedProperty _drag;

        SerializedProperty _spin;
        SerializedProperty _speedToSpin;
        SerializedProperty _spinRandomness;

        SerializedProperty _noiseAmplitude;
        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseMotion;

        SerializedProperty _shapes;
        SerializedProperty _scale;
        SerializedProperty _scaleRandomness;
        SerializedProperty _material;
        SerializedProperty _castShadows;
        SerializedProperty _receiveShadows;

        SerializedProperty _Billboarding;
        SerializedProperty _VerticalBillboarding;

        //SerializedProperty _kernelShader;
        // SerializedProperty _debugShader;
        // SerializedProperty _defaultMaterial;

        SerializedProperty _randomSeed;
        SerializedProperty _debug;

        static GUIContent _textCenter    = new GUIContent("位置");
        static GUIContent _textSize      = new GUIContent("尺寸");
        static GUIContent _textParticleNumber = new GUIContent("最大粒子数量");
        static GUIContent _textEmitter = new GUIContent("发射器");
        static GUIContent _textThrottle = new GUIContent("控制阀门");
        static GUIContent _textMotion    = new GUIContent("噪音运动");
        static GUIContent _textAmplitude = new GUIContent("噪音振幅");
        static GUIContent _textFrequency = new GUIContent("噪音频率");
        static GUIContent _textTotalSeconds = new GUIContent("总时长（秒）");
        static GUIContent _textBillboardVertical = new GUIContent("广告牌垂直约束");

        void OnEnable()
        {
            _maxParticles  = serializedObject.FindProperty("_maxParticles");
            _emitterCenter = serializedObject.FindProperty("_emitterCenter");
            _emitterSize   = serializedObject.FindProperty("_emitterSize");
            _throttle      = serializedObject.FindProperty("_throttle");
            _totalSeconds = serializedObject.FindProperty("_total_seconds");

            _life           = serializedObject.FindProperty("_life");
            _lifeRandomness = serializedObject.FindProperty("_lifeRandomness");

            _initialVelocity = serializedObject.FindProperty("_initialVelocity");
            _directionSpread = serializedObject.FindProperty("_directionSpread");
            _speedRandomness = serializedObject.FindProperty("_speedRandomness");

            _acceleration = serializedObject.FindProperty("_acceleration");
            _drag         = serializedObject.FindProperty("_drag");

            _Billboarding   = serializedObject.FindProperty("_Billboarding");
            _VerticalBillboarding = serializedObject.FindProperty("_VerticalBillboarding");
            _spin           = serializedObject.FindProperty("_spin");
            _speedToSpin    = serializedObject.FindProperty("_speedToSpin");
            _spinRandomness = serializedObject.FindProperty("_spinRandomness");

            _noiseAmplitude = serializedObject.FindProperty("_noiseAmplitude");
            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseMotion    = serializedObject.FindProperty("_noiseMotion");

            _shapes          = serializedObject.FindProperty("_shapes");
            _scale           = serializedObject.FindProperty("_scale");
            _scaleRandomness = serializedObject.FindProperty("_scaleRandomness");
            _material        = serializedObject.FindProperty("_material");
            _castShadows     = serializedObject.FindProperty("_castShadows");
            _receiveShadows  = serializedObject.FindProperty("_receiveShadows");

           // _kernelShader = serializedObject.FindProperty("_kernelShader");            

            _randomSeed = serializedObject.FindProperty("_randomSeed");
            _debug      = serializedObject.FindProperty("_debug");
           // _debugShader = serializedObject.FindProperty("_debugShader");
           // _defaultMaterial = serializedObject.FindProperty("_defaultMaterial");

            _debug.boolValue = false;
            //_kernelShader.objectReferenceValue = Shader.Find("Hidden/MyParcitle/Kernel");
           // _debugShader.objectReferenceValue = Shader.Find("Hidden/MyParcitle/Debug");
            //_defaultMaterial.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("DefaultMaterial");
        }

        private float time = 0;
        private void Awake()
        {
            time = Time.realtimeSinceStartup;
            EditorApplication.update += UpdateHandler;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= UpdateHandler;
        }

        void UpdateHandler()
        {
            if(!Application.isPlaying)
            {
                var s = target as Spray;
                if (s!= null)
                {
                    float deltaTime = Time.realtimeSinceStartup - time;
                    time = Time.realtimeSinceStartup;

                    s.OnEditor_Update(deltaTime);
                }

                foreach (var sceneView in SceneView.sceneViews)
                {
                    if (sceneView is SceneView)
                    {
                        (sceneView as SceneView).Repaint();
                    }
                }
            }

        }

        public override void OnInspectorGUI()
        {
            var targetSpray = target as Spray;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_maxParticles, _textParticleNumber);
            if (!_maxParticles.hasMultipleDifferentValues) {
                var note = $"已初始化: {targetSpray.maxParticles}，顶点数：{targetSpray.meshVectexs}" ;
                EditorGUILayout.LabelField(" ", note, EditorStyles.miniLabel);
            }

            if (EditorGUI.EndChangeCheck())
                targetSpray.NotifyConfigChange();

            EditorGUILayout.LabelField(_textEmitter, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_emitterCenter, _textCenter);
            EditorGUILayout.PropertyField(_emitterSize, _textSize);
            EditorGUILayout.PropertyField(_throttle, _textThrottle);
            EditorGUILayout.PropertyField(_totalSeconds,_textTotalSeconds);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_life,new GUIContent("单粒子生命时长"));
            EditorGUILayout.PropertyField(_lifeRandomness, new GUIContent("生命时长随机种子"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("速度", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(_initialVelocity, new GUIContent("初始速度"));
            EditorGUILayout.PropertyField(_directionSpread, new GUIContent("方向"));
            EditorGUILayout.PropertyField(_speedRandomness, new GUIContent("随机性"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_acceleration, new GUIContent("加速"));
            EditorGUILayout.PropertyField(_drag, new GUIContent("阻力"));
            --EditorGUI.indentLevel;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_Billboarding, new GUIContent("广告牌"));

            if (!_Billboarding.boolValue)
            {                

                EditorGUILayout.LabelField("旋转", EditorStyles.boldLabel);

                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_spin, new GUIContent("转动方向"));
                EditorGUILayout.PropertyField(_speedToSpin, new GUIContent("转动速度"));
                EditorGUILayout.PropertyField(_spinRandomness, new GUIContent("随机性"));
                --EditorGUI.indentLevel;
            }
            else
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_textBillboardVertical, new GUILayoutOption[0]);
                _VerticalBillboarding.floatValue = EditorGUILayout.Slider(_VerticalBillboarding.floatValue, 0, 1, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                --EditorGUI.indentLevel;
            }

            targetSpray.SetBillboardMat(_Billboarding.boolValue);


            EditorGUILayout.Space();

            EditorGUILayout.LabelField("扰乱噪音", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(_noiseAmplitude, _textAmplitude);
            EditorGUILayout.PropertyField(_noiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_noiseMotion, _textMotion);
            --EditorGUI.indentLevel;

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();


            EditorGUILayout.LabelField("外观资源", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(_shapes,new GUIContent("模型"), true);

            if (EditorGUI.EndChangeCheck())
                targetSpray.NotifyConfigChange();

            EditorGUILayout.PropertyField(_scale, new GUIContent("缩放"));
            EditorGUILayout.PropertyField(_scaleRandomness,new GUIContent("缩放随机性"));

            EditorGUILayout.PropertyField(_material, new GUIContent("材质"));
            EditorGUILayout.PropertyField(_castShadows, new GUIContent("投射阴影（取决于shader）"));
            EditorGUILayout.PropertyField(_receiveShadows, new GUIContent("接收阴影（取决于shader）"));
            --EditorGUI.indentLevel;
            //EditorGUILayout.PropertyField(_kernelShader);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_randomSeed, new GUIContent("随机种子"));
            //EditorGUILayout.PropertyField(_debug);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
