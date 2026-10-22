using UnityEngine;
using UnityEngine.UI;  

public class ScoreDisplay : MonoBehaviour {
    [Tooltip("The Text component to update")]
    public Text scoreText;        
    public Text bestText;        

    void Start() {
        if (bestText != null) {
            int best = PlayerPrefs.GetInt("HighScore", 0);
            bestText.text = "Best: " + best;
        }
    }

    void Update() {
        if (scoreText != null) {
            scoreText.text = "Score: " + Main.score;
        }
    }
}