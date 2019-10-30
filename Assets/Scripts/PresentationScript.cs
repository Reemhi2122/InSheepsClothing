using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresentationScript : MonoBehaviour
{
    private int _currentDia;
    private SpriteRenderer _spriteRenderer;

    public List<Sprite> Dias;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(NextDia());
    }

    private IEnumerator NextDia()
    {
        _spriteRenderer.sprite = Dias[_currentDia];
        _currentDia = (_currentDia + 1) % (Dias.Count);
        yield return new WaitForSeconds(10);
        StartCoroutine(NextDia());
    }
}
