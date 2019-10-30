using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Sheep
{
    private void Awake()
    {
        base.Awake();
        StartCoroutine(PlayEvent());
    }

    private IEnumerator PlayEvent()
    {
        yield return new WaitForSeconds(Random.Range(15, 30));
        _animator.SetBool("IsOut", true);
        yield return new WaitForSeconds(Random.Range(10, 15));
        _animator.SetBool("IsOut", false);
        StartCoroutine(PlayEvent());
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
