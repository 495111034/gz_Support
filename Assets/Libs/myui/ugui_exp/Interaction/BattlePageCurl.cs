using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BattlePageCurl : MonoBehaviour
    {
        //reference: Deforming Pages of 3D Electronic Books

        public Canvas m_Canvas;
        public Camera m_Camera;
        public Renderer m_Render;
        RectTransform m_Rect;
        Mesh m_Mesh;

        //vertices on the mesh
        Vector3[] m_MeshVertices;

        //generated page offset to fill the UI screen
        [SerializeField] Vector3 m_PageOffset;

        //top point of the cone
        public Vector3 m_A;

        // edge bottom left
        Vector3 m_Ebl;

        Vector2[] m_uv;


        // triangles for the page normal
        int[] m_FrontTriangles;
        int[] m_BackTriangles;

        [SerializeField] int m_Xsize;
        [SerializeField] int m_Ysize;
        [SerializeField] int m_Density;

        //y value of A
        public float m_Apex;

        //cone angle
        public float m_Theta;

        //the page transform rotation angle for each update
        public float m_RhoX;
        public float m_RhoY;

        //flip time setting
        public float m_FlipTime;

        //current flip time
        float m_TimePass = 0.0f;

        //if flip finished
        bool m_FilpFinish = true;

        //page effect
        //public GameObject pageCurlEffect;

        //key frame timepoint
        List<float> m_TimePoints = new List<float>() {0.0f, 0.5f, 0.7f, 0.9f, 1.0f};

        //key frame data， theta, rho, apex
        Dictionary<float, List<float>> m_ParameterDic;

        /// <summary>
        /// build a page to curl
        /// </summary>
        /// <param name="xSize">page width</param>
        /// <param name="ySize">page height</param>
        /// <param name="vertexDensity">the amount of points in the page is vertexDensity*vertexDensity </param>
        /// <param name="basePoint">the page start position </param>
        private void BuildMesh(int xSize, int ySize, int vertexDensity, Vector3 basePoint)
        {

            m_Mesh.name = "Procedural Grid";

            m_FrontTriangles = new int[vertexDensity * vertexDensity * 6];
            m_BackTriangles = new int[vertexDensity * vertexDensity * 6];

            m_MeshVertices = new Vector3[(vertexDensity + 1) * (vertexDensity + 1)];
            m_uv = new Vector2[(vertexDensity + 1) * (vertexDensity + 1)];
            float yStep = (float) ySize / vertexDensity;
            float xStep = (float) xSize / vertexDensity;

            for (int i = 0; i <= vertexDensity; ++i)
            for (int j = 0; j <= vertexDensity; ++j)
            {
                m_MeshVertices[i * (vertexDensity + 1) + j] = new Vector3(j * xStep, i * yStep);
                m_MeshVertices[i * (vertexDensity + 1) + j] += basePoint;
                m_uv[i * (vertexDensity + 1) + j] = new Vector2((j * (float) xStep) / (float) xSize,
                    (i * (float) yStep) / (float) ySize);
            }


            for (int ti = 0, vi = 0, y = 0; y < vertexDensity; y++, vi++)
            {
                for (int x = 0; x < vertexDensity; ++x, ti += 6, ++vi)
                {
                    m_FrontTriangles[ti] = vi;
                    m_FrontTriangles[ti + 3] = m_FrontTriangles[ti + 2] = vi + 1;
                    m_FrontTriangles[ti + 4] = m_FrontTriangles[ti + 1] = vi + vertexDensity + 1;
                    m_FrontTriangles[ti + 5] = vi + vertexDensity + 2;

                    m_BackTriangles[ti] = vi;
                    m_BackTriangles[ti + 1] = m_BackTriangles[ti + 3] = vi + 1;
                    m_BackTriangles[ti + 2] = m_BackTriangles[ti + 5] = vi + vertexDensity + 1;
                    m_BackTriangles[ti + 4] = vi + vertexDensity + 2;
                }
            }

            m_Mesh.vertices = m_MeshVertices;
            m_Mesh.uv = m_uv;
            m_Mesh.subMeshCount = 2;
            m_Mesh.SetTriangles(m_FrontTriangles, 0);
            m_Mesh.SetTriangles(m_BackTriangles, 1);


            if (m_Render.materials.Length != 2)
                m_Render.materials = new Material[]
                    {new Material(resource.ShaderManager.Find("MyShaders/others/ImgFade")), new Material(resource.ShaderManager.Find("MyShaders/others/ImgFade"))};


            m_Render.enabled = false;

        }


        /// <summary>
        ///     adjust the mesh position 
        /// </summary>
        /// <param name="offset">offset to move</param>
        /// <param name="newMesh">the mesh to adjust</param>
        private void AdjustMesh(Vector3 offset, Vector3[] newMesh)
        {
            for (int i = 0; i < newMesh.Length; ++i)
            {
                newMesh[i] += offset;
            }
        }

        /// <summary>
        /// initialize the parameters for page curl
        /// </summary>
        private void InitParameter()
        {
            m_Theta = 90.0f;
            m_RhoY = 0.0f;

            m_Apex = 0.0f;
            float scaleFactor = 1.0f;
            if (m_Canvas) scaleFactor = m_Canvas.scaleFactor;

            m_PageOffset = new Vector3(m_Rect.rect.width / 2, m_Rect.rect.height / 2);


            ///calculate the position of cone's top point 
            float PageWidth = (m_Mesh.bounds.size.x * transform.localScale.x * scaleFactor) / 2;
            float PageHeigth = m_Mesh.bounds.size.y * transform.localScale.y * scaleFactor;

            Vector3 globalEBL = transform.position + new Vector3(PageWidth, PageHeigth / 2, 0);
            //m_Ebl = transform.InverseTransformPoint(globalEBL);
            //m_Ebl = globalEBL;
            m_A = new Vector3(m_Ebl.x, m_Ebl.y, m_Ebl.z);
        }

        void Awake()
        {
            // m_FlipTime = 0.26f;
            m_Rect = GetComponent<RectTransform>();
            GetComponent<MeshFilter>().mesh = m_Mesh = new Mesh();
            m_Render = GetComponent<Renderer>();

            /*//创建翻页特效
            GameObject resObj = Resources.Load("EffectAssets/UI_ChangeActor") as GameObject;
            pageCurlEffect = Instantiate(resObj);
            pageCurlEffect.SetActive(false);
            pageCurlEffect.transform.SetParent(transform.parent.parent);
            pageCurlEffect.transform.localScale = Vector3.one;
            pageCurlEffect.transform.localPosition = new Vector3(0, 0, 20f);
            pageCurlEffect.transform.localRotation = Quaternion.Euler(0, 0, 260);
            pageCurlEffect.transform.localScale = Vector3.one;
            GameObjectUtils.SetLayerRecursively(pageCurlEffect, (int) ObjLayer.UIEffect);*/
           
            
            //设置翻页Mesh的Rect的缩放及相对屏幕位置
            m_Rect.offsetMin = m_Rect.offsetMax = Vector2.zero;
            transform.localScale = Vector3.one;
        }

        void Start()
        {
            m_Xsize = (int) m_Rect.rect.width;
            m_Ysize = (int) m_Rect.rect.height;
            m_Density = (int) Mathf.Min(m_Xsize, m_Ysize) / 100;
            //StartCoroutine(SaveScreenToTexture());        

            BuildMesh(m_Xsize, m_Ysize, m_Density, new Vector3(0, 0));
            InitParameter();

            m_ParameterDic = new Dictionary<float, List<float>>();
            m_ParameterDic.Add(0.0f, new List<float>() {90.0f, 0.0f, 0f});
            m_ParameterDic.Add(0.5f, new List<float>() {60.0f, 0.0f, 0f});
            m_ParameterDic.Add(0.7f, new List<float>() {40.0f, 0.0f, 0f});
            m_ParameterDic.Add(0.9f, new List<float>() {20.0f, 0.0f, 0f});
            m_ParameterDic.Add(1.0f, new List<float>() {0.0f, 0.0f, 0f});
        }

        private void OnDisable()
        {

        }


        public bool ShowFlip; //{ get; set; }

        public bool IsComplete
        {
            get { return m_FilpFinish; }
        }

        Screenshot cameraShot;

        void Update()
        {
            // start/restart page curl
            if (ShowFlip && m_FilpFinish)
            {
                ShowFlip = false;
                if (cameraShot == null)
                    cameraShot = new Screenshot();

                // m_Render.sharedMaterial.SetTexture("_MainTex", cameraShot.Texture);
                m_Render.sharedMaterials[0].SetTexture(resource.ShaderNameHash.MainTex, cameraShot.Texture);
                m_Render.sharedMaterials[1].SetTexture(resource.ShaderNameHash.MainTex, cameraShot.Texture);

                m_FilpFinish = false;
                InitParameter();

                m_Mesh.vertices = m_MeshVertices;

                m_Mesh.RecalculateBounds();
                m_Mesh.RecalculateNormals();

                m_TimePass = 0.0f;
                m_Render.enabled = true;
                //pageCurlEffect.SetActive(true);
            }

            if (!m_FilpFinish)
            {
                float t = m_TimePass / m_FlipTime;

                ReCalculateMesh(t);

                m_TimePass += Time.deltaTime;

                if (t > 1.0)
                {
                    m_FilpFinish = true;
                    m_Render.enabled = false;
                    //pageCurlEffect.SetActive(false);
                }
            }
            else
            {
                if (cameraShot != null)
                {
                    cameraShot.Release();
                    cameraShot = null;
                }
            }
        }

        /// <summary>
        /// interpolate current theta apex and rho for page curl depend on timepass
        /// </summary>
        /// <param name="t">timepass</param>
        /// <param name="theta"></param>
        /// <param name="apex"></param>
        /// <param name="rho"></param>
        private void GetTAR(float t, out float theta, out float apex, out float rho)
        {

            if (t < 0.0f) t = 0.0f;
            if (t > 1.0f)
            {
                t = 1.0f;
                m_FilpFinish = true;
            }

            int i = 0;
            while (t > m_TimePoints[i + 1])
            {
                ++i;
            }

            //0: theta, 1: rho, 2: apex
            var baseList = m_ParameterDic[m_TimePoints[i]];
            var upperList = m_ParameterDic[m_TimePoints[i + 1]];
            float thetaBase = baseList[0];
            float rhoBase = baseList[1];
            float apexBase = baseList[2];
            float thetaDelta = upperList[0] - baseList[0];
            float rhoDelta = upperList[1] - baseList[1];
            float apexDelta = upperList[2] - baseList[2];

            float rate = (t - m_TimePoints[i]) / (m_TimePoints[i + 1] - m_TimePoints[i]);

            theta = thetaBase + rate * thetaDelta;
            apex = apexBase + rate * apexDelta;
            rho = rhoBase + rate * rhoDelta;


        }



        private float DegreeToRadians(float degree)
        {
            return degree * Mathf.PI / 180.0f;
        }

        /// <summary>
        /// get the new position for a single point on the mesh
        /// </summary>
        /// <param name="p">point</param>
        /// <param name="theta"></param>
        /// <param name="apex"></param>
        /// <returns></returns>
        public Vector3 CurlTurn(Vector3 p, float theta, float apex)
        {
            //R is the length of AP
            float R = Mathf.Sqrt(Mathf.Pow(p.x, 2) + Mathf.Pow(p.y + apex, 2.0f));

            //r is radius of parallel circle
            float r = R * Mathf.Sin(DegreeToRadians(theta));

            //beta is angle of arc sp
            float beta = Mathf.Asin(p.x / R) / Mathf.Sin(DegreeToRadians(theta));

            float x = r * Mathf.Sin(beta);
            float y = (R + apex) - ((r * (1 - Mathf.Cos(beta))) * Mathf.Sin(DegreeToRadians(theta)));
            float z = -(r * (1 - Mathf.Cos(beta))) * Mathf.Cos(DegreeToRadians(theta));
            return new Vector3(x, y, z);
        }


        /// <summary>
        /// re calculate the mesh each update
        /// </summary>
        /// <param name="t"></param>
        private void ReCalculateMesh(float t) // t range 0%-100%
        {
            if (m_Xsize != (int) m_Rect.rect.width || m_Ysize != (int) m_Rect.rect.height)
            {
                m_Xsize = (int) m_Rect.rect.width;
                m_Ysize = (int) m_Rect.rect.height;
                BuildMesh(m_Xsize, m_Ysize, m_Density, Vector3.zero);
            }

            Vector3[] newMesh = new Vector3[m_MeshVertices.Length];
            GetTAR(t, out m_Theta, out m_Apex, out m_RhoY);

            //re calculate vertices
            for (int i = 0; i < m_MeshVertices.Length; ++i)
            {
                newMesh[i] = CurlTurn(m_MeshVertices[i], m_Theta, m_A.y + m_Apex);

            }

            AdjustMesh(new Vector3(-m_Rect.rect.width / 2, -m_Rect.rect.height / 2), newMesh);
            m_Mesh.vertices = newMesh;
            m_Mesh.RecalculateNormals();

            AdjustMesh(m_PageOffset, m_Mesh.vertices);
            transform.localEulerAngles = new Vector3(0, m_RhoY, 0);
        }


    }
}
