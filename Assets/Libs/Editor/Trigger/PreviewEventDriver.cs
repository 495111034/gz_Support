
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PreviewEventDriver : EditorMonoBehaviour
{
    public static Action startEvent;
    public static Action updateEvent;

    public override void Start()
    {       
        if (startEvent != null)
            try
            {
                startEvent();
            }catch(Exception e)
            {
                Debug.LogException(e);
                startEvent = null;
                EditorSceneTool.CleanHierarchy();
            }

    }

    public override void Update()
    {
        if(updateEvent != null)
            try
            {
                updateEvent();
            }
            catch (Exception e)
            {

                Debug.LogException(e);

                updateEvent = null;
                EditorSceneTool.CleanHierarchy();

            }
    }

    public override void OnPlaymodeStateChanged(PlayModeState playModeState)
    {
        if(playModeState == PlayModeState.PlayingOrWillChangePlayMode)
        {
            startEvent = null;
            updateEvent = null;
            EditorSceneTool.CleanHierarchy();
        }
    }
}
