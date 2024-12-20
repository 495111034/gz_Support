using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
using UnityEditor.Animations;
using UnityEngine.Profiling;

namespace GameSupport.EffectToolBox
{
    public class ArtToolsEffectChecker 
    {
        //-------------------特效预览---------------------
        private List<int> psCounts = new List<int>();
        private List<int> meshTrians = new List<int>();
        private List<Material> matCounts = new List<Material>();
        private List<Texture> texCounts = new List<Texture>();
        private List<GameObject> objCounts = new List<GameObject>();
        private List<Mesh> meshList = new List<Mesh>();

                
        private GameObject m_CurrEffect;       
        
        private int m_PsCount = 0;
        private int m_PsSysCount = 0;
        private int m_MeshTrian = 0;
        private int m_MeshPSCount = 0;
        private int m_TrailsSysCount = 0;
        private int m_CollisionSysCount = 0;
        private int m_MeshShapeSysCount = 0;
        private int m_MehsColliderCount = 0;
        private int m_MehsShadowCount = 0;
        private int m_TextureSizeCount = 0;
        private int m_SubMeshCount = 0;
        private int m_MeshMotionVectorCount = 0;
        private int m_MeshProbCount = 0;
        private float m_OverDraw = 0;
        private int m_StateNameCount = 0;
        private float m_MeshPsSize = 0f;
        private float m_PsSize = 0f; 
        private float m_ClipSize = 0f;
        private float m_MatSize = 0f;
        private int m_EffectScriptCount = 0;

        private GUIStyle style = new GUIStyle();
        


        private bool m_OnlyShowNotPass = true;
        private bool m_OverDrawCount = false;
        private bool m_OverDrawView = true;

        private int m_QaulityLevel = 0;

        private string m_CullCheckStr = "";
        private Dictionary<GameObject, string> m_WaringCheckStr = new Dictionary<GameObject, string>();
        private List<string> m_TexWaringCheckStr = new List<string>();
        private EffectEvla m_EffectEvla = null;
        private List<string> IllegalNameList = new List<string>();

        //private struct WaringCheckData
        //{
        //    public string str;
        //    public GameObject obj;
        //    public WaringCheckData(string s, GameObject go)
        //    {
        //        str = s;
        //        obj = go;
        //    }
        //}

        public struct QualitySettings
        {
            public int psCount;
            public int psSysCount;
            public int matCount;
            public int texCount;
            public int texSizeCount;
            public int meshTrian;
            public int meshPSCount;
            public int trailsSysCount;
            public int collisionSysCount;
            public int meshShapeSysCount;
            public int mehsColliderCount;
            public int mehsShadowCount;
            public int subMeshCount;
            public int meshMotionVectorCount;
            public int meshProbCount;
            public float overDraw;
            public int stateNameCount;
            public int effectScriptCount;

            public static QualitySettings[] presetQualitySettings =
            {
                // Low_LOD0
                new QualitySettings
                {
                     psCount = 30,
                     psSysCount = 5,
                     matCount = 5,
                     texCount = 5,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 0,
                     collisionSysCount = 0,
                     meshShapeSysCount = 2,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
        },
                // Low_LOD1
                new QualitySettings
                {
                     psCount = 20,
                     psSysCount = 3,
                     matCount = 3,
                     texCount = 3,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 0,
                     collisionSysCount = 0,
                     meshShapeSysCount = 1,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
                },
                // Low_LOD2
                new QualitySettings
                {
                     psCount = 10,
                     psSysCount = 2,
                     matCount = 2,
                     texCount = 2,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 0,
                     collisionSysCount = 0,
                     meshShapeSysCount = 1,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
                },

                // Medium_LOD0
                new QualitySettings
                {
                     psCount = 40,
                     psSysCount = 10,
                     matCount = 10,
                     texCount = 10,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 1,
                     collisionSysCount = 0,
                     meshShapeSysCount = 3,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
                },

                // Medium_LOD1
                new QualitySettings
                {
                     psCount = 20,
                     psSysCount = 5,
                     matCount = 5,
                     texCount = 5,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 1,
                     collisionSysCount = 0,
                     meshShapeSysCount = 2,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
                },

                // Medium_LOD2
                new QualitySettings
                {
                     psCount = 10,
                     psSysCount = 3,
                     matCount = 2,
                     texCount = 2,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 0,
                     collisionSysCount = 0,
                     meshShapeSysCount = 1,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
                },

                // High_LOD0
                new QualitySettings
                {
                     psCount = 60,
                     psSysCount = 15,
                     matCount = 15,
                     texCount = 15,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 2,
                     collisionSysCount = 0,
                     meshShapeSysCount = 5,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,

                },
                // High_LOD1
                new QualitySettings
                {
                     psCount = 30,
                     psSysCount = 8,
                     matCount = 8,
                     texCount = 8,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 1,
                     collisionSysCount = 0,
                     meshShapeSysCount = 3,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
                },

                // High_LOD2
                new QualitySettings
                {
                     psCount = 4,
                     psSysCount = 15,
                     matCount = 4,
                     texCount = 4,
                     texSizeCount = 1024,

                     meshTrian = 0,
                     meshPSCount = 0,

                     trailsSysCount = 0,
                     collisionSysCount = 0,
                     meshShapeSysCount = 2,

                     mehsColliderCount = 0,
                     mehsShadowCount = 0,
                     subMeshCount = 0,
                     meshMotionVectorCount = 0,
                     meshProbCount = 0,
                     overDraw = 0,
                     stateNameCount = 0,
                     effectScriptCount = 1,
                }
            };
        }


        private Vector2 scrollPos = new Vector2();
        public void Refresh()
        {
            m_MeshTrian = 0;
            m_PsCount = 0;
            m_OverDraw = 0;
            m_OverDraw = 0;
            if (m_EffectEvla != null)
            {
                m_EffectEvla.Reset();
            }
        }


        public void OnTotalGUI()
        {
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Toggle("Show UnPass", m_OnlyShowNotPass);
            if (EditorGUI.EndChangeCheck())
            {
                m_OnlyShowNotPass = !m_OnlyShowNotPass;                
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Toggle("Show OverDraw", m_OverDrawCount);
            if (EditorGUI.EndChangeCheck())
            {
                m_OverDrawCount = !m_OverDrawCount;
                if(m_OverDrawCount)
                {
                    m_EffectEvla = new EffectEvla(Camera.main);
                    m_OverDraw = 0;
                    m_EffectEvla.Reset();
                    m_EffectEvla.SetOverDrawView(m_OverDrawView);
                }
                else
                {
                    Camera.main.ResetReplacementShader();
                    m_OverDraw = 0;
                    m_EffectEvla = null;
                }              
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Toggle("OverDraw View", m_OverDrawView);
            if (EditorGUI.EndChangeCheck())
            {
                m_OverDrawView = !m_OverDrawView;
                if(m_EffectEvla != null)
                {
                    m_EffectEvla.SetOverDrawView(m_OverDrawView);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();



            QualitySettings settings = QualitySettings.presetQualitySettings[m_QaulityLevel];            

            if (!m_OnlyShowNotPass || m_PsSysCount > settings.psSysCount)
                EditorGUILayout.LabelField("当前特效的粒子系统数量：    " + m_PsSysCount + "      建议值：" + settings.psSysCount);
            if (!m_OnlyShowNotPass || m_PsCount >settings.psCount)
                EditorGUILayout.LabelField("当前特效的最大粒子数量：    " + m_PsCount + "      建议值：" + settings.psCount);
            if (!m_OnlyShowNotPass || matCounts.Count >settings.matCount)
                EditorGUILayout.LabelField("当前特效的材质数量：         " + matCounts.Count + "      建议值：" + settings.matCount);
            if (!m_OnlyShowNotPass || texCounts.Count > settings.texCount)
                EditorGUILayout.LabelField("当前特效的贴图数量：         " + texCounts.Count + "      建议值：" + settings.texCount);
            if (!m_OnlyShowNotPass || m_OverDraw > settings.overDraw)
                EditorGUILayout.LabelField("当前特效OverDraw数量：" + m_OverDraw + "      建议值：" + settings.overDraw);



            EditorGUILayout.Space();
            EditorGUILayout.Space();
            style.normal.textColor = Color.red;
            var meshTrian = (m_MeshTrian / 3) + (m_PsCount * 2);
            if (!m_OnlyShowNotPass || meshTrian > settings.meshTrian)
                EditorGUILayout.LabelField("当前特效大概的三角面数：    " + meshTrian + "     建议值：" + settings.meshTrian);
            if (!m_OnlyShowNotPass || m_MeshPSCount > settings.meshPSCount)
                EditorGUILayout.LabelField("当前特效发射Mesh的数量：  " + m_MeshPSCount + "     建议值：" + settings.meshPSCount);
            if (!m_OnlyShowNotPass || m_TrailsSysCount >settings.trailsSysCount)
                EditorGUILayout.LabelField("当前特效Trails模块数量：    " + m_TrailsSysCount + "      建议值：" + settings.trailsSysCount, style);
            if (!m_OnlyShowNotPass || m_CollisionSysCount >settings.collisionSysCount)
                EditorGUILayout.LabelField("当前特效Collision模块数量：" + m_CollisionSysCount + "      建议值：" + settings.collisionSysCount, style);
            if (!m_OnlyShowNotPass || m_MeshShapeSysCount >settings.meshShapeSysCount)
                EditorGUILayout.LabelField("当前特效Shape喷射器数量：   " + m_MeshShapeSysCount + "      建议值：" + settings.meshShapeSysCount);

            if (!m_OnlyShowNotPass || m_MehsShadowCount >settings.mehsShadowCount)
                EditorGUILayout.LabelField("当前特效投影接受阴影数量： " + m_MehsShadowCount + "      建议值：" + settings.mehsShadowCount, style);
            if (!m_OnlyShowNotPass || m_MehsColliderCount >settings.mehsColliderCount)
                EditorGUILayout.LabelField("当前特效MeshCollider数量：" + m_MehsColliderCount + "      建议值：" + settings.mehsColliderCount);

            if (!m_OnlyShowNotPass || m_SubMeshCount > settings.subMeshCount)
                EditorGUILayout.LabelField("当前特效使用SubMesh数量：" + m_SubMeshCount + "      建议值：" + settings.subMeshCount, style);

            if (!m_OnlyShowNotPass || m_MeshMotionVectorCount > settings.meshMotionVectorCount)
                EditorGUILayout.LabelField("当前特效MotionVectors数量：" + m_MeshMotionVectorCount + "      建议值：" + settings.meshMotionVectorCount, style);

            if (!m_OnlyShowNotPass || m_MeshProbCount > settings.meshProbCount)
                EditorGUILayout.LabelField("当前特效L&R Prob数量：" + m_MeshProbCount + "      建议值：" + settings.meshProbCount, style);

            if (!m_OnlyShowNotPass || m_StateNameCount > settings.stateNameCount)
                EditorGUILayout.LabelField("当前特效状态机非Effect的数量：" + m_StateNameCount + "      建议值：" + settings.stateNameCount, style);

            if (!m_OnlyShowNotPass || m_EffectScriptCount > settings.effectScriptCount)
                EditorGUILayout.LabelField("当前特效EffectScript的数量：" + m_EffectScriptCount, style);


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (!m_OnlyShowNotPass || m_TextureSizeCount > settings.texSizeCount)
                EditorGUILayout.LabelField("当前特效的贴图大小：         " + m_TextureSizeCount + "KB      建议值：" + settings.texSizeCount + "KB");
            EditorGUILayout.LabelField("当前特效的Mesh大小：" + Math.Round(m_MeshPsSize, 2) + "KB");
            EditorGUILayout.LabelField("当前特效的AnimationClip大小：" + Math.Round(m_ClipSize, 2) + "KB");
            EditorGUILayout.LabelField("当前特效的材质大小：" + Math.Round(m_MatSize, 2) + "KB");
            EditorGUILayout.LabelField("当前特效的粒子系统大小：" + Math.Round(m_PsSize, 2) + "KB");
            EditorGUILayout.LabelField("以上合计：" + Math.Round(m_PsSize + m_MatSize + m_ClipSize + m_MeshPsSize + m_TextureSizeCount, 2) + "KB");


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("特效中使用的以下资源名称存在空格：");
            EditorGUILayout.BeginVertical();
            foreach (var str in IllegalNameList)
            {
                GUIStyle sty = new GUIStyle(GUI.skin.label);
                sty.normal.textColor = Color.red;
                if (GUILayout.Button(str, sty))
                {
                    UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(str);
                    Selection.activeObject = go;
                }
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("特效中使用的以下贴图分辨率可能过大：");
            //GUILayout.Label(m_TexWaringCheckStr);
            EditorGUILayout.BeginVertical();
            foreach(var str in m_TexWaringCheckStr)
            {
                if (GUILayout.Button(str, GUI.skin.label))
                {
                    UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(str.Split('\n')[0]);
                    Selection.activeObject = go;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ParticleSystem以下选项会性能问题：");
            foreach(var item in m_WaringCheckStr)
            {
                if (item.Key != null)
                {
                    if (GUILayout.Button(string.Format("{0}:{1}", item.Key.name, item.Value), GUI.skin.label))
                    {
                        Selection.activeObject = item.Key;
                    }
                }
                
            }
            //GUILayout.Label(m_WaringCheckStr);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ParticleSystem以下选项会导致无法自动剔除：");
            GUILayout.Label(m_CullCheckStr);
        }
        public void OnGUI()
        {       
            if (objCounts.Count > 0)
            {
                foreach (var obj in objCounts)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical(GUILayout.Width(200));
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), false, GUILayout.Width(200));
                    if (obj)
                    {
                        var ps = obj.GetComponent<ParticleSystem>();
                        if (ps)
                        {
                            if (ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.Mesh && ps.GetComponent<ParticleSystemRenderer>().mesh != null)
                            {
                                EditorGUILayout.LabelField("PS.Mesh的面数: " + (ps.GetComponent<ParticleSystemRenderer>().mesh.triangles.Length / 3));
                            }
                        }
                        var meshF = obj.GetComponent<MeshFilter>();
                        if (meshF)
                        {
                            if (meshF.sharedMesh)
                            {
                                EditorGUILayout.LabelField("Mesh的面数: " + (meshF.sharedMesh.triangles.Length / 3));
                            }
                        }
                        var meshSkinF = obj.GetComponent<SkinnedMeshRenderer>();
                        if (meshSkinF)
                        {
                            if (meshSkinF.sharedMesh)
                            {
                                EditorGUILayout.LabelField("Mesh的面数: " + (meshSkinF.sharedMesh.triangles.Length / 3));
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    if (obj != null)
                    {
                        var rend = obj.GetComponent<Renderer>();
                        var matS = rend.sharedMaterials;
                        if (matS.Length > 0)
                        {
                            foreach (var mat in matS)
                            {
                                if (mat != null)
                                {
                                    var matLB = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(200));
                                    int count = ShaderUtil.GetPropertyCount(mat.shader);
                                    for (int i = 0; i < count; i++)
                                    {
                                        var matShaderType = ShaderUtil.GetPropertyType(mat.shader, i);
                                        if (ShaderUtil.ShaderPropertyType.TexEnv == matShaderType)
                                        {
                                            var assetMatShaderProName = ShaderUtil.GetPropertyName(mat.shader, i);
                                            var tex = mat.GetTexture(assetMatShaderProName);
                                            if (tex != null)
                                            {
                                                EditorGUILayout.LabelField(tex.name + ":" + tex.height + "*" + tex.width );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }
        }

        //------------------------特效预览------------------------------------------
        void OnDestroy()
        {
            ListClear();
            m_CurrEffect = null;       
        }

        void ListClear()
        {
            m_WaringCheckStr.Clear();
            m_TexWaringCheckStr.Clear();
            IllegalNameList.Clear();
            meshList.Clear();
            meshTrians.Clear();
            psCounts.Clear();
            m_MeshPSCount = 0;
            matCounts.Clear();
            texCounts.Clear();
            objCounts.Clear();
            m_TrailsSysCount = 0;
            m_CollisionSysCount = 0;
            m_MeshShapeSysCount = 0;
            m_MehsColliderCount = 0;
            m_MehsShadowCount = 0;
            m_SubMeshCount = 0;
            m_MeshProbCount = 0;
            m_MeshMotionVectorCount = 0;
            m_StateNameCount = 0;
            m_ClipSize = 0f;
            m_MatSize = 0f;
            m_EffectScriptCount = 0;

            m_MeshPsSize = 0f;
            m_PsSize = 0f;
        }

        public static void BatchAddStartDelay(GameObject go, float delay)
        {
            var psS = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psS)
            {
                var maindata = ps.main;
                maindata.startDelay = new ParticleSystem.MinMaxCurve(maindata.startDelay.constant + delay);
                if (maindata.startDelay.constant < 0.0001f)
                {
                    maindata.startDelay = 0.0f;
                }
            }
        }

        public static void BatchRemoveTrailMat(GameObject go)
        {
            var psS = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psS)
            {
                if (ps.GetComponent<ParticleSystemRenderer>().trailMaterial != null)
                {
                    ps.GetComponent<ParticleSystemRenderer>().trailMaterial = null;
                }
            }
        }

        public static void BatchOptimizationScalingMode(GameObject go)
        {
            var psS = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psS)
            {
                var maindata = ps.main;
                maindata.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
        }
        public static void BatchRepair(GameObject go)
        {
            if (go == null)
                return;

            var animators = go.GetComponentsInChildren<Animator>(true);
            AnimatorControllerLayer[] acLayers;
            ChildAnimatorState[] ch_animStates;
            AnimatorController ac;
            AnimatorStateMachine stateMachine;
            foreach (var animator in animators)
            {
                if (animator.runtimeAnimatorController != null)
                {
                    ac = animator.runtimeAnimatorController as AnimatorController;
                    if (ac != null)
                    {
                        acLayers = ac.layers;

                        foreach (AnimatorControllerLayer i in acLayers) //for each layer
                        {
                            stateMachine = i.stateMachine;
                            ch_animStates = null;
                            ch_animStates = stateMachine.states;
                            foreach (ChildAnimatorState j in ch_animStates) //for each state
                            {
                                if (j.state.name.ToLower() != "effect")
                                {
                                    j.state.name = "Effect";
                                }
                            }
                        }
                    }
                }
            }
            EditorUtility.SetDirty(go);
            AssetDatabase.SaveAssets();
        }
        public static void RemoveEffectScript(GameObject go)
        {
            var trans = go.GetComponentsInChildren<Transform>(true);
            int scriptCount = 0;
            foreach (var tran in trans)
            {
                var comp = tran.GetComponent("EffectScript");
                if (comp != null)
                {
                    if (scriptCount > 0)
                    {
                        GameObject.DestroyImmediate(comp);
                    }
                    else
                    {
                        scriptCount++;
                    }
                }
            }
        }
        public static void BatchOptimizationRes(GameObject go)
        {
            // 找到所有子节点包含Mesh的对象
            var meshRenders = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mesh in meshRenders)
            {
                if (mesh.receiveShadows || mesh.shadowCastingMode != ShadowCastingMode.Off)
                {
                    mesh.receiveShadows = false;
                    mesh.shadowCastingMode = ShadowCastingMode.Off;
                }

                if (mesh.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                {
                    mesh.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                }

                if (mesh.lightProbeUsage != LightProbeUsage.Off || mesh.reflectionProbeUsage != ReflectionProbeUsage.Off)
                {
                    mesh.lightProbeUsage = LightProbeUsage.Off;
                    mesh.reflectionProbeUsage = ReflectionProbeUsage.Off;
                }
            }

            var meshsColliders = go.GetComponentsInChildren<Collider>(true);
            foreach (var meshCollider in meshsColliders)
            {
                GameObject.DestroyImmediate(meshCollider, true);
            }

            var psS = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psS)
            {
                var main = ps.main;
                main.prewarm = false;

                if (ps.shape.enabled && ps.shape.meshRenderer && (ps.shape.shapeType == ParticleSystemShapeType.Mesh || ps.shape.shapeType == ParticleSystemShapeType.SkinnedMeshRenderer || ps.shape.shapeType == ParticleSystemShapeType.MeshRenderer))
                {
                    ps.shape.meshRenderer.receiveShadows = false;
                    ps.shape.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                if (ps.GetComponent<ParticleSystemRenderer>().shadowCastingMode != ShadowCastingMode.Off || ps.GetComponent<ParticleSystemRenderer>().receiveShadows)
                {
                    ps.GetComponent<ParticleSystemRenderer>().receiveShadows = false;
                    ps.GetComponent<ParticleSystemRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                }
                if (ps.GetComponent<ParticleSystemRenderer>().motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                {
                    ps.GetComponent<ParticleSystemRenderer>().motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                }
                if (ps.GetComponent<ParticleSystemRenderer>().lightProbeUsage != LightProbeUsage.Off || ps.GetComponent<ParticleSystemRenderer>().reflectionProbeUsage != ReflectionProbeUsage.Off)
                {
                    ps.GetComponent<ParticleSystemRenderer>().lightProbeUsage = LightProbeUsage.Off;
                    ps.GetComponent<ParticleSystemRenderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
                }
            }
        }

        void OptimizationCheckRes(GameObject go, GameObject currAsset)
        {
            if (currAsset == null)
            {
                return;
            }
            ListClear();
            m_WaringCheckStr = GetWaringSupportedString(go);
            m_CullCheckStr = GetCullingSupportedString(go);

            var objTranS = go.GetComponentsInChildren<Transform>(true);
            if (objTranS.Length > 0)
            {
                foreach (var objTran in objTranS)
                {
                    var obj = objTran.gameObject;
                    var rend = obj.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        if (!objCounts.Contains(obj))
                        {
                            objCounts.Add(obj);
                        }
                    }
                    var effectScript = obj.GetComponent("EffectScript");
                    if (effectScript != null)
                    {
                        m_EffectScriptCount++;
                    }
                }
            }

            var meshs = go.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mesh in meshs)
            {
                if (mesh.sharedMesh != null)
                {
                    meshTrians.Add(mesh.sharedMesh.triangles.Length);
                }
            }

            var meshRenders = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mesh in meshRenders)
            {
                if (mesh.receiveShadows)
                {
                    m_MehsShadowCount++;
                }
                if (mesh.shadowCastingMode != ShadowCastingMode.Off)
                {
                    m_MehsShadowCount++;
                }

                if (mesh.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                {
                    m_MeshMotionVectorCount++;
                }

                if (mesh.lightProbeUsage != LightProbeUsage.Off )
                {
                    m_MeshProbCount++;
                }
                if (mesh.reflectionProbeUsage != ReflectionProbeUsage.Off)
                {
                    m_MeshProbCount++;
                }
            }

            var meshsCollider = go.GetComponentsInChildren<Collider>(true);
            m_MehsColliderCount = meshsCollider.Length;
            foreach (var ps in currAsset.GetComponentsInChildren<ParticleSystem>())
            {
                m_PsSize += Profiler.GetRuntimeMemorySizeLong(ps);
            }

            var psS = go.GetComponentsInChildren<ParticleSystem>();
            m_PsSysCount = psS.Length;
            foreach (var ps in psS)
            {
                var psConut = ps.GetParticles(new ParticleSystem.Particle[ps.main.maxParticles]);
                if (ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.Mesh && ps.GetComponent<ParticleSystemRenderer>().mesh != null)
                {
                   meshTrians.Add(ps.GetComponent<ParticleSystemRenderer>().mesh.triangles.Length * psConut);
                   
                   string sm_path = AssetDatabase.GetAssetPath(ps.GetComponent<ParticleSystemRenderer>().mesh.GetInstanceID());
                   var mesh = AssetDatabase.LoadAssetAtPath<GameObject>(sm_path);
                   if(mesh)
                   {    
                        var count = mesh.GetComponentsInChildren<MeshFilter>().Length;
                        if(count > 1)
                        {
                            m_SubMeshCount += count;
                        }
                   }                    
                    m_MeshPSCount = m_MeshPSCount + 1;
                    if (!meshList.Contains(ps.GetComponent<ParticleSystemRenderer>().mesh))
                    {
                        meshList.Add(ps.GetComponent<ParticleSystemRenderer>().mesh);
                    }
                }
                if (ps.trails.enabled)
                {
                    m_TrailsSysCount++;
                }
                if (ps.collision.enabled)
                {
                    m_CollisionSysCount++;
                }
                if (ps.shape.enabled && (ps.shape.shapeType == ParticleSystemShapeType.Mesh || ps.shape.shapeType == ParticleSystemShapeType.SkinnedMeshRenderer || ps.shape.shapeType == ParticleSystemShapeType.MeshRenderer))
                {
                    m_MeshShapeSysCount++;                    
                }
                if (ps.GetComponent<ParticleSystemRenderer>().shadowCastingMode != ShadowCastingMode.Off )
                {
                    m_MehsShadowCount++;
                }
                if (ps.GetComponent<ParticleSystemRenderer>().receiveShadows)
                {
                    m_MehsShadowCount++;
                }
                if (ps.GetComponent<ParticleSystemRenderer>().motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                {
                    m_MeshMotionVectorCount++;
                }
                if (ps.GetComponent<ParticleSystemRenderer>().lightProbeUsage != LightProbeUsage.Off)
                {
                    m_MeshProbCount++;
                }
                if (ps.GetComponent<ParticleSystemRenderer>().reflectionProbeUsage != ReflectionProbeUsage.Off)
                {
                    m_MeshProbCount++;
                }
                var psNum = ps.GetParticles(new ParticleSystem.Particle[ps.main.maxParticles]);
                psCounts.Add(psNum);
            }
            foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>())
            {
                if (!meshList.Contains(meshFilter.sharedMesh))
                {
                    meshList.Add(meshFilter.sharedMesh);
                }
            }
            foreach (var mesh in meshList)
            {
                m_MeshPsSize += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            m_MeshPsSize /= 1024f;
            m_PsSize /= 1024f;
            var skinMeshs = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var mesh in skinMeshs)
            {
                if (mesh.sharedMesh != null)
                {
                    meshTrians.Add(mesh.sharedMesh.triangles.Length);
                }
            }

            var animators = go.GetComponentsInChildren<Animator>(true);
            AnimatorControllerLayer[] acLayers;
            ChildAnimatorState[] ch_animStates;
            AnimatorController ac;
            AnimatorStateMachine stateMachine;
            foreach (var animator in animators)
            {
                if (animator.runtimeAnimatorController != null)
                {
                    ac = animator.runtimeAnimatorController as AnimatorController;
                    if (ac != null)
                    {
                        foreach (var clip in ac.animationClips)
                        {
                            CheckResName(clip);
                            m_ClipSize += Profiler.GetRuntimeMemorySizeLong(clip);
                        }
                        acLayers = ac.layers;

                        foreach (AnimatorControllerLayer i in acLayers) //for each layer
                        {
                            stateMachine = i.stateMachine;
                            ch_animStates = null;
                            ch_animStates = stateMachine.states;
                            foreach (ChildAnimatorState j in ch_animStates) //for each state
                            {
                                if(j.state.name.ToLower() != "effect")
                                {
                                    m_StateNameCount++;
                                }
                            }
                        }
                    }
                }
            }
            m_ClipSize /= 1024f;

            var rendS = go.GetComponentsInChildren<Renderer>(true);
            if (rendS.Length > 0)
            {
                foreach (var rend in rendS)
                {
                    var matS = rend.sharedMaterials;
                    if (matS.Length > 0)
                    {
                        foreach (var mat in matS)
                        {
                            if (mat != null)
                            {
                                if (!matCounts.Contains(mat))
                                {
                                    matCounts.Add(mat);
                                }
                            }
                            else
                            {
                                //Debug.LogError("这个预设有缺少材质！！！！！！！");
                            }
                        }
                    }
                }
            }

            if (matCounts.Count > 0)
            {
                m_TextureSizeCount = 0;
                string checkStr = "";
                foreach (var mat in matCounts)
                {
                    CheckResName(mat);
                    m_MatSize += Profiler.GetRuntimeMemorySizeLong(mat);
                    int count = ShaderUtil.GetPropertyCount(mat.shader);
                    for (int i = 0; i < count; i++)
                    {
                        var matShaderType = ShaderUtil.GetPropertyType(mat.shader, i);
                        if (ShaderUtil.ShaderPropertyType.TexEnv == matShaderType)
                        {
                            var assetMatShaderProName = ShaderUtil.GetPropertyName(mat.shader, i);
                            var tex = mat.GetTexture(assetMatShaderProName);
                            if (tex != null)
                            {
                                CheckResName(tex);
                                //查找list里是否存在某个对象
                                if (!texCounts.Contains(tex))
                                {
                                    texCounts.Add(tex);
                                    m_TextureSizeCount += GetParticleEffectData.GetStorageMemorySize(tex);
                                    string content = CheckTexSize(tex);
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        m_TexWaringCheckStr.Add(content);
                                    }
                                    //m_TextureSizeCount += (int)UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
                                }
                            }
                        }
                    }
                }
                m_TextureSizeCount /= 1024;
                m_MatSize /= 1024f;
            }


            if (meshTrians.Count > 0)
            {
                var psNum = 0;
                foreach (var item in meshTrians)
                {
                    psNum = psNum + item;
                }
                if (m_MeshTrian < psNum)
                {
                    m_MeshTrian = psNum;
                }
            }

            if (psCounts.Count > 0)
            {
                var psNum = 0;
                foreach (var item in psCounts)
                {
                    psNum = psNum + item;
                }
                if (m_PsCount < psNum)
                {
                    m_PsCount = psNum;
                }
            }
            if(m_EffectEvla != null)
            {
                m_EffectEvla.Update();
                EffectEvlaData[] effectEvlaData = m_EffectEvla.GetEffectEvlaData();
                float pixFillRate = (float)Math.Round(effectEvlaData[0].GetPixRate(), 2);
                if(m_OverDraw < pixFillRate)
                {
                    m_OverDraw = pixFillRate;
                }
            }
        }
        //名称包含空格
        private void CheckResName(UnityEngine.Object res)
        {
            if (res.name.Contains(" "))
            {
                string path = AssetDatabase.GetAssetPath(res);
                IllegalNameList.Add(path);
            }
        }
        private string CheckTexSize(Texture tex)
        {
            string waring = null;
            string path = AssetDatabase.GetAssetPath(tex);
            if (!string.IsNullOrEmpty(path))
            {
                string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
                var tImp = GetTexPlaSetting(path, platform);
                if (tImp != null && tImp.maxTextureSize > 512)
                {
                    waring = path + "\n    size > 512";
                }
            }
            return waring;
        }
        
        public static TextureImporter GetTexImport(string texPath)
        {
            var tImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (tImporter == null)
            {
                return null;
            }
            return tImporter;
        }

        
        public static TextureImporterPlatformSettings GetTexPlaSetting(string texPath, string platform)
        {
            var tImp = GetTexImport(texPath);
            if (tImp == null)
            {
                return null;
            }
            var plaSetting = tImp.GetPlatformTextureSettings(platform);
            Resources.UnloadAsset(tImp);
            return plaSetting;
        }
        public void OnSelectionChangeFunc(GameObject go, GameObject currAsset)
        {
            if (go == null)
            {
                return;
            }
            if (m_CurrEffect != go)
            {
                m_CurrEffect = go;
                if (true)
                {
                    m_QaulityLevel = 3;
                    var h = go.FindChild("H", true, false);
                    if (h && h.activeInHierarchy)
                    {
                        m_QaulityLevel = 6;
                    }
                }
                else
                {
                    if (m_CurrEffect.name.Contains("_L_LOD0"))
                    {
                        m_QaulityLevel = 0;
                    }
                    else if (m_CurrEffect.name.Contains("_L_LOD1"))
                    {
                        m_QaulityLevel = 1;
                    }
                    else if (m_CurrEffect.name.Contains("_L_LOD2"))
                    {
                        m_QaulityLevel = 2;
                    }
                    else if (m_CurrEffect.name.Contains("_M_LOD0"))
                    {
                        m_QaulityLevel = 3;
                    }
                    else if (m_CurrEffect.name.Contains("_M_LOD1"))
                    {
                        m_QaulityLevel = 4;
                    }
                    else if (m_CurrEffect.name.Contains("_M_LOD2"))
                    {
                        m_QaulityLevel = 5;
                    }
                    else if (m_CurrEffect.name.Contains("_H_LOD0"))
                    {
                        m_QaulityLevel = 6;
                    }
                    else if (m_CurrEffect.name.Contains("_H_LOD1"))
                    {
                        m_QaulityLevel = 7;
                    }
                    else if (m_CurrEffect.name.Contains("_H_LOD2"))
                    {
                        m_QaulityLevel = 8;
                    }
                }

                m_MeshTrian = 0;
                m_PsCount = 0;
                m_OverDraw = 0;
                if (m_EffectEvla != null)
                {
                    m_EffectEvla.Reset();                    
                }
            }
            OptimizationCheckRes(go, currAsset);
        }   

        static string CheckCulling(ParticleSystem particleSystem)
        {
            string text = "";
            if (particleSystem.collision.enabled)
            {
                text += "\n勾选了 Collision";
            }

            if (particleSystem.emission.enabled)
            {
                if (particleSystem.emission.rateOverDistance.curveMultiplier != 0)
                {
                    text += "\nEmission使用了Current(非线性运算)";
                }
            }

            if (particleSystem.externalForces.enabled)
            {
                text += "\n勾选了 External Forces";
            }

            if (particleSystem.forceOverLifetime.enabled)
            {
                if (GetIsRandomized(particleSystem.forceOverLifetime.x)
                    || GetIsRandomized(particleSystem.forceOverLifetime.y)
                    || GetIsRandomized(particleSystem.forceOverLifetime.z)
                    || particleSystem.forceOverLifetime.randomized)
                {
                    text += "\nForce Over Lifetime使用了Current(非线性运算)";
                }
            }
            if (particleSystem.inheritVelocity.enabled)
            {
                if (GetIsRandomized(particleSystem.inheritVelocity.curve))
                {
                    text += "\nInherit Velocity使用了Current(非线性运算)";
                }
            }
            if (particleSystem.noise.enabled)
            {
                text += "\n勾选了 Noise";
            }
            if (particleSystem.rotationBySpeed.enabled)
            {
                text += "\n勾选了 Rotation By Speed";
            }
            if (particleSystem.rotationOverLifetime.enabled)
            {
                if (GetIsRandomized(particleSystem.rotationOverLifetime.x)
                    || GetIsRandomized(particleSystem.rotationOverLifetime.y)
                    || GetIsRandomized(particleSystem.rotationOverLifetime.z))
                {
                    text += "\nRotation Over Lifetime使用了Current(非线性运算)";
                }
            }
            if (particleSystem.shape.enabled)
            {
                ParticleSystemShapeType shapeType = (ParticleSystemShapeType)particleSystem.shape.shapeType;
                switch (shapeType)
                {
                    case ParticleSystemShapeType.Cone:
                    case ParticleSystemShapeType.ConeVolume:
    #if UNITY_2018_1_OR_NEWER
                    case ParticleSystemShapeType.Donut:
    #endif
                    case ParticleSystemShapeType.Circle:
                        if (particleSystem.shape.arcMode != ParticleSystemShapeMultiModeValue.Random)
                        {
                            text += "\nShape的Circle-Arc使用了Random模式";
                        }
                        break;
                    case ParticleSystemShapeType.SingleSidedEdge:
                        if (particleSystem.shape.radiusMode != ParticleSystemShapeMultiModeValue.Random)
                        {
                            text += "\nShape的Edge-Radius使用了Random模式";
                        }
                        break;
                    default:
                        break;
                }
            }
            if (particleSystem.subEmitters.enabled)
            {
                text += "\n勾选了 SubEmitters";
            }
            if (particleSystem.trails.enabled)
            {
                text += "\n勾选了 Trails";
            }
            if (particleSystem.trigger.enabled)
            {
                text += "\n勾选了 Trigger";
            }
            if (particleSystem.velocityOverLifetime.enabled)
            {
                if (GetIsRandomized(particleSystem.velocityOverLifetime.x)
                    || GetIsRandomized(particleSystem.velocityOverLifetime.y)
                    || GetIsRandomized(particleSystem.velocityOverLifetime.z))
                {
                    text += "\nVelocity Over Lifetime使用了Current(非线性运算)";
                }
            }
            if (particleSystem.limitVelocityOverLifetime.enabled)
            {
                text += "\n勾选了 Limit Velocity Over Lifetime";
            }
            if (particleSystem.main.simulationSpace != ParticleSystemSimulationSpace.Local)
            {
                text += "\nSimulationSpace 不等于 Local";
            }
            if (particleSystem.main.gravityModifier.mode != 0)
            {
                text += "\nGravityModifier 不是常量";
            }
            return text;
        }

        static bool GetIsRandomized(ParticleSystem.MinMaxCurve minMaxCurve)
        {
            bool flag = AnimationCurveSupportsProcedural(minMaxCurve.curveMax);

            bool result;
            if (minMaxCurve.mode != ParticleSystemCurveMode.TwoCurves && minMaxCurve.mode != ParticleSystemCurveMode.TwoConstants)
            {
                result = flag;
            }
            else
            {
                bool flag2 = AnimationCurveSupportsProcedural(minMaxCurve.curveMin);
                result = (flag && flag2);
            }

            return result;
        }

        static bool AnimationCurveSupportsProcedural(AnimationCurve curve)
        {
            //switch (AnimationUtility.IsValidPolynomialCurve(curve)) //保护级别，无法访问，靠
            //{
            //    case AnimationUtility.PolynomialValid.Valid:
            //        return true;
            //    case AnimationUtility.PolynomialValid.InvalidPreWrapMode:
            //        break;
            //    case AnimationUtility.PolynomialValid.InvalidPostWrapMode:
            //        break;
            //    case AnimationUtility.PolynomialValid.TooManySegments:
            //        break;
            //}
            return false; //只能默认返回false了
        }

        public static string GetCullingSupportedString(GameObject go)
        {
            var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
            string text = "";
            foreach (ParticleSystem item in particleSystems)
            {
                string str = CheckCulling(item);
                if (!string.IsNullOrEmpty(str))
                {
                    text += item.gameObject.name + ":" + str + "\n\n";
                }
            }
            return text;
        }

        static string CheckWaring(ParticleSystem particleSystem)
        {
            string text = "";
            if (particleSystem.main.prewarm)
            {
                text += "\n勾选了 Prewarm";
            }
            return text;
        }
        static Dictionary<GameObject, string> GetWaringSupportedString(GameObject go)
        {
            Dictionary<GameObject, string> result = new Dictionary<GameObject, string>();
            var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
            string text = "";
            foreach (ParticleSystem item in particleSystems)
            {
                string str = CheckWaring(item);
                if (!string.IsNullOrEmpty(str))
                {
                    //text += item.gameObject.name + ":" + str + "\n\n";
                    result.Add(item.gameObject, str);
                }
            }
            return result;
        }
    }

}