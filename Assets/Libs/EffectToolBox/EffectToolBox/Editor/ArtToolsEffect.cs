using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace GameSupport.EffectToolBox
{
    public class ArtToolsEffect : EditorWindow
    {  

        //-------------------特效预览---------------------
        private List<Animator> m_AnimatorChilds = new List<Animator>();
        private List<float> m_TotalTimeS = new List<float>();
        private List<int> m_PsCounts = new List<int>();
        private List<int> m_MeshTrians = new List<int>();

        private bool m_AnimatorBaker = false;

        private bool m_OpenPreviewProject = false;
        private bool m_Loading = false;
        private GameObject m_CurrAsset;
        private GameObject m_CurrRootAsset;
        private GameObject m_CurrEffect;
        private GameObject m_CurrEffectHierarchy;
        private GameObject m_CurrPrefab;
        private bool m_IsInspectorUpdate = false;
        private GameObject m_SimulatePSOneGameObject = null;
        private bool m_SimulatePSOne = false;
        private GameObject mRootAsset = null;
        /// <summary>
        /// 锁定选择时必须是最高父集
        /// </summary>
        private bool m_LockRoot = true;

        private int m_Instance = 0;
        /// <summary>
        /// 最大粒子数
        /// </summary>
        private int m_PsCount = 0;
        /// <summary>
        /// 最大模型面数
        /// </summary>
        private int m_MeshTrian = 0;

        /// <summary>
        /// 时间增量
        /// </summary>
        private float delta = 0.0167f;

        /// <summary>
        /// 当前运行时间
        /// </summary>
        private float m_RunningTime;
        /// <summary>
        /// 滑动条时间
        /// </summary>
        //private float m_ProgressBar;
        /// <summary>
        /// 上一次系统时间
        /// </summary>
        private double m_PreviousTime;

        /// <summary>
        /// 粒子的最长存活
        /// </summary>
        private float m_PsTime = 0.0f;

        /// <summary>
        /// 最大时间长度	
        /// </summary>
        private float m_AniMaxTime = 0.0f;

        /// <summary>
        /// 用来采样animation
        /// </summary>
        private float m_AniTime = 0.0f;
        /// <summary>
        /// 播放模式下时间控制
        /// </summary>
        private float m_PlayingTime = 0.0f;
        private float m_RunPlayingTime = 0.0f;
        private bool m_IsPlayingTime = false;
        private float m_RepairDelay = 0.5f;

        private Vector2 scrollPos = new Vector2();
        //-----------------------------------------------------

        //-------------------预览当前特效-----------------------
        private bool m_OpenPreviewHierarchy = true;
        private bool m_EffectAniPlayLock = false;
        //private bool m_EffectAniPlayPause = false;
        private bool m_EffectAniPlayLoop = true;
        private bool m_EffectHierarchyUpdate = true;
        //----------------------------------------------------

        //----------------------------------------特效发射------------------------------------------------
        private bool m_EffectShoot = false;
        private GameObject m_Bullet;
        private float m_BulletLife;
        private float m_BulletSpeed = 20.0f;
        //------------------------------------------------------------------------------------------------

        //----------------------------------------------------------------------------------------
        ArtToolsEffectChecker m_ArtToolsEffectChecker;

        [MenuItem("MY_Support/Effect ToolBox", false, 6666)]
        static void AddEffectArtTools()
        {            
            GetWindow<ArtToolsEffect>("Effect ToolBox");
        }

        public ArtToolsEffect()
        {
            m_ArtToolsEffectChecker = new ArtToolsEffectChecker();
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;

        }

        void OnEnable()
        {
            m_PreviousTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += InspectorUpdate;
            m_IsInspectorUpdate = true;
            if(m_OpenPreviewProject)
            {
                OpenProjectPreview();
            }

            if (m_OpenPreviewHierarchy)
            {
                OpenHierarchyPreview();
            }
        }

        private void OnDisable()
        {

        }

        private void OpenProjectPreview()
        {
            m_PsCount = 0;
            m_MeshTrian = 0;
            if (m_OpenPreviewProject && !m_IsInspectorUpdate)
            {
                OnEnable();
            }
            if (m_OpenPreviewProject)
            {
                m_RunningTime = 0;
                EffectUpdateHierarchy();
                m_OpenPreviewHierarchy = false;
                m_CurrEffectHierarchy = null;
                m_EffectAniPlayLock = false;
                //m_EffectAniPlayPause = false;
                m_EffectAniPlayLoop = true;
                m_AnimatorBaker = false;
            }
            else
            {
                if (m_CurrEffect != null && m_Instance > 0)
                {
                    DestroyImmediate(m_CurrEffect);
                    m_Loading = false;
                }
            }
            m_CurrPrefab = null;
            OnSelectionChange();
        }

        private void OpenHierarchyPreview()
        {
            m_LockRoot = true;
            m_PsCount = 0;
            m_MeshTrian = 0;
            m_RunningTime = 0;
            EffectUpdateHierarchy();            
            if (!m_OpenPreviewHierarchy)
            {
                m_CurrEffectHierarchy = null;
                m_CurrPrefab = null;
                m_EffectAniPlayLock = false;
                //m_EffectAniPlayPause = false;
                m_EffectAniPlayLoop = true;
            }
            if (m_OpenPreviewHierarchy && !m_IsInspectorUpdate)
            {
                OnEnable();
            }
            if (m_OpenPreviewHierarchy)
            {
                m_AnimatorBaker = false;
                m_OpenPreviewProject = false;
                m_EffectAniPlayLoop = true;
                if (m_CurrEffect != null && m_Instance > 0)
                {
                    DestroyImmediate(m_CurrEffect);
                    m_Loading = false;
                }
            }
            OnSelectionChangeHierarchy();
        }

        private void PlayToolBoxOnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Preview", EditorStyles.boldLabel, GUILayout.Height(22f));
            Rect btnPosition = EditorGUILayout.GetControlRect(true, 14f, GUILayout.Width(12f));
            btnPosition.y += 2;
            EditorGUILayout.EndHorizontal();

            Line3D();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();


            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(m_OpenPreviewHierarchy, "Hierachy Preview", EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                m_OpenPreviewHierarchy = !m_OpenPreviewHierarchy;
                OpenHierarchyPreview();
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(m_OpenPreviewProject, "Project Preview", EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                m_OpenPreviewProject = !m_OpenPreviewProject;
                OpenProjectPreview();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            if (m_OpenPreviewHierarchy)
            {
                if (m_OpenPreviewHierarchy)
                {
                    EditorGUILayout.Toggle("Lock", m_EffectAniPlayLock);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_EffectAniPlayLock = !m_EffectAniPlayLock;
                        if (!m_EffectAniPlayLock)
                        {
                            //m_EffectAniPlayPause = false;
                            m_EffectAniPlayLoop = true;
                        }
                    }
                }

                if (m_OpenPreviewHierarchy)
                {
                    m_LockRoot = EditorGUILayout.Toggle("Select Parent：", m_LockRoot);
                }
                if (m_EffectAniPlayLock)
                {
                    EditorGUILayout.ObjectField("Lock GameObject：", m_CurrEffectHierarchy, typeof(GameObject), false);
                }
                if (m_EffectAniPlayLock)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Toggle("Loop", m_EffectAniPlayLoop);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_EffectAniPlayLoop = !m_EffectAniPlayLoop;
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (!m_EffectAniPlayLoop && !Application.isPlaying)
                {
                    if (GUILayout.Button("PlayAll"))
                    {
                        m_RunningTime = 0;
                        m_PsTime = 0;
                        m_EffectHierarchyUpdate = true;
                    }
                }

                if (!m_EffectAniPlayLoop)
                {
                    if (GUILayout.Button("Play Select"))
                    {
                        //m_EffectAniPlayPause = false;
                        PlayingOneAniPlay();
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (m_OpenPreviewHierarchy)
                {
                    if (GUILayout.Button("Reset Delay"))
                    {
                        PSDelayToZero();
                    }
                }
                if (m_OpenPreviewHierarchy)
                {
                    if (GUILayout.Button("Replay"))
                    {
                        m_RunningTime = 0;
                        if (m_CurrEffectHierarchy != null)
                        {
                            Selection.activeObject = m_CurrEffectHierarchy;
                        }
                        EffectUpdateHierarchy();
                        m_ArtToolsEffectChecker.Refresh();
                    }
                }
            }
        }
        private void CheckTotalToolBoxGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Check Total", EditorStyles.boldLabel, GUILayout.Height(22f));
            Rect btnPosition = EditorGUILayout.GetControlRect(true, 14f, GUILayout.Width(12f));
            btnPosition.y += 2;
            EditorGUILayout.EndHorizontal();
            Line3D();
            EditorGUILayout.Space();
            m_ArtToolsEffectChecker.OnTotalGUI();
        }

        private void CheckDetailToolBoxGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Check", EditorStyles.boldLabel, GUILayout.Height(22f));
            Rect btnPosition = EditorGUILayout.GetControlRect(true, 14f, GUILayout.Width(12f));
            btnPosition.y += 2;
            EditorGUILayout.EndHorizontal();
            Line3D();
            EditorGUILayout.Space();
            m_ArtToolsEffectChecker.OnGUI();
        }

        private void ProfilerToolBoxGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Profiler", EditorStyles.boldLabel, GUILayout.Height(22f));
            Rect btnPosition = EditorGUILayout.GetControlRect(true, 14f, GUILayout.Width(12f));
            btnPosition.y += 2;
            EditorGUILayout.EndHorizontal();
            Line3D();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Profiler"))
            {
                ArtToolsEffectProfiler.Profiler();
                Repaint();
            }
            if (GUILayout.Button("Stop"))
            {
                EditorApplication.isPlaying = false;
                OnSelectionChange();
            }
            EditorGUILayout.EndHorizontal();
            ArtToolsEffectProfiler.OnGUI();
        }

        private void MonitorToolBoxGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Monitor", EditorStyles.boldLabel, GUILayout.Height(22f));
            Rect btnPosition = EditorGUILayout.GetControlRect(true, 14f, GUILayout.Width(12f));
            btnPosition.y += 2;
            EditorGUILayout.EndHorizontal();
            Line3D();
            EditorGUILayout.Space();
           
            //ArtToolsEffectMonitor.OnGUI();
            EditorGUILayout.Space();
        }

        private void OptimizationToolBoxGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Optimization", EditorStyles.boldLabel, GUILayout.Height(22f));
            Rect btnPosition = EditorGUILayout.GetControlRect(true, 14f, GUILayout.Width(12f));
            btnPosition.y += 2;
            EditorGUILayout.EndHorizontal();
            Line3D();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            m_RepairDelay = EditorGUILayout.FloatField("Delay：", m_RepairDelay);
            if (GUILayout.Button(new GUIContent("Batch Add StartDelay", "批量增加延迟时间")))//批量增加延迟时间
            {
                ArtToolsEffectChecker.BatchAddStartDelay(Selection.activeGameObject, m_RepairDelay);
            }
            if (GUILayout.Button(new GUIContent("Batch Dec StartDelay", "批量减少延迟时间")))//批量减少延迟时间
            {
                ArtToolsEffectChecker.BatchAddStartDelay(Selection.activeGameObject, -1 * m_RepairDelay);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Batch ScalingMode", "批量更改粒子缩放模式")))//批量更改粒子缩放模式
            {
                ArtToolsEffectChecker.BatchOptimizationScalingMode(Selection.activeGameObject);
            }
            if (GUILayout.Button(new GUIContent("Batch TrailMat", "批量删除拖尾材质(根据情况决定)")))//批量删除拖尾材质(根据情况决定)
            {
                ArtToolsEffectChecker.BatchRemoveTrailMat(Selection.activeGameObject);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Batch Optimization", "批量优化")))//批量优化
            {
                ArtToolsEffectChecker.BatchOptimizationRes(Selection.activeGameObject);
            }                
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Batch Repair", "批量修复特效动画状态机")))//批量修复特效动画状态机
            {
                ArtToolsEffectChecker.BatchRepair(Selection.activeGameObject);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Repair EffectScript", "移除子节点中多余的EffectScript脚本")))//移除子节点中多余的EffectScript脚本
            {
                ArtToolsEffectChecker.RemoveEffectScript(Selection.activeGameObject);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void SceneToolBoxGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Scene", EditorStyles.boldLabel, GUILayout.Height(22f));
            Rect btnPosition = EditorGUILayout.GetControlRect(true, 14f, GUILayout.Width(12f));
            btnPosition.y += 2;
            EditorGUILayout.EndHorizontal();
            Line3D();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
           
            if (GUILayout.Button("Role Scene"))
            {
                EditorSceneManager.OpenScene("Assets/plugins_camera/SupportRes/EffectToolBox/Editor/Res/Scenes/role_scence.unity");
            }
            if (GUILayout.Button("UI Scene"))
            {
                EditorSceneManager.OpenScene("Assets/plugins_camera/SupportRes/EffectToolBox/Editor/Res/Scenes/ui_scence.unity");
            }
            if (GUILayout.Button("Role AB Scene"))
            {
                EditorSceneManager.OpenScene("Assets/plugins_camera/SupportRes/EffectToolBox/Editor/Res/Scenes/role_ab_scence.unity");
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);
            PlayToolBoxOnGUI();            
            CheckTotalToolBoxGUI();
            MonitorToolBoxGUI();
            ProfilerToolBoxGUI();            
            //CheckDetailToolBoxGUI();
            OptimizationToolBoxGUI();
            SceneToolBoxGUI();
            GUILayout.EndScrollView();

        }
        //------------------------特效预览------------------------------------------
        void OnDestroy()
        {
            m_OpenPreviewProject = false;
            m_OpenPreviewHierarchy = false;
            if (m_CurrEffect != null)
            {
                DestroyImmediate(m_CurrEffect);
                m_AnimatorBaker = false;
                m_Loading = false;
                m_EffectShoot = false;
            }
            EditorApplication.update -= InspectorUpdate;
            m_IsInspectorUpdate = false;
        }

        void SimulateUpdate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (m_OpenPreviewProject && m_Loading && m_CurrAsset != null)
            {
                if (m_AnimatorBaker)
                {
                    for (int i = 0; i < m_AnimatorChilds.Count; i++)
                    {
                        if (m_AnimatorChilds[i] != null && m_AnimatorChilds[i].runtimeAnimatorController != null)
                        {
                            if (m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).loop)
                            {
                                if (m_TotalTimeS[i] < m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length)
                                {
                                    m_AnimatorChilds[i].playbackTime = (m_RunningTime % m_TotalTimeS[i]);
                                }
                                else
                                {
                                    m_AnimatorChilds[i].playbackTime = (m_RunningTime % m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length);
                                }
                            }
                            else
                            {
                                if (m_RunningTime <= m_TotalTimeS[i] && m_RunningTime <= m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length)
                                {
                                    m_AnimatorChilds[i].playbackTime = m_RunningTime;
                                }
                                if (m_RunningTime >= m_AniMaxTime && m_RunningTime >= m_PsTime)
                                {
                                    m_RunningTime = 0.0f;
                                }
                            }
                            m_AnimatorChilds[i].Update(0);
                        }
                    }
                }
                if (!m_AnimatorBaker)
                {
                    AnimatorBaker();
                }
                PSAniPlay();
                AnimationPlay();
            }

            if (!m_OpenPreviewHierarchy && !m_OpenPreviewProject && m_CurrEffect != null)
            {
                DestroyImmediate(m_CurrEffect);
                m_AnimatorBaker = false;
                m_Loading = false;
                EditorApplication.update -= InspectorUpdate;
                m_IsInspectorUpdate = false;
            }

            EffectUpdateHierarchy();
            SimulatePSAniPlay();
        }


        void OptimizationCheckRes(GameObject go)
        {
        }

        void Update()
        {
            if (!Application.isPlaying && !m_OpenPreviewProject && !m_OpenPreviewHierarchy)
            {
                if (m_IsInspectorUpdate)
                {
                    EditorApplication.update -= InspectorUpdate;
                    m_IsInspectorUpdate = false;
                }
                return;
            }
            GetCurrPrefab();

            if (m_CurrEffect != null)
            {
                m_ArtToolsEffectChecker.OnSelectionChangeFunc(m_CurrEffect, m_CurrAsset);
                Repaint();
            }
            if (m_CurrEffectHierarchy != null)
            {
                //var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(m_CurrEffectHierarchy);
                m_ArtToolsEffectChecker.OnSelectionChangeFunc(m_CurrEffectHierarchy, m_CurrAsset);
                Repaint();
            }

            if (!Application.isPlaying)
            {
                GetPlayingTime();

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                { //运行播放前判断
                    if (m_CurrEffect != null)
                    {
                        DestroyImmediate(m_CurrEffect);
                        //m_OpenPreviewProject = false;
                    }
                    if (m_CurrEffectHierarchy != null)
                    {
                        m_CurrEffectHierarchy = null;
                        //m_OpenPreviewHierarchy = false;
                        //m_EffectAniPlayLock = false;
                        //m_EffectAniPlayPause = false;
                    }
                }
                return;
            }


            if (m_CurrPrefab != null && !m_IsPlayingTime && m_PlayingTime > 0)
            {
                m_RunPlayingTime = m_PlayingTime;
                if (!m_IsPlayingTime && m_CurrPrefab != null && m_RunPlayingTime > 0.0f)
                {
                    PlayingAllAniPlay();
                }
            }
            if (m_IsPlayingTime && m_EffectAniPlayLoop)
            {
                m_RunPlayingTime = m_RunPlayingTime - Time.deltaTime;
                if (m_RunPlayingTime < 0)
                {
                    m_IsPlayingTime = false;
                    m_PlayingTime = 0;
                }
            }
            GetPlayingTime();
            EffectShootUpdate();
        }

        private GameObject GetRoot()
        {
            var uiRoot = GameObject.Find("UIRoot");
            if(uiRoot)
            {
                var go = GameObject.Find("Root");
                if (!go)
                    go = GameObject.Find("DlgRoot");

                return go;
            }
            else
            {
                return null;
            }


        }

        private GameObject GetAssetRoot(GameObject go)
        {
            if(mRootAsset != null)
            {
                if (go.transform.parent == null || go.transform.parent == mRootAsset.transform)
                {
                    return go;
                }
                else
                {
                    return GetAssetRoot(go.transform.parent.gameObject);
                }             
            }
            else
            {
               return go.transform.root.gameObject;
            }            
        }


        void OnSelectionChange()
        {
            if (Application.isPlaying)
            {
                return;
            }

            mRootAsset = GetRoot();
            if (m_OpenPreviewProject && m_Instance > 0)
            {
                DestroyImmediate(m_CurrEffect);
                m_Loading = false;
            }

            if (m_OpenPreviewProject)
            {
                m_CurrEffectHierarchy = null;
                m_CurrAsset = Selection.activeGameObject;
                m_AnimatorBaker = false;
                CheckResource();
            }

            if (m_OpenPreviewHierarchy)
            {
                OnSelectionChangeHierarchy();
            }
        }

        void CheckResource()
        {
            if (m_OpenPreviewProject && m_CurrAsset != null && AssetDatabase.Contains(m_CurrAsset) && GetAssetRoot(m_CurrAsset).transform == m_CurrAsset.transform)
            {
                GameObject go = null;
                if (mRootAsset != null)
                {
                    go = Instantiate(m_CurrAsset, new Vector3(0, 0, 0), m_CurrAsset.transform.localRotation, mRootAsset.transform);
                }
                else
                {
                    go = Instantiate(m_CurrAsset, new Vector3(0, 0, 0), m_CurrAsset.transform.localRotation);
                }
                m_CurrEffect = go as GameObject;

                m_Instance += 1;

                m_Loading = true;
                m_PsCount = 0;
                m_MeshTrian = 0;

                var animatorTranS = m_CurrEffect.GetComponentsInChildren<Animator>();
                m_AnimatorChilds.Clear();
                m_TotalTimeS.Clear();
                foreach (var tran in animatorTranS)
                {
                    m_AnimatorChilds.Add(tran);
                    m_TotalTimeS.Add(0.0f);
                }
                AnimatorBaker();

                m_RunningTime = 0.0f;
                m_PlayingTime = 0.0f;
            }
        }

        void PSAniPlay()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (m_OpenPreviewProject && m_Loading && m_CurrAsset != null)
            {
                if (m_CurrEffect != null)
                {
                    var psS = m_CurrEffect.GetComponentsInChildren<ParticleSystem>();
                    m_PsTime = 0.0f;
                    if (psS.Length > 0)
                    {
                        foreach (var ps in psS)
                        {
                            if ((ps.main.duration + ps.startLifetime + ps.startDelay) > m_PsTime)
                            {
                                m_PsTime = ps.main.duration + ps.startLifetime + ps.startDelay;
                                if (m_AniMaxTime < m_PsTime)
                                {
                                    m_AniMaxTime = m_PsTime;
                                }
                            }
                        }
                        foreach (var ps in psS)
                        {
                            if (ps.main.loop == false)
                            {
                                if (m_RunningTime >= m_PsTime)
                                {
                                    ps.Stop(false);
                                    m_RunningTime = 0;
                                }
                            }

                            bool useAutoRandomSeed = ps.useAutoRandomSeed;
                            if (useAutoRandomSeed)
                            {
                                ps.Pause(false);
                                ps.useAutoRandomSeed = false;
                                ps.Play(false);
                            }
                            if (m_RunningTime <= delta)
                            {
                                ps.Simulate(delta, false, true); ;
                            }
                            else
                            {
                                ps.Simulate(delta, false, false);
                            }
                            if (useAutoRandomSeed)
                            {
                                ps.useAutoRandomSeed = useAutoRandomSeed;
                            }
                        }
                    }
                }
            }
        }

        void AnimatorBaker()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (m_OpenPreviewProject && m_Loading && m_CurrAsset != null)
            {
                m_AniMaxTime = 0.0f;

                if (m_AnimatorChilds.Count > 0)
                {

                    if (!m_AnimatorBaker && m_CurrEffect != null)
                    {

                        foreach (var ps in m_AnimatorChilds)
                        {
                            if (ps.runtimeAnimatorController != null)
                            {
                                
                                if (ps.GetCurrentAnimatorStateInfo(0).length > m_AniMaxTime)
                                {
                                    m_AniMaxTime = ps.GetCurrentAnimatorStateInfo(0).length;
                                }
                            }
                        }

                        for (int i = 0; i < m_AnimatorChilds.Count; i++)
                        {
                            if (m_AnimatorChilds[i].runtimeAnimatorController != null)
                            {
                                float frameRate = 30f;
                                int frameCount = (int)((m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length * frameRate) + 2);
                                m_AnimatorChilds[i].Rebind();
                                m_AnimatorChilds[i].StopPlayback();
                                m_AnimatorChilds[i].recorderStartTime = 0;

                                m_AnimatorChilds[i].StartRecording(frameCount);

                                for (var j = 0; j < frameCount - 1; j++)
                                {
                                    m_AnimatorChilds[i].Update(1.0f / frameRate);
                                }

                                m_AnimatorChilds[i].StopRecording();
                                m_AnimatorChilds[i].StartPlayback();
                                m_TotalTimeS[i] = m_AnimatorChilds[i].recorderStopTime;
                                if (m_AnimatorChilds[i].recorderStopTime < 0)
                                {
                                    return;
                                }
                            }
                        }
                        m_AnimatorBaker = true;
                    }
                }

            }
        }

        void AnimationPlay()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (m_OpenPreviewProject && m_Loading && m_CurrAsset != null)
            {
                if (m_CurrEffect != null)
                {
                    var psS = m_CurrEffect.GetComponentsInChildren<Animation>();

                    if (psS.Length > 0)
                    {
                        foreach (var ps in psS)
                        {
                            if (ps.clip != null)
                            {

                                if (ps.clip.length > m_AniMaxTime)
                                {
                                    m_AniMaxTime = ps.clip.length;
                                }
                            }
                        }

                        foreach (var ps in psS)
                        {
                            if (ps.clip != null)
                            {

                                m_AniTime = m_RunningTime;

                                ps.clip.SampleAnimation(ps.gameObject, m_AniTime);
                                if (m_AniTime >= m_AniMaxTime && m_RunningTime >= m_PsTime)
                                {
                                    m_RunningTime = 0.0f;
                                }
                                if (m_AniTime >= ps.clip.length)
                                {
                                    m_AniTime = 0.0f;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InspectorUpdate()
        {
            //delta = EditorApplication.timeSinceStartup - m_PreviousTime;
            //m_PreviousTime = EditorApplication.timeSinceStartup;

            if (!Application.isPlaying)
            {
                m_RunningTime = m_RunningTime + delta;
                //			if (!m_EffectAniPlayPause) {
                //				m_RunningTime = m_RunningTime + delta;
                //				m_ProgressBar = m_RunningTime;
                //			}
                SimulateUpdate();

            }
        }
        //---------------------------------------------------------------------------------------------

        //--------------------------------Hierarchy特效预览------------------------------------------------
        void OnSelectionChangeHierarchy()
        {
            if (m_EffectAniPlayLoop)
            {
                m_RunningTime = 0;
                EffectUpdateHierarchy();
            }
            //		if (!m_EffectAniPlayPause && effectAniPlayLoop) {
            //			m_RunningTime = 0;
            //			EffectUpdate ();
            //		}

            m_CurrAsset = Selection.activeGameObject;

            if (m_OpenPreviewHierarchy && m_CurrAsset != null && !AssetDatabase.Contains(m_CurrAsset) && !m_EffectAniPlayLock)
            {
                if (m_LockRoot)
                {
                    if (GetAssetRoot(m_CurrAsset).transform== m_CurrAsset.transform)
                    {
                        m_CurrEffectHierarchy = m_CurrAsset;
                        m_AnimatorBaker = false;
                        EffectHierarchy();
                    }
                    return;
                }
                else
                {
                    m_CurrEffectHierarchy = m_CurrAsset;
                    m_AnimatorBaker = false;
                    EffectHierarchy();
                }
            }
        }

        void EffectHierarchy()
        {
            if (m_OpenPreviewHierarchy && m_CurrAsset != null && m_CurrEffectHierarchy != null && !AssetDatabase.Contains(m_CurrAsset))
            {
                var animatorTranS = m_CurrEffectHierarchy.GetComponentsInChildren<Animator>();
                m_AnimatorChilds.Clear();
                m_TotalTimeS.Clear();
                foreach (var tran in animatorTranS)
                {
                    m_AnimatorChilds.Add(tran);
                    m_TotalTimeS.Add(0.0f);
                }
                AnimaterBakerHierarchy();

                m_RunningTime = 0.0f;
                m_PlayingTime = 0.0f;
                m_PsCount = 0;
                m_MeshTrian = 0;
            }
        }

        void PSPlayAniHierarchy()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (m_OpenPreviewHierarchy && m_CurrEffectHierarchy != null)
            {
                if (m_CurrAsset == null && !m_EffectAniPlayLock)
                {
                    m_RunningTime = 0;
                }

                var psS = m_CurrEffectHierarchy.GetComponentsInChildren<ParticleSystem>();
                m_PsTime = 0.0f;
                if (psS.Length > 0)
                {
                    foreach (var ps in psS)
                    {
                        if ((ps.main.duration + ps.startLifetime + ps.startDelay) > m_PsTime)
                        {
                            m_PsTime = ps.main.duration + ps.startLifetime + ps.startDelay;
                            if (m_AniMaxTime < m_PsTime)
                            {
                                m_AniMaxTime = m_PsTime;
                            }
                        }
                    }
                    foreach (var ps in psS)
                    {
                        if (ps.main.loop == false)
                        {
                            if (m_EffectAniPlayLoop && m_RunningTime >= m_PsTime)
                            {
                                ps.Stop();
                                m_RunningTime = 0;
                            }
                        }
                        bool useAutoRandomSeed = ps.useAutoRandomSeed;
                        //ps.Stop(false , ParticleSystemStopBehavior.StopEmittingAndClear);
                        //if (useAutoRandomSeed)
                        //{
                        //    ps.useAutoRandomSeed = false;
                        //}
                        if (m_RunningTime <= delta)
                        {
                            ps.Simulate(delta, false, true); ;
                        }
                        else
                        {
                            ps.Simulate(delta, false, false);
                        }
                        //if (useAutoRandomSeed)
                        //{
                        //    ps.useAutoRandomSeed = useAutoRandomSeed;
                        //}
                        //ps.Play(false);
                    }
                }
            }
        }

        void AnimaterBakerHierarchy()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (m_OpenPreviewHierarchy && m_CurrAsset != null && m_CurrEffectHierarchy != null)
            {
                m_AniMaxTime = 0.0f;

                if (m_AnimatorChilds.Count > 0)
                {

                    if (!m_AnimatorBaker)
                    {

                        foreach (var ps in m_AnimatorChilds)
                        {
                            if (ps.runtimeAnimatorController != null)
                            {

                                if (ps.GetCurrentAnimatorStateInfo(0).length > m_AniMaxTime)
                                {
                                    m_AniMaxTime = ps.GetCurrentAnimatorStateInfo(0).length;
                                }
                            }
                        }

                        for (int i = 0; i < m_AnimatorChilds.Count; i++)
                        {
                            if (m_AnimatorChilds[i].runtimeAnimatorController != null)
                            {
                                float frameRate = 30f;
                                int frameCount = (int)((m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length * frameRate) + 2);
                                m_AnimatorChilds[i].Rebind();
                                m_AnimatorChilds[i].StopPlayback();
                                m_AnimatorChilds[i].recorderStartTime = 0;

                                m_AnimatorChilds[i].StartRecording(frameCount);

                                for (var j = 0; j < frameCount - 1; j++)
                                {
                                    m_AnimatorChilds[i].Update(1.0f / frameRate);
                                }

                                m_AnimatorChilds[i].StopRecording();
                                m_AnimatorChilds[i].StartPlayback();
                                m_TotalTimeS[i] = m_AnimatorChilds[i].recorderStopTime;
                                if (m_AnimatorChilds[i].recorderStopTime < 0)
                                {
                                    return;
                                }
                            }
                        }
                        m_AnimatorBaker = true;
                    }
                }

            }
        }

        void AnimationPlayHierarchy()
        {
            if (Application.isPlaying)
            {
                return;
            }


            if (m_OpenPreviewHierarchy && m_CurrEffectHierarchy != null)
            {
                if (m_CurrAsset == null && !m_EffectAniPlayLock)
                {
                    m_RunningTime = 0;
                }

                var psS = m_CurrEffectHierarchy.GetComponentsInChildren<Animation>();

                if (psS.Length > 0)
                {
                    foreach (var ps in psS)
                    {
                        if (ps.clip.length > m_AniMaxTime)
                        {
                            m_AniMaxTime = ps.clip.length;
                        }
                    }

                    foreach (var ps in psS)
                    {
                        m_AniTime = m_RunningTime;

                        ps.clip.SampleAnimation(ps.gameObject, m_AniTime);
                        if (m_EffectAniPlayLoop && m_AniTime >= m_AniMaxTime && m_RunningTime >= m_PsTime)
                        {
                            m_RunningTime = 0.0f;
                        }
                        if (m_AniTime >= ps.clip.length)
                        {
                            m_AniTime = 0.0f;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hierarchy下特效预览
        /// </summary>
        void EffectUpdateHierarchy()
        {
            if (Application.isPlaying)
            {
                return;
            }

            //单独播放粒子时禁止其它播放
            if (m_SimulatePSOne)
            {
                return;
            }

            if (!m_EffectAniPlayLoop)
            {
                if (m_RunningTime >= m_AniMaxTime)
                {
                    m_EffectHierarchyUpdate = false;
                }
            }
            else
            {
                m_EffectHierarchyUpdate = true;
            }

            if (m_OpenPreviewHierarchy && m_CurrEffectHierarchy != null && m_EffectHierarchyUpdate)
            {
                if (m_CurrAsset == null && !m_EffectAniPlayLock)
                {
                    m_RunningTime = 0;
                }
                if (m_AnimatorBaker)
                {
                    for (int i = 0; i < m_AnimatorChilds.Count; i++)
                    {
                        if (m_AnimatorChilds[i] != null && m_AnimatorChilds[i].runtimeAnimatorController != null)
                        {
                            if (m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).loop)
                            {
                                if (m_TotalTimeS[i] < m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length)
                                {
                                    m_AnimatorChilds[i].playbackTime = (m_RunningTime % m_TotalTimeS[i]);
                                }
                                else
                                {
                                    m_AnimatorChilds[i].playbackTime = (m_RunningTime % m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length);
                                }
                            }
                            else
                            {
                                if (m_RunningTime <= m_TotalTimeS[i] && m_RunningTime <= m_AnimatorChilds[i].GetCurrentAnimatorStateInfo(0).length)
                                {
                                    m_AnimatorChilds[i].playbackTime = m_RunningTime;
                                }
                                if (m_EffectAniPlayLoop && m_RunningTime >= m_AniMaxTime && m_RunningTime >= m_PsTime)
                                {
                                    m_RunningTime = 0.0f;
                                }
                            }
                            m_AnimatorChilds[i].Update(0);
                        }
                    }
                }
                if (!m_AnimatorBaker)
                {
                    AnimaterBakerHierarchy();
                }

                PSPlayAniHierarchy();
                AnimationPlayHierarchy();
            }

            if (!m_OpenPreviewHierarchy && m_CurrEffectHierarchy != null)
            {
                m_AnimatorBaker = false;
                EditorApplication.update -= InspectorUpdate;
                m_IsInspectorUpdate = false;
            }
        }
        //------------------------------------------------------------------------------------------------

        //----------------------------------------特效发射------------------------------------------------
        void EffectShootUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_EffectShoot)
            {
                if (m_BulletLife < 0)
                {
                    m_BulletLife = 3.0f;
                }

                if (m_OpenPreviewProject)
                {
                    m_Bullet = m_CurrEffect;
                    ShootEffect(m_Bullet);
                }
                if (m_OpenPreviewHierarchy)
                {
                    m_Bullet = m_CurrEffectHierarchy;
                    ShootEffect(m_Bullet);
                }
            }

            if (!m_EffectShoot && m_Bullet != null)
            {
                m_BulletLife = 0;
                m_Bullet.transform.position = Vector3.zero;
                var trails = m_Bullet.GetComponentsInChildren<TrailRenderer>();
                foreach (var trail in trails)
                {
                    trail.Clear();
                }
                m_Bullet = null;
            }
        }

        void ShootEffect(GameObject go)
        {
            if (go != null)
            {
                go.transform.Translate(Vector3.forward * Time.deltaTime * m_BulletSpeed);
                m_BulletLife -= Time.deltaTime;
                if (m_BulletLife <= 0)
                {
                    var trails = go.GetComponentsInChildren<TrailRenderer>();
                    foreach (var trail in trails)
                    {
                        trail.Clear();
                    }
                    go.transform.position = Vector3.zero;
                }
            }
        }
        //------------------------------------------------------------------------------------------------

        //------------------------------------播放模式下循环播放------------------------------------------
        void GetCurrPrefab()
        {
            if (m_OpenPreviewProject && m_CurrEffect != null && m_CurrAsset != null)
            {
                m_CurrPrefab = m_CurrEffect;
            }
            if (m_OpenPreviewHierarchy && m_CurrEffectHierarchy != null)
            {
                m_CurrPrefab = m_CurrEffectHierarchy;
            }
        }

        void GetPlayingTime()
        {
            if (m_CurrPrefab != null && m_PlayingTime <= 0)
            {
                var psS = m_CurrPrefab.GetComponentsInChildren<ParticleSystem>();
                if (psS.Length > 0)
                {
                    foreach (var ps in psS)
                    {
                        if ((ps.main.duration + ps.startLifetime + ps.startDelay) > m_PlayingTime)
                        {
                            m_PlayingTime = ps.duration + ps.startLifetime + ps.startDelay;
                        }
                    }
                }

                var atrS = m_CurrPrefab.GetComponentsInChildren<Animator>();
                if (atrS.Length > 0)
                {
                    foreach (var atr in atrS)
                    {
                        if (atr.runtimeAnimatorController != null)
                        {
                            if (m_PlayingTime < atr.GetCurrentAnimatorStateInfo(0).length)
                            {
                                m_PlayingTime = atr.GetCurrentAnimatorStateInfo(0).length;
                            }
                        }
                    }
                }

                var aniS = m_CurrPrefab.GetComponentsInChildren<Animation>();
                if (aniS.Length > 0)
                {
                    foreach (var ani in aniS)
                    {
                        if (ani.clip != null)
                        {
                            if (ani.clip.length > m_PlayingTime)
                            {
                                m_PlayingTime = ani.clip.length;
                            }
                        }
                    }
                }
                m_IsPlayingTime = false;
                m_RunPlayingTime = m_PlayingTime;
            }
        }

        void PlayingAllAniPlay()
        {
            if (m_CurrPrefab != null)
            {
                var psS = m_CurrPrefab.GetComponentsInChildren<ParticleSystem>();
                if (psS.Length > 0)
                {
                    foreach (var ps in psS)
                    {
                        if (!ps.isPlaying)
                        {
                            ps.Play(true);
                        }
                    }
                }

                var atrS = m_CurrPrefab.GetComponentsInChildren<Animator>();
                if (atrS.Length > 0)
                {
                    foreach (var atr in atrS)
                    {
                        var hash = atr.GetCurrentAnimatorStateInfo(0).fullPathHash;
                        if (atr.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                        {
                            atr.Play(hash, 0, 0);
                        }
                    }
                }

                var aniS = m_CurrPrefab.GetComponentsInChildren<Animation>();
                if (aniS.Length > 0)
                {
                    foreach (var ani in aniS)
                    {
                        if (!ani.isPlaying)
                        {
                            ani.Play();
                        }
                    }
                }
                m_IsPlayingTime = true;
            }
        }

        /// <summary>
        /// 播放单独的动画
        /// </summary>
        void PlayingOneAniPlay()
        {
            if (Selection.activeGameObject != null)
            {
                var psobj = Selection.activeGameObject;
                var ps = psobj.GetComponent<ParticleSystem>();

                if (Application.isPlaying && ps != null && !ps.isPlaying)
                {
                    ps.Play(false);
                }
                if (!Application.isPlaying && ps != null)
                {
                    m_RunningTime = 0; 
                    m_PsTime = 0.0f;
                    if ((ps.main.duration + ps.startLifetime + ps.startDelay) > m_PsTime)
                    {
                        m_PsTime = ps.main.duration + ps.startLifetime + ps.startDelay;
                    }
                    m_SimulatePSOne = true;
                    m_SimulatePSOneGameObject = Selection.activeGameObject;
                }
            }
        }

        void SimulatePSAniPlay()
        {
            if (m_SimulatePSOne && m_SimulatePSOneGameObject != null)
            {
                var psobj = m_SimulatePSOneGameObject;
                var ps = psobj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    if (m_RunningTime <= delta)
                    {
                        ps.Simulate(delta, true, true);
                    }
                    else
                    {
                        ps.Simulate(delta, true, false);
                    }
                }
                if (m_RunningTime >= m_PsTime)
                {
                    m_SimulatePSOne = false;
                    m_SimulatePSOneGameObject = null;
                }
            }
        }

        void PSDelayToZero()
        {
            if (m_CurrPrefab != null)
            {
                var psS = m_CurrPrefab.GetComponentsInChildren<ParticleSystem>();
                if (psS.Length > 0)
                {
                    foreach (var ps in psS)
                    {
                        ps.startDelay = 0;
                    }
                }
            }
        }

        private void PlaymodeStateChanged()
        {
            if (!EditorApplication.isPlaying)
            {
                OnSelectionChange();
                OnSelectionChangeHierarchy();
            }

        }


        //--------------------------------------------------------------------------------------------------
        // UI GENERAL

        #region UI UTILIES

        static readonly GUIContent sTmpContent = new GUIContent();
        public static GUIContent TempGUIContent(string _label, string _tooltip = null)
        {
            sTmpContent.text = _label;
            sTmpContent.tooltip = _tooltip;
            return sTmpContent;
        }

        private bool GUITab(string label, int index)
        {
            return false;
        }

        private void Line3D()
        {
            GUILine(Color.gray, 1);
            GUILine(new Color(.8f, .8f, .8f, 1f), 1);
        }

        static public GUIStyle _LineStyle;
        static public GUIStyle LineStyle
        {
            get
            {
                if (_LineStyle == null)
                {
                    _LineStyle = new GUIStyle();
                    _LineStyle.normal.background = EditorGUIUtility.whiteTexture;
                    _LineStyle.stretchWidth = true;
                }

                return _LineStyle;
            }
        }

        static public void GUILine(float height)
        {
            GUILine(Color.black, height);
        }
        static public void GUILine(Color color, float height)
        {
            GUILine(color, height, float.MaxValue);
        }
        static public void GUILine(Color color, float height, float width)
        {
            Rect position = GUILayoutUtility.GetRect(0f, width, height, height, LineStyle);

            if (Event.current.type == EventType.Repaint)
            {
                Color orgColor = GUI.color;
                GUI.color = orgColor * color;
                LineStyle.Draw(position, false, false, false, false);
                GUI.color = orgColor;
            }
        }
        #endregion

    }
}