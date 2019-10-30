using UnityEngine;

public abstract class Sheep : MonoBehaviour
{
    protected Animator _animator;
    protected GameEvent _curEvent;

    protected void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnEventChange(GameEvent a_Event)
    {
        _curEvent = a_Event;
    }
}
