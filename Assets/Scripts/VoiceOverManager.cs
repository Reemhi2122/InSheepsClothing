using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceOverManager : MonoBehaviour
{
    public static VoiceOverManager Instance = null;

    public AudioClip Vote, Intro;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _audioSource = GetComponent<AudioSource>();
    }

    public void Warn(WarningType a_WarningType)
    {
        if(GameManager.Instance.IsGameOver) return;
        
        switch (a_WarningType)
        {
            case WarningType.Vote:
                PlayClip(Vote);
                break;
        }
        _audioSource.Play();
    }

    public void PlayClip(AudioClip a_AudioClip)
    {
        _audioSource.Stop();
        _audioSource.clip = a_AudioClip;
        _audioSource.Play();
    }

    public enum WarningType
    {
        Vote
    }
}