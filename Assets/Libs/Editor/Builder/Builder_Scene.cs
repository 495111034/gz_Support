

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class Builder_All 
{
    static HashSet<string> _scene_build_paths = new HashSet<string>();

    static void _scene_WriteTransform(XmlWriter xr, Transform go)
    {
        if (go.position != Vector3.zero)
        {
            xr.WriteAttributeString("pos", DataParse.ToString(go.position));
        }
        if (go.rotation != Quaternion.identity)
        {
            xr.WriteAttributeString("rot", DataParse.ToString(go.rotation));
        }
        if (go.lossyScale != Vector3.one)
        {
            xr.WriteAttributeString("scale", DataParse.ToString(go.lossyScale));
        }
    }

    static void _scene_write_go(XmlWriter xr, Transform go, GameObject prefab)
    {
        xr.WriteStartElement("go");
        {
            var prefab_pathname = AssetDatabase.GetAssetPath(prefab).ToLower();
            var prefab_name = Path.GetFileNameWithoutExtension(prefab_pathname);
            xr.WriteAttributeString("prefab", prefab_name);
            //
            if (go.name != prefab.name)
            {
                xr.WriteAttributeString("name", go.name);
            }
            if (!go.gameObject.activeInHierarchy)
            {
                xr.WriteAttributeString("active", "0");
            }
            //
            xr.WriteAttributeString("layer", go.gameObject.layer.ToString());
            //
            _scene_WriteTransform(xr, go);
            //
            var rds = go.GetComponentsInChildren<Renderer>();
            if (rds != null && rds.Length > 0)
            {
                var flag = false;
                foreach (var rd in rds)
                {
                    if (rd.lightmapIndex >= 0)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    var lm_indexs = new string[rds.Length];
                    var lm_offsets = new string[rds.Length];
                    for (var i = 0; i < rds.Length; ++i)
                    {
                        var rd = rds[i];
                        lm_indexs[i] = rd.lightmapIndex.ToString();
                        lm_offsets[i] = DataParse.ToString(rd.lightmapScaleOffset * 1000);
                    }
                    xr.WriteAttributeString("lm_index", string.Join("|", lm_indexs));
                    xr.WriteAttributeString("lm_offset", string.Join("|", lm_offsets));
                }
            }
            var LODGroup = go.GetComponentInChildren<LODGroup>();
            if (LODGroup)
            {
                var lods = LODGroup.GetLODs();
                xr.WriteAttributeString("lods", lods[0].screenRelativeTransitionHeight + "," + lods[0].fadeTransitionWidth + "|" + lods[1].screenRelativeTransitionHeight + "," + lods[1].fadeTransitionWidth);
            }
        }
        xr.WriteEndElement();
    }

    public static bool _scene_WriteTextIfChanged(string pathname, string text)
    {
        if (File.Exists(pathname) && File.ReadAllText(pathname) == text)
        {
            return false;
        }
        File.WriteAllText(pathname, text);
        return true;
    }

    public static HashSet<GameObject> GetExportGameObjects(Dictionary<Transform, GameObject> real_gos = null)
    {
        var prefabs = new HashSet<GameObject>();
        var gos = GameObject.FindObjectsOfType<GameObject>();
        foreach (var go in gos)
        {
            if(go.transform.localScale.x < 0 || go.transform.localScale.y < 0 || go.transform.localScale.z < 0)
            {
                Log.LogError($"打包日志]prefab scale is 负的={go.name}");
            }
            var rootname = go.transform.root.name;
            if (!string.IsNullOrEmpty(rootname) && rootname[0] == '[')
            {
                var parent = go.transform.parent;
                if (parent && PrefabUtility.GetCorrespondingObjectFromSource(parent)) 
                {
                    continue;
                }

                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (prefab)
                {
                    if (!prefabs.Contains(prefab))
                    {
                        var prefab_pathname = AssetDatabase.GetAssetPath(prefab).ToLower();
                        if (!prefab_pathname.EndsWith(".prefab"))
                        {
                            Log.LogError($"打包日志]prefab path not endwith .prefab, path={prefab_pathname}");
                        }
                        else
                        {
                            prefabs.Add(prefab);
                        }
                    }
                    //
                    if (real_gos != null && prefabs.Contains(prefab))
                    {
                        real_gos.Add(go.transform, prefab);
                    }
                }
            }
        }
        //
        return prefabs;
    }   

    
    static string _build_SceneBundle(string pathname, bool build_Prefab)
    {
        Log.LogInfo($"OpenScene {pathname}");
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(pathname);
        var sb = new StringBuilder();
        XmlWriterSettings sett = new XmlWriterSettings();
        sett.Indent = true;
        sett.NewLineChars = Environment.NewLine;
        sett.Encoding = UTF8Encoding.UTF8;
        //
        //_dept_get_files(pathname);
        //
        using (var writer = XmlWriter.Create(sb, sett))
        {
            writer.WriteStartElement("scene");
            {
                writer.WriteAttributeString("name", scene.name);
                _scene_build_paths.Add(pathname);

                HashSet<GameObject> errors = new HashSet<GameObject>();
                var real_gos = new Dictionary<Transform, GameObject>();
                var prefabs = GetExportGameObjects(real_gos);

                var logs = new Dictionary<GameObject, Transform>();
                foreach (var kv in real_gos)
                {
                    logs[kv.Value] = kv.Key;
                }

                foreach (var prefab in prefabs)
                {
                    var ret = AssetbundleBuilder.check_mat_mesh(AssetDatabase.GetAssetPath(prefab), logs[prefab].gameObject);
                    if (ret)
                    {
                        if (build_Prefab)
                        {
                            _build_Prefab(null, prefab, "prefab_");
                        }
                    }
                    else
                    {
                        errors.Add(prefab);
                    }
                }
                foreach (var kv in real_gos)
                {
                    if (!errors.Contains(kv.Value))
                    {
                        _scene_write_go(writer, kv.Key, kv.Value);
                    }
                }
            }
            writer.WriteEndElement();
            //
            writer.Close();
            var xml_data = sb.ToString();
            CreateSceneXml(scene.name, xml_data);
        }
        return scene.name;
    }

    public static void CreateSceneXml(string name, string xml_data)
    {             //
        var xml = name + ".xml";
        _scene_WriteTextIfChanged(PathDefs.EXPORT_PATH_SCENE + xml, xml_data);
        _scene_WriteTextIfChanged(PathDefs.EXPORT_ROOT_OS + "xmls/" + xml, xml_data);
    }
}
