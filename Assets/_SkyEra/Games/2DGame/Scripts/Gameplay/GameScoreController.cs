using System;
using UnityEngine;
using SkyEra.Games.TwoDGame.Core;
using SkyEra.Games.TwoDGame.Data;

namespace SkyEra.Games.TwoDGame.Gameplay
{
    [DisallowMultipleComponent]
    public class GameScoreController : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private GameSessionBootstrap sessionBootstrap;
        [SerializeField] private StarConnectionGameController connectionController;

        private int currentScore;
        private GameModeData currentMode;

        public event Action<int> ScoreChanged;

        public int CurrentScore => currentScore;

        public GameModeData CurrentMode => currentMode;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (sessionBootstrap != null)
            {
                sessionBootstrap.SessionLoaded -=
                    HandleSessionLoaded;

                sessionBootstrap.SessionLoaded +=
                    HandleSessionLoaded;
            }

            if (connectionController != null)
            {
                connectionController.ValidConnectionCreated -=
                    HandleValidConnection;

                connectionController.ValidConnectionCreated +=
                    HandleValidConnection;

                connectionController.InvalidConnectionAttempted -=
                    HandleInvalidConnection;

                connectionController.InvalidConnectionAttempted +=
                    HandleInvalidConnection;

                connectionController.PatternCompleted -=
                    HandlePatternCompleted;

                connectionController.PatternCompleted +=
                    HandlePatternCompleted;

                connectionController.PlayerProgressCleared -=
                    HandlePlayerProgressCleared;

                connectionController.PlayerProgressCleared +=
                    HandlePlayerProgressCleared;

                connectionController.PlayerConnectionUndone -=
                    HandlePlayerConnectionUndone;

                connectionController.PlayerConnectionUndone +=
                    HandlePlayerConnectionUndone;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (sessionBootstrap != null)
            {
                sessionBootstrap.SessionLoaded -=
                    HandleSessionLoaded;
            }

            if (connectionController != null)
            {
                connectionController.ValidConnectionCreated -=
                    HandleValidConnection;

                connectionController.InvalidConnectionAttempted -=
                    HandleInvalidConnection;

                connectionController.PatternCompleted -=
                    HandlePatternCompleted;

                connectionController.PlayerProgressCleared -=
                    HandlePlayerProgressCleared;

                connectionController.PlayerConnectionUndone -=
                    HandlePlayerConnectionUndone;
            }
        }

        private void HandleSessionLoaded(
            GameSessionData session)
        {
            if (session == null ||
                session.GameMode == null)
            {
                Debug.LogError(
                    "[GameScoreController] Cannot initialize score " +
                    "because the loaded session or GameModeData is null.",
                    this);

                return;
            }

            currentMode = session.GameMode;

            SetScore(
                currentMode.StartingScore);

            Debug.Log(
                $"[GameScoreController] Score initialized to " +
                $"{currentScore} for mode '{currentMode.DisplayName}'.",
                this);
        }

        private void HandleValidConnection(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            if (currentMode == null)
            {
                return;
            }

            AddScore(
                currentMode.CorrectConnectionScore);

            Debug.Log(
                $"[GameScoreController] Correct connection " +
                $"'{fromStar.StarId}' → '{toStar.StarId}'. " +
                $"Score: {currentScore}.",
                this);
        }

        private void HandleInvalidConnection(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            if (currentMode == null)
            {
                return;
            }

            int penalty =
                currentMode.IncorrectConnectionPenalty;

            if (penalty <= 0)
            {
                return;
            }

            int minimumScore =
                Mathf.Max(
                    0,
                    currentMode.StartingScore);

            SetScore(
                Mathf.Max(
                    minimumScore,
                    currentScore - penalty));

            Debug.Log(
                $"[GameScoreController] Incorrect connection " +
                $"'{fromStar.StarId}' → '{toStar.StarId}'. " +
                $"Penalty: {penalty}. Score: {currentScore}.",
                this);
        }

        private void HandlePlayerConnectionUndone(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            if (currentMode == null)
            {
                return;
            }

            int connectionScore =
                Mathf.Max(
                    0,
                    currentMode.CorrectConnectionScore);

            int minimumScore =
                Mathf.Max(
                    0,
                    currentMode.StartingScore);

            SetScore(
                Mathf.Max(
                    minimumScore,
                    currentScore - connectionScore));

            Debug.Log(
                $"[GameScoreController] Undo connection " +
                $"'{fromStar.StarId}' → '{toStar.StarId}'. " +
                $"Removed {connectionScore} points. " +
                $"Score: {currentScore}.",
                this);
        }

        private void HandlePatternCompleted()
        {
            if (currentMode == null)
            {
                return;
            }

            AddScore(
                currentMode.CompletionBonus);

            Debug.Log(
                $"[GameScoreController] Completion bonus " +
                $"+{currentMode.CompletionBonus}. " +
                $"Final score: {currentScore}.",
                this);
        }

        private void HandlePlayerProgressCleared()
        {
            ResetScore();

            Debug.Log(
                $"[GameScoreController] Score reset to " +
                $"{currentScore} because player progress was cleared.",
                this);
        }

        public void ResetScore()
        {
            if (currentMode == null)
            {
                SetScore(0);
                return;
            }

            SetScore(
                currentMode.StartingScore);
        }

        private void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetScore(
                currentScore + amount);
        }

        private void SetScore(int value)
        {
            currentScore =
                Mathf.Max(
                    0,
                    value);

            ScoreChanged?.Invoke(
                currentScore);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sessionBootstrap == null)
            {
                sessionBootstrap =
                    GetComponent<GameSessionBootstrap>();
            }

            if (connectionController == null)
            {
                connectionController =
                    GetComponent<StarConnectionGameController>();
            }
        }
#endif
    }
}