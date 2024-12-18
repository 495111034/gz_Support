/**
* Copyright (c) 2016,广州冰峰网络科技有限公司;
* All rights reserved.
* 
* 文件名称：DrawMeshColor
* 简    述：
* 创建标识：
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;




	public class DrawMeshColor
	{

    static float maxdepth = 1;
    static string assetPath = System.IO.Path.Combine(PathDefs.ASSETS_PATH_SCENE_ASSETS, "GenMesh");
		static string extName ="asset";

		[MenuItem("Export/DrawDepth")]
		public static void DrawColor()
		{
			if (Selection.activeGameObject != null) {
				GameObject g = Selection.activeGameObject;
				MeshFilter mf =  g.GetComponent<MeshFilter> ();
				if (mf != null) {
					Mesh mesh = mf.sharedMesh;
					Vector3[] vectors = mesh.vertices;
					int count = vectors.Length;
					Color[]colorArray = new Color[count];
					for(int i= 0;i<count;i++)
					{
						Vector3 v = vectors [i];
						Vector3 pointInGroud = g.transform.TransformPoint (v);
						Vector3 targetPoint = g.transform.localToWorldMatrix.MultiplyPoint (v);
						RaycastHit hit;
						Vector3 hitPoint = Vector3.zero;
						bool isHit = Physics.Raycast (targetPoint+Vector3.up*20, Vector3.down, out hit,100000000f);
						if (isHit) {
							hitPoint = hit.point;
						}
						float depthColor = GetDepthColor (targetPoint, hitPoint);
						colorArray [i] =  new Color (depthColor, depthColor, depthColor, depthColor);
//						if (isHit) {
//							Debug.Log (depthColor);
//						}
					}
					mesh.colors =  colorArray;
					mf.sharedMesh = mesh;
					SaveMesh (mf);
				}
			}

		}

		static void SaveMesh(MeshFilter mf)
		{
        if(!System.IO.Directory.Exists(assetPath))
        {
            System.IO.Directory.CreateDirectory(assetPath);
        }
			Mesh mesh =GameObject.Instantiate(mf.sharedMesh);
			mesh.name = mf.sharedMesh.name;
        AssetDatabase.CreateAsset(mesh, System.IO.Path.Combine(assetPath, $"{mesh.name}.{mesh}"));
			mf.sharedMesh = mesh;
		}

		static float GetDepthColor(Vector3 orgPoint,Vector3 hitPoint)
		{
			if (hitPoint == Vector3.zero) {
				return 1;
			} else {
				float depth = orgPoint.y - hitPoint.y;
				if (depth < 0) {
					return 0;
				}
				depth = Mathf.Clamp(depth,0,maxdepth);
				float alpha = depth / maxdepth;
				return alpha;
			}
		}
	}
