using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//效果在Scene视图中可以查看
//镜面效果

[ExecuteInEditMode]
public class RealTimeReflect : MonoBehaviour
{
    public LayerMask reflectLayers = -1;
    static readonly int _ReflectionTexID = Shader.PropertyToID("_MirrorReflectionTex");

    public bool m_DisablePixelLights = true;
    public int m_TextureSize = 512;
    public float m_ClipPlaneOffset = 0.07f;

    private Hashtable m_ReflectionCameras = new Hashtable(); // Camera -> Camera table

    private RenderTexture m_ReflectionTexture = null;
    private int m_OldReflectionTextureSize = 0;

    private static bool s_InsideRendering = false;//important!!!

    //private float m_MaskTexScale = 0;
    public float m_MaxMaskTexScale = 1f;
    //private bool isPlayerEnter = false;
    //public float m_WaveSpeed = 0.4f;

    //private bool m_IsPlayerMoving = false;
    //public bool m_StepMode = false;
    //public bool m_StepTouch = true;

#if UNITY_EDITOR
    private void Update()
    {
        if (gameObject.layer != 0)
        {
            gameObject.layer = 0;
            Debug.LogError("挂载 RealTimeReflect 的对象Layer只能是Default 否则移动端直接卡死！！！！");
            UnityEditor.EditorUtility.DisplayDialog("提示", "挂载 RealTimeReflect 的对象Layer只能是Default\n 否则移动端直接卡死！！！！", "ok");
        }
    }
#endif

    private void Awake()
    {
        if (gameObject.layer != 0)
        {
            gameObject.layer = 0;
        }
    }

    private void Start()
    {
        if (gameObject.layer != 0)
        {
            gameObject.layer = 0;
        }

        //if (QualityUtils.IsBadGPU) 
        //{
        //    this.enabled = false;
        //}
    }
    [System.NonSerialized]
    List<Material> materials = new List<Material>();

    public void OnWillRenderObject()
    {
        var rend = GetComponent<Renderer>();
        if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled)
            return;

        Camera cam = Camera.current;
        if (!cam)
            return;
        if (m_TextureSize <= 0)
            return;

        // Safeguard from recursive reflections.        
        if (s_InsideRendering)
            return;
        s_InsideRendering = true;


        Camera reflectionCamera;

        UnityEngine.Profiling.Profiler.BeginSample("CreateMirrorObjects");
        CreateMirrorObjects(cam, out reflectionCamera);
        UnityEngine.Profiling.Profiler.EndSample();

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Optionally disable pixel lights for reflection
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (m_DisablePixelLights)
            QualitySettings.pixelLightCount = 0;

        UnityEngine.Profiling.Profiler.BeginSample("UpdateCameraModes");
        UpdateCameraModes(cam, reflectionCamera);
        UnityEngine.Profiling.Profiler.EndSample();
        // Render reflection
        // Reflect camera around reflection plane
        float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        UnityEngine.Profiling.Profiler.BeginSample("CalculateReflectionMatrix");
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        UnityEngine.Profiling.Profiler.EndSample();

        Vector3 oldpos = cam.transform.position;
        UnityEngine.Profiling.Profiler.BeginSample("MultiplyPoint");
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        UnityEngine.Profiling.Profiler.EndSample();
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        UnityEngine.Profiling.Profiler.BeginSample("CameraSpacePlane");
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("CalculateObliqueMatrix");
        //Matrix4x4 projection = cam.projectionMatrix;
        Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
        UnityEngine.Profiling.Profiler.EndSample();
        reflectionCamera.projectionMatrix = projection;
        reflectionCamera.cullingMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;

        reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value;
        reflectionCamera.targetTexture = m_ReflectionTexture;
        //GL.SetRevertBackfacing(true);
        GL.invertCulling = true;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = cam.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
        UnityEngine.Profiling.Profiler.BeginSample("reflectionCamera.Render");
        reflectionCamera.Render();
        UnityEngine.Profiling.Profiler.EndSample();
        reflectionCamera.transform.position = oldpos;
        //GL.SetRevertBackfacing(false);
        GL.invertCulling = false;
        //
        UnityEngine.Profiling.Profiler.BeginSample("GetSharedMaterials");
        rend.GetSharedMaterials(materials);
        //Material[] materials = rend.sharedMaterials;
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("SetTexture");
        foreach (Material mat in materials)
        {
            if (mat.HasProperty(_ReflectionTexID))
                mat.SetTexture(_ReflectionTexID, m_ReflectionTexture);
        }
        UnityEngine.Profiling.Profiler.EndSample();
        if (m_DisablePixelLights)
            QualitySettings.pixelLightCount = oldPixelLightCount;




        s_InsideRendering = false;
    }


    void OnDisable()
    {
        ClearData();
    }

    private void OnDestroy()
    {
        ClearData();
    }

    void ClearData()
    {
        if (m_ReflectionTexture)
        {
            RenderTexture.ReleaseTemporary(m_ReflectionTexture);
            m_ReflectionTexture = null;
        }

        if (m_ReflectionCameras != null)
        {
            foreach (DictionaryEntry kvp in m_ReflectionCameras)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(((Camera)kvp.Value).gameObject);
                }
                else
                {
                    DestroyImmediate(((Camera)kvp.Value).gameObject);
                }
            }
            m_ReflectionCameras.Clear();
        }
    }


    private void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
            return;
        // set camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            src.TryGetComponent<Skybox>(out var sky);
            dest.TryGetComponent<Skybox>(out var mysky);
            if (!sky || !sky.material)
            {
                mysky.enabled = false;
            }
            else
            {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }

        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    // On-demand create any objects we need
    private void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
    {
        reflectionCamera = null;

        if (!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
        {
            if (m_ReflectionTexture)
            {
                RenderTexture.ReleaseTemporary(m_ReflectionTexture);
                m_ReflectionTexture = null;
            }
            m_ReflectionTexture = RenderTexture.GetTemporary(m_TextureSize, m_TextureSize, 16);
            m_ReflectionTexture.name = "__MirrorReflection" + GetInstanceID();
            m_ReflectionTexture.isPowerOfTwo = true;
            m_ReflectionTexture.hideFlags = HideFlags.DontSave;
            m_OldReflectionTextureSize = m_TextureSize;
        }

        // Camera for reflection
        reflectionCamera = m_ReflectionCameras.ContainsKey(currentCamera) ? m_ReflectionCameras[currentCamera] as Camera : null;
        if (!reflectionCamera)
        {
            GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.HideAndDontSave;
            m_ReflectionCameras[currentCamera] = reflectionCamera;
        }
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    // Calculates reflection matrix around the given plane
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }
}

