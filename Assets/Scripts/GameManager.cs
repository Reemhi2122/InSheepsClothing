using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    public delegate void GameEventChange(GameEvent a_Event);
    public static event GameEventChange OnGameEventChange;

    public List<SittingSheep> SheepList = new List<SittingSheep>();
    public List<Sheep> SheepLookingAtPlayer = new List<Sheep>();

    private GameEvent _curEvent;

    private int _yesAnswers, _noAnswers, _numVotes, _lives = 3;

    private Coroutine _checkPlayerCoroutine, _votingCoroutine;

    public float VotingChanceMultiplier = 6f, VotingMultiplier = 30f;

    public GameObject[] Hearts;

    public UnityEvent OnStart, OnGameOver;

    public bool IsGameOver, IsVoting;

    public void AddSheepLooking(Sheep s)
    {
        if (!SheepLookingAtPlayer.Contains(s))
            SheepLookingAtPlayer.Add(s);
    }

    public void RemoveSheepLooking(Sheep s)
    {
        if (SheepLookingAtPlayer.Contains(s))
            SheepLookingAtPlayer.Remove(s);
    }

    public bool IsBeingLookedAt()
    {
        if (SheepLookingAtPlayer.Count > 0)
            return true;
        else
            return false;
    }

    public void AddVote(int a_Vote)
    {
        _numVotes++;
        if (a_Vote == 0)
            _noAnswers++;
        if (a_Vote == 1)
            _yesAnswers++;
    }

    public void ResetVotes()
    {
        _numVotes = 0;
        _yesAnswers = 0;
        _noAnswers = 0;
    }

    public int CheckVotes()
    {
        int requiredAnswer = -1;

        if (_numVotes == (SheepList.Count - 2))
        {
            if (_noAnswers > _yesAnswers)
            {
                requiredAnswer = 0;
            }
            else if (_yesAnswers > _noAnswers)
            {
                requiredAnswer = 1;
            }
        }

        return requiredAnswer;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        _votingCoroutine = StartCoroutine(SendVote());

        OnStart.Invoke();
    }

    private IEnumerator SendVote()
    {
        ChangeEvent(GameEvent.None);
        yield return new WaitForSeconds(VotingMultiplier + VotingChanceMultiplier);
        IsVoting = true;
        VoiceOverManager.Instance.Warn(VoiceOverManager.WarningType.Vote);
        ChangeEvent(GameEvent.Voting);
        _votingCoroutine = StartCoroutine(SendVote());
        _checkPlayerCoroutine = StartCoroutine(CheckPlayerVote());
    }

    public void HoldSign(bool a_Choice)
    {
        if (_checkPlayerCoroutine == null) return;

        int i;

        if (a_Choice)
            i = 1;
        else
            i = 0;

        if (CheckVotes() != i)
        {
            Debug.Log("Incorrect!");
            IsVoting = false;
            LoseLife();
        }
        else if (CheckVotes() == i)
        {
            ScoreManager.Instance.AddScore(20);
            Debug.Log("Correct!");
            SoundEffectManager.Instance.PlayCorrect();
            IsVoting = false;
            ChangeEvent(GameEvent.None);
            if(_checkPlayerCoroutine != null)
            {
                StopCoroutine(_checkPlayerCoroutine);
                _checkPlayerCoroutine = null;
            }
            ResetVotes();
        }
    }

    private IEnumerator CheckPlayerVote()
    {
        yield return new WaitForSeconds(VotingChanceMultiplier);
        GameOver();
    }

    public void LoseLife()
    {
        _lives--;
        SoundEffectManager.Instance.PlayIncorrect();
        Hearts[_lives].SetActive(false);

        if (_lives != 0)
            return;

        GameOver();
    }

    public void GameOver()
    {
        IsGameOver = true;
        StopAllCoroutines();
        ChangeEvent(GameEvent.None);
        Debug.Log("Game Over");
        OnGameOver.Invoke();
        StartCoroutine(InitGameOver());
    }

    private IEnumerator InitGameOver()
    {
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene(0);
    }

    public void ChangeEvent(GameEvent a_Event)
    {
        _curEvent = a_Event;
        if(a_Event == GameEvent.Voting)
            IsVoting = true;
        if (OnGameEventChange != null)
            OnGameEventChange(a_Event);
    }

    private void Update()
    {
        if (Time.timeSinceLevelLoad > 13)
        {
            if (VotingMultiplier > 6f)
            {
                VotingMultiplier = 10f + ((Time.timeSinceLevelLoad - 30) * -0.002f);
            }
            if (VotingChanceMultiplier > 3f)
            {
                VotingChanceMultiplier = 30f + ((Time.timeSinceLevelLoad - 30) * -0.01f);
            }
        }
		if (Input.GetKeyDown(KeyCode.R))
			SceneManager.LoadScene(0);
    }
}

public enum GameEvent
{
    None,
    Voting
}
