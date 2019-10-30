using System.Collections;
using UnityEngine;

public class SittingSheep : Sheep
{
    public bool IsPlayer = false;

    private Transform Target;

    Transform chair;

    private IEnumerator LookRandom()
    {
        yield return new WaitForSeconds(Random.Range(15, 100));
        if(!GameManager.Instance.IsVoting)
        {
            Target = ISCPlayer.Instance.transform;
            GameManager.Instance.AddSheepLooking(this);
        }
        yield return new WaitForSeconds(Random.Range(3, 7));
        if (!GameManager.Instance.IsVoting)
        {
            Target = StandingSheep.Instance.transform;
            GameManager.Instance.RemoveSheepLooking(this);
        }
        StartCoroutine(LookRandom());
    }

    private void Update()
    {
        if (IsPlayer) return;

        Quaternion rotation;

        Vector3 lookPos = Target.position - chair.position;
        lookPos.y = 0;
        rotation = Quaternion.LookRotation(lookPos);

        chair.rotation = Quaternion.Slerp(chair.rotation, rotation, 0.1f);
    }

    private void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        Target = StandingSheep.Instance.gameObject.transform;
        chair = this.transform.parent;

        if (!IsPlayer)
            StartCoroutine(LookRandom());
    }

    private void OnEnable()
    {
        if(!IsPlayer)
            GameManager.OnGameEventChange += OnEventChange;
    }

    private void OnDisable()
    {
                if(!IsPlayer)
        GameManager.OnGameEventChange -= OnEventChange;
    }

    public void OnEventChange(GameEvent a_Event)
    {
        _curEvent = a_Event;
        switch (_curEvent)
        {
            case GameEvent.None:

                GameManager.Instance.RemoveSheepLooking(this);
                _animator.SetBool("Hold", false);
                Target = StandingSheep.Instance.gameObject.transform;
                break;
            case GameEvent.Voting:
                _animator.SetBool("Hold", false);

                StartCoroutine(Vote());
                break;
        }
    }

    private IEnumerator Vote()
    {
        yield return new WaitForSeconds(Random.Range(1, 2));
        int random = Random.Range(0, 2);
        Target = ISCPlayer.Instance.gameObject.transform;
        GameManager.Instance.AddSheepLooking(this);
        GameManager.Instance.AddVote(random);
        _animator.SetBool("Hold", true);
        _animator.SetInteger("Side", random);
        yield return new WaitForSeconds(GameManager.Instance.VotingChanceMultiplier);
        GameManager.Instance.RemoveSheepLooking(this);
        _animator.SetBool("Hold", false);
    }

    private IEnumerator LookAtS(Transform a_Target, float time = 3)
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        Transform chair = this.transform.parent;
        Vector3 lookPos = a_Target.position - chair.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);

        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            chair.rotation = Quaternion.Slerp(chair.rotation, rotation, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
