using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StandingSheep : Sheep
{
    public static StandingSheep Instance = null;

    private void Awake()
    {
        base.Awake();

        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        GameManager.OnGameEventChange += OnGameEventChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameEventChange -= OnGameEventChanged;
    }

    private void OnGameEventChanged(GameEvent a_GameEvent)
    {
        switch (a_GameEvent)
        {
            case GameEvent.None:
                _animator.SetBool("Idle", true);
                break;
            case GameEvent.Voting:
                _animator.SetBool("Idle", false);
                break;
        }
    }

    public void LookAt()
    {

        GameManager.Instance.AddSheepLooking(this);
    }

    public void StopLookingAt()
    {

        GameManager.Instance.RemoveSheepLooking(this);
    }
}
