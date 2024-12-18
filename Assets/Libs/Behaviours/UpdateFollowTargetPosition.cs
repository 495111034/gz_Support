using UnityEngine;

public class UpdateFollowTargetPosition : MonoBehaviour
{
    public Transform target;
    public Transform self;

    public bool is_pos;
    public bool is_rotate;
    public bool is_scale;

    private void Start()
    {
        if (self == null)
        {
            self = transform;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            if (is_pos)
            {
                self.position = target.position;
            }
            if (is_rotate)
            {
                self.rotation = target.rotation;
            }
            if (is_scale)
            {
                self.localScale = target.localScale;
            }
        }
    }
}
