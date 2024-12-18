using UnityEngine;
using System.Collections;

public class AdjustAnimation : MonoBehaviour {
	public float speed = 1f;
    private float _last_speed = 1f;
	private Animation _animation;
	string _name = "";
    void Start() 
	{
		_animation = gameObject.GetComponent<Animation>();
		_name = _animation.clip.name;
    }

    void Update() 
	{
		if (speed != _last_speed)
		{
			if (_animation.IsPlaying(_name))
			{
				_animation[_name].speed = speed;
				_last_speed = speed;
			}
		}
    }
}
