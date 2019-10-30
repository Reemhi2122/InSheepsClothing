using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance = null;

    public GameObject ScoreUp;
    public TextMeshProUGUI ScoreText;

    private int _score;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    public void AddScore(int a_Points)
    {
        _score += a_Points;
        ScoreText.text = _score.ToString();
        if (a_Points != 0)
        {
            TextMeshProUGUI t = Instantiate(ScoreUp, this.transform).GetComponent<TextMeshProUGUI>();
            t.text = a_Points.ToString();
            Destroy(t.gameObject, 1);
        }
    }
}
