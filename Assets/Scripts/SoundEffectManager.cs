using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance = null;

    public AudioClip Correct, Incorrect, Inhale;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayCorrect()
    {
        _audioSource.PlayOneShot(Correct);
    }

    public void PlayIncorrect()
    {
        _audioSource.PlayOneShot(Incorrect);
    }

    public void InhaleAir()
    {
        _audioSource.PlayOneShot(Inhale);
    }

}
