using UnityEngine;

public class PlayAnimatorOnEnable : MonoBehaviour
{
    public string anima_name = "";
    public int layer = 0;
    public float normalizedTime = 0;

    public void OnEnable()
    {
        Animator anim = gameObject.GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null && anim.runtimeAnimatorController.animationClips.Length > 0)
        {
            if (string.IsNullOrEmpty(anima_name))
            {
                anim.Play(anim.runtimeAnimatorController.animationClips[0].name, layer, normalizedTime);
            }
            else
            {
                anim.Play(anima_name, layer, normalizedTime);
            }
        }
    }
}
