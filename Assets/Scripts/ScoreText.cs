using UnityEngine;
using TMPro;
public class ScoreText : MonoBehaviour
{
    TextMeshProUGUI text;
    Animator animator;
    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        text = GetComponent<TextMeshProUGUI>();
    }

    public void AddScore(string score)
    {
        text.text = score;
        animator.Play("scoreText_collect");
    }
}