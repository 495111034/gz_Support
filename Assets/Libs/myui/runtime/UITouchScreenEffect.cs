using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// 屏幕点击特效
    /// </summary>
    public class UITouchScreenEffect : MonoBehaviour
    {


        public void SetParmas(Camera _camera, GameObject go, int layer, int renderer_queue, int cache_count = 1)
        {
            fx_parent = new GameObject("fx_parent").transform;
            fx_parent.SetParent(_camera.transform);
            fx_parent.localPosition = Vector3.forward * 2;
            fx_parent.localScale = Vector3.one * 0.0012f;

            fxRenderCamera = _camera;
            fxSample = go;
            go.transform.SetParent(fx_parent);
            go.transform.localScale = Vector3.zero;
            go.transform.localPosition = Vector3.zero;
            pool.Enqueue(go);

            max_cache_count = cache_count;

            var rendes = go.GetComponentsInChildren<Renderer>();
            if (rendes != null)
            {
                for (int i = 0; i < rendes.Length; i++)
                {
                    rendes[i].sortingOrder = layer + rendes[i].sortingOrder + 1;
                    rendes[i].sharedMaterial.renderQueue = renderer_queue;
                }
            }
        }

        private int max_cache_count = 0;

        /// <summary>
        /// 屏幕特效原始资源
        /// </summary>
        public GameObject fxSample;

        public Transform fx_parent;
        /// <summary>
        /// 屏幕特效的生命时长，超过后会进行缓存
        /// </summary>
        public float fxLifeTime = 1.0f;

        /// <summary>
        /// 屏幕特效渲染使用的相机
        /// </summary>
        public Camera fxRenderCamera;

        private Queue<GameObject> pool = new Queue<GameObject>(4);
        private List<GameObject> activatedFXList = new List<GameObject>();
        private List<float> activatedTimeList = new List<float>();

        private void Update()
        {
            for (int i = activatedFXList.Count - 1; i >= 0; --i)
            {
                activatedTimeList[i] -= Time.deltaTime;
                if (activatedTimeList[i] < 0)
                {
                    RecycleFX(activatedFXList[i]);
                    activatedFXList.RemoveAt(i);
                    activatedTimeList.RemoveAt(i);
                }
            }

            if (Application.isMobilePlatform)
            {
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        PlayFX(touch.position);
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    PlayFX(Input.mousePosition);
                }
            }
        }

        private void PlayFX(Vector2 tapPos)
        {
            GameObject fx = CreateFX();
            activatedFXList.Add(fx);
            activatedTimeList.Add(fxLifeTime);

            Vector3 pos = fxRenderCamera.ScreenToWorldPoint(tapPos);
            pos.z = fx_parent.position.z;
            fx.transform.position = pos;
            fx.SetActive(true);
        }

        private GameObject CreateFX()
        {
            GameObject newFX = null;
            if (activatedFXList.Count > max_cache_count)
            {
                newFX = activatedFXList[0];
                activatedFXList.RemoveAt(0);
                activatedTimeList.RemoveAt(0);
            }
            else if (pool.Count > 0)
            {
                newFX = pool.Dequeue();
            }
            else
            {
                newFX = Instantiate(fxSample, fx_parent);
            }
            newFX.SetActiveX(false);
            newFX.transform.localScale = Vector3.one;
            return newFX;
        }

        private void RecycleFX(GameObject fx)
        {
            fx.transform.localScale = Vector3.zero;
            pool.Enqueue(fx);
        }
    }
}
