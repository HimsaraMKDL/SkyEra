using TMPro;
using UnityEngine;
using SkyEra.Games.TwoDGame.Gameplay;

namespace SkyEra.Games.TwoDGame.UI
{
    [DisallowMultipleComponent]
    public class ScoreDisplayView : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private GameScoreController scoreController;

        [Header("UI References")]
        [SerializeField] private TMP_Text scoreText;

        [Header("Display")]
        [SerializeField] private string scoreFormat = "Score: {0}";

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshCurrentScore();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (scoreController == null)
            {
                return;
            }

            scoreController.ScoreChanged -= HandleScoreChanged;
            scoreController.ScoreChanged += HandleScoreChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (scoreController == null)
            {
                return;
            }

            scoreController.ScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            RefreshScore(newScore);
        }

        public void RefreshCurrentScore()
        {
            if (scoreController == null)
            {
                RefreshScore(0);
                return;
            }

            RefreshScore(scoreController.CurrentScore);
        }

        public void RefreshScore(int score)
        {
            if (scoreText == null)
            {
                return;
            }

            scoreText.text = string.Format(
                scoreFormat,
                Mathf.Max(0, score));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (scoreText == null)
            {
                scoreText = GetComponent<TMP_Text>();
            }

            if (string.IsNullOrWhiteSpace(scoreFormat))
            {
                scoreFormat = "Score: {0}";
            }
        }
#endif
    }
}