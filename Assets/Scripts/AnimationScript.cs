using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public VideoPlayer vidPlayer;

    public void PlayAnim()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        vidPlayer.Play();
        yield return new WaitForSeconds(20);
        SceneManager.LoadScene(1);
    }
}
