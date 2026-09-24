using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkyEra.Games.TwoDGame.Core;
using SkyEra.Games.TwoDGame.Data;

namespace SkyEra.Games.TwoDGame.Gameplay
{
    [DisallowMultipleComponent]
    public class GameHintController : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField]
        private GameSessionBootstrap sessionBootstrap;

        [SerializeField]
        private StarConnectionGameController connectionController;

        [SerializeField]
        private RectTransform gameplayArea;

        [Header("Hint Visual")]
        [Min(0.1f)]
        [SerializeField]
        private float highlightDuration = 1.5f;

        private readonly HashSet<string> completedConnectionKeys =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        private GameModeData currentMode;

        private int totalHints;
        private int hintsRemaining;

        private Coroutine highlightCoroutine;

        private StarNodeView highlightedStarA;
        private StarNodeView highlightedStarB;

        public event Action<int, int> HintsChanged;

        public event Action<StarNodeView, StarNodeView>
            HintShown;

        public event Action HintUnavailable;

        public int TotalHints => totalHints;

        public int HintsRemaining => hintsRemaining;

        public bool HasHintsRemaining =>
            hintsRemaining > 0;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void Start()
        {
            if (sessionBootstrap != null &&
                sessionBootstrap.HasActiveSession)
            {
                InitializeForSession(
                    sessionBootstrap.CurrentSession);
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();

            StopHintHighlight();
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
                    HandleValidConnectionCreated;

                connectionController.ValidConnectionCreated +=
                    HandleValidConnectionCreated;

                connectionController.PlayerConnectionUndone -=
                    HandleConnectionUndone;

                connectionController.PlayerConnectionUndone +=
                    HandleConnectionUndone;

                connectionController.PlayerProgressCleared -=
                    HandlePlayerProgressCleared;

                connectionController.PlayerProgressCleared +=
                    HandlePlayerProgressCleared;
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
                    HandleValidConnectionCreated;

                connectionController.PlayerConnectionUndone -=
                    HandleConnectionUndone;

                connectionController.PlayerProgressCleared -=
                    HandlePlayerProgressCleared;
            }
        }

        private void HandleSessionLoaded(
            GameSessionData session)
        {
            InitializeForSession(session);
        }

        private void InitializeForSession(
            GameSessionData session)
        {
            StopHintHighlight();

            completedConnectionKeys.Clear();

            if (session == null ||
                session.GameMode == null)
            {
                currentMode = null;
                totalHints = 0;
                hintsRemaining = 0;

                HintsChanged?.Invoke(
                    hintsRemaining,
                    totalHints);

                return;
            }

            currentMode = session.GameMode;

            totalHints =
                Mathf.Max(
                    0,
                    currentMode.AvailableHints);

            hintsRemaining = totalHints;

            HintsChanged?.Invoke(
                hintsRemaining,
                totalHints);

            Debug.Log(
                $"[GameHintController] Hints initialized: " +
                $"{hintsRemaining}/{totalHints} for mode " +
                $"'{currentMode.DisplayName}'.",
                this);
        }

        private void HandleValidConnectionCreated(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            if (fromStar == null ||
                toStar == null)
            {
                return;
            }

            completedConnectionKeys.Add(
                CreateUndirectedConnectionKey(
                    fromStar.StarId,
                    toStar.StarId));
        }

        private void HandleConnectionUndone(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            if (fromStar == null ||
                toStar == null)
            {
                return;
            }

            completedConnectionKeys.Remove(
                CreateUndirectedConnectionKey(
                    fromStar.StarId,
                    toStar.StarId));
        }

        private void HandlePlayerProgressCleared()
        {
            completedConnectionKeys.Clear();

            StopHintHighlight();
        }

        public void UseHint()
        {
            if (connectionController == null ||
                sessionBootstrap == null ||
                gameplayArea == null ||
                currentMode == null)
            {
                Debug.LogError(
                    "[GameHintController] Hint system is missing " +
                    "one or more required references.",
                    this);

                return;
            }

            if (connectionController.IsGameplayLocked ||
                connectionController.IsPatternCompleted)
            {
                return;
            }

            if (hintsRemaining <= 0)
            {
                HintUnavailable?.Invoke();

                Debug.Log(
                    "[GameHintController] No hints remaining.",
                    this);

                return;
            }

            if (!TryFindHintConnection(
                    out StarNodeView fromStar,
                    out StarNodeView toStar))
            {
                HintUnavailable?.Invoke();

                Debug.Log(
                    "[GameHintController] No incomplete valid " +
                    "connection is available to hint.",
                    this);

                return;
            }

            hintsRemaining--;

            HintsChanged?.Invoke(
                hintsRemaining,
                totalHints);

            ShowHint(
                fromStar,
                toStar);

            HintShown?.Invoke(
                fromStar,
                toStar);

            Debug.Log(
                $"[GameHintController] Hint shown for " +
                $"'{fromStar.StarId}' → '{toStar.StarId}'. " +
                $"Hints remaining: {hintsRemaining}.",
                this);
        }

        private bool TryFindHintConnection(
            out StarNodeView fromStar,
            out StarNodeView toStar)
        {
            fromStar = null;
            toStar = null;

            if (sessionBootstrap.CurrentSession == null ||
                sessionBootstrap.CurrentSession.StarPattern == null)
            {
                return false;
            }

            StarPatternData pattern =
                sessionBootstrap.CurrentSession.StarPattern;

            StarNodeView[] stars =
                gameplayArea.GetComponentsInChildren
                    <StarNodeView>(false);

            if (stars == null ||
                stars.Length < 2)
            {
                return false;
            }

            StarNodeView selectedStar =
                connectionController.SelectedStar;

            // Prefer a connection from the player's
            // currently selected star.
            if (selectedStar != null)
            {
                for (int i = 0;
                     i < stars.Length;
                     i++)
                {
                    StarNodeView candidate =
                        stars[i];

                    if (candidate == null ||
                        candidate == selectedStar)
                    {
                        continue;
                    }

                    if (!IsIncompleteValidConnection(
                            pattern,
                            selectedStar,
                            candidate))
                    {
                        continue;
                    }

                    fromStar = selectedStar;
                    toStar = candidate;

                    return true;
                }
            }

            // Otherwise find any remaining valid connection.
            for (int i = 0;
                 i < stars.Length - 1;
                 i++)
            {
                StarNodeView firstStar =
                    stars[i];

                if (firstStar == null)
                {
                    continue;
                }

                for (int j = i + 1;
                     j < stars.Length;
                     j++)
                {
                    StarNodeView secondStar =
                        stars[j];

                    if (secondStar == null)
                    {
                        continue;
                    }

                    if (!IsIncompleteValidConnection(
                            pattern,
                            firstStar,
                            secondStar))
                    {
                        continue;
                    }

                    fromStar = firstStar;
                    toStar = secondStar;

                    return true;
                }
            }

            return false;
        }

        private bool IsIncompleteValidConnection(
            StarPatternData pattern,
            StarNodeView firstStar,
            StarNodeView secondStar)
        {
            if (pattern == null ||
                firstStar == null ||
                secondStar == null)
            {
                return false;
            }

            if (!pattern.ContainsConnection(
                    firstStar.StarId,
                    secondStar.StarId))
            {
                return false;
            }

            string connectionKey =
                CreateUndirectedConnectionKey(
                    firstStar.StarId,
                    secondStar.StarId);

            return
                !completedConnectionKeys.Contains(
                    connectionKey);
        }

        private void ShowHint(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            StopHintHighlight();

            highlightedStarA = fromStar;
            highlightedStarB = toStar;

            StarNodeView selectedStar =
                connectionController.SelectedStar;

            if (currentMode.HighlightNextValidStar &&
                selectedStar != null &&
                selectedStar == fromStar)
            {
                // Keep the player's selected star unchanged.
                // Strongly highlight only the suggested next star.
                ApplyHintVisual(
                    toStar);
            }
            else
            {
                // With no useful current selection,
                // highlight both ends of the suggested connection.
                ApplyHintVisual(
                    fromStar);

                ApplyHintVisual(
                    toStar);
            }

            highlightCoroutine =
                StartCoroutine(
                    HideHintAfterDelay());
        }

        private void ApplyHintVisual(
            StarNodeView star)
        {
            if (star == null)
            {
                return;
            }

            // Use the strong selected visual temporarily.
            // This affects only the view, not gameplay selection.
            star.SetSelected(true);
            star.SetGlowVisible(true);
        }

        private IEnumerator HideHintAfterDelay()
        {
            yield return
                new WaitForSecondsRealtime(
                    highlightDuration);

            RestoreHighlightedStars();

            highlightCoroutine = null;
        }

        private void StopHintHighlight()
        {
            if (highlightCoroutine != null)
            {
                StopCoroutine(
                    highlightCoroutine);

                highlightCoroutine = null;
            }

            RestoreHighlightedStars();
        }

        private void RestoreHighlightedStars()
        {
            RestoreStarVisual(
                highlightedStarA);

            RestoreStarVisual(
                highlightedStarB);

            highlightedStarA = null;
            highlightedStarB = null;
        }

        private void RestoreStarVisual(
            StarNodeView star)
        {
            if (star == null)
            {
                return;
            }

            bool isActuallySelected =
                connectionController != null &&
                connectionController.SelectedStar == star;

            star.SetSelected(
                isActuallySelected);

            star.SetGlowVisible(
                isActuallySelected);
        }

        private static string CreateUndirectedConnectionKey(
            string firstStarId,
            string secondStarId)
        {
            if (string.Compare(
                    firstStarId,
                    secondStarId,
                    StringComparison.OrdinalIgnoreCase) <= 0)
            {
                return
                    $"{firstStarId}|{secondStarId}";
            }

            return
                $"{secondStarId}|{firstStarId}";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            highlightDuration =
                Mathf.Max(
                    0.1f,
                    highlightDuration);

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