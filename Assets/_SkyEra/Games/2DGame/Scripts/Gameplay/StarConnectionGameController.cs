using System;
using System.Collections.Generic;
using UnityEngine;
using SkyEra.Games.TwoDGame.Core;
using SkyEra.Games.TwoDGame.Data;

namespace SkyEra.Games.TwoDGame.Gameplay
{
    [DisallowMultipleComponent]
    public class StarConnectionGameController : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private RectTransform gameplayArea;
        [SerializeField] private StarPatternView starPatternView;
        [SerializeField] private GameSessionBootstrap sessionBootstrap;
        [SerializeField] private ConnectionLineView playerConnectionLinePrefab;

        [Header("Player Connection Visuals")]
        [SerializeField] private Color playerConnectionColor =
            new Color(0.20f, 0.90f, 1.00f, 1.00f);

        [Min(1f)]
        [SerializeField] private float playerConnectionThickness = 7f;

        private readonly List<ConnectionLineView> playerConnectionLines =
            new List<ConnectionLineView>();

        private readonly HashSet<string> completedConnectionKeys =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        private StarNodeView selectedStar;
        private bool patternCompleted;
        private bool gameplayLocked;

        public event Action<StarNodeView> StarSelected;
        public event Action<StarNodeView> StarDeselected;

        public event Action<StarNodeView, StarNodeView>
            ValidConnectionCreated;

        public event Action<StarNodeView, StarNodeView>
            InvalidConnectionAttempted;

        public event Action PatternCompleted;
        public event Action GameplayTimedOut;

        public StarNodeView SelectedStar => selectedStar;

        public IReadOnlyList<ConnectionLineView> PlayerConnectionLines =>
            playerConnectionLines;

        public int CompletedConnectionCount =>
            completedConnectionKeys.Count;

        public bool IsPatternCompleted => patternCompleted;

        public bool IsGameplayLocked => gameplayLocked;

        private void OnEnable()
        {
            SubscribeToRuntimeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromRuntimeEvents();
        }

        private void SubscribeToRuntimeEvents()
        {
            if (starPatternView != null)
            {
                starPatternView.StarClicked -= HandleStarClicked;
                starPatternView.StarClicked += HandleStarClicked;
            }

            if (sessionBootstrap != null)
            {
                sessionBootstrap.SessionLoaded -= HandleSessionLoaded;
                sessionBootstrap.SessionLoaded += HandleSessionLoaded;
            }
        }

        private void UnsubscribeFromRuntimeEvents()
        {
            if (starPatternView != null)
            {
                starPatternView.StarClicked -= HandleStarClicked;
            }

            if (sessionBootstrap != null)
            {
                sessionBootstrap.SessionLoaded -= HandleSessionLoaded;
            }
        }

        private void HandleSessionLoaded(GameSessionData session)
        {
            ResetGameplayState();
        }

        private void HandleStarClicked(StarNodeView clickedStar)
        {
            if (clickedStar == null ||
                patternCompleted ||
                gameplayLocked ||
                starPatternView == null ||
                starPatternView.CurrentPattern == null)
            {
                return;
            }

            if (selectedStar == null)
            {
                SelectStar(clickedStar);
                return;
            }

            if (selectedStar == clickedStar)
            {
                ClearSelection();
                return;
            }

            TryCreateConnection(
                selectedStar,
                clickedStar);
        }

        private void TryCreateConnection(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            StarPatternData pattern =
                starPatternView.CurrentPattern;

            if (pattern == null)
            {
                ClearSelection();
                return;
            }

            bool validConnection =
                pattern.ContainsConnection(
                    fromStar.StarId,
                    toStar.StarId);

            if (!validConnection)
            {
                InvalidConnectionAttempted?.Invoke(
                    fromStar,
                    toStar);

                ClearSelection();
                return;
            }

            string connectionKey =
                CreateUndirectedConnectionKey(
                    fromStar.StarId,
                    toStar.StarId);

            if (completedConnectionKeys.Contains(connectionKey))
            {
                SelectStar(toStar);
                return;
            }

            CreatePlayerConnectionLine(
                fromStar,
                toStar);

            completedConnectionKeys.Add(connectionKey);

            ValidConnectionCreated?.Invoke(
                fromStar,
                toStar);

            if (HasCompletedPattern())
            {
                CompletePattern();
                return;
            }

            SelectStar(toStar);
        }

        private void CreatePlayerConnectionLine(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            if (playerConnectionLinePrefab == null)
            {
                Debug.LogError(
                    "[StarConnectionGameController] Player Connection " +
                    "Line Prefab is not assigned.",
                    this);

                return;
            }

            if (gameplayArea == null)
            {
                Debug.LogError(
                    "[StarConnectionGameController] Gameplay Area " +
                    "is not assigned.",
                    this);

                return;
            }

            ConnectionLineView lineView =
                Instantiate(
                    playerConnectionLinePrefab,
                    gameplayArea);

            lineView.name =
                $"PlayerLine_{fromStar.StarId}_{toStar.StarId}";

            lineView.Configure(
                fromStar,
                toStar);

            lineView.SetThickness(
                playerConnectionThickness);

            lineView.SetColor(
                playerConnectionColor);

            PlacePlayerLineBehindStars(lineView);

            playerConnectionLines.Add(lineView);
        }

        private void PlacePlayerLineBehindStars(
            ConnectionLineView lineView)
        {
            if (lineView == null ||
                starPatternView == null)
            {
                return;
            }

            int guideLineCount =
                starPatternView.SpawnedGuideLineCount;

            int siblingIndex =
                Mathf.Clamp(
                    guideLineCount,
                    0,
                    gameplayArea.childCount - 1);

            lineView.transform.SetSiblingIndex(
                siblingIndex);
        }

        private void SelectStar(StarNodeView star)
        {
            if (star == null ||
                gameplayLocked ||
                patternCompleted)
            {
                return;
            }

            if (selectedStar != null &&
                selectedStar != star)
            {
                selectedStar.SetSelected(false);

                StarDeselected?.Invoke(
                    selectedStar);
            }

            selectedStar = star;
            selectedStar.SetSelected(true);

            StarSelected?.Invoke(selectedStar);
        }

        private void ClearSelection()
        {
            if (selectedStar == null)
            {
                return;
            }

            StarNodeView previousSelection =
                selectedStar;

            selectedStar.SetSelected(false);
            selectedStar = null;

            StarDeselected?.Invoke(
                previousSelection);
        }

        private bool HasCompletedPattern()
        {
            StarPatternData pattern =
                starPatternView.CurrentPattern;

            if (pattern == null)
            {
                return false;
            }

            return pattern.ConnectionCount > 0 &&
                   completedConnectionKeys.Count >=
                   pattern.ConnectionCount;
        }

        private void CompletePattern()
        {
            patternCompleted = true;
            gameplayLocked = true;

            ClearSelection();

            if (starPatternView != null)
            {
                starPatternView.SetAllInteractions(false);
            }

            PatternCompleted?.Invoke();

            Debug.Log(
                $"[StarConnectionGameController] Pattern " +
                $"'{starPatternView.CurrentPattern.DisplayName}' " +
                $"completed with " +
                $"{completedConnectionKeys.Count} connections.",
                this);
        }

        public void EndGameplayByTimeout()
        {
            if (patternCompleted ||
                gameplayLocked)
            {
                return;
            }

            gameplayLocked = true;

            ClearSelection();

            if (starPatternView != null)
            {
                starPatternView.SetAllInteractions(false);
            }

            GameplayTimedOut?.Invoke();

            Debug.Log(
                $"[StarConnectionGameController] Gameplay locked " +
                $"because time expired. Completed connections: " +
                $"{completedConnectionKeys.Count}.",
                this);
        }

        public void ResetGameplayState()
        {
            ClearSelection();
            ClearPlayerConnectionLines();

            completedConnectionKeys.Clear();

            patternCompleted = false;
            gameplayLocked = false;

            if (starPatternView != null)
            {
                starPatternView.ResetAllStarStates();
                starPatternView.SetAllInteractions(true);
            }
        }

        public void ClearPlayerConnectionLines()
        {
            for (int i = playerConnectionLines.Count - 1;
                 i >= 0;
                 i--)
            {
                ConnectionLineView line =
                    playerConnectionLines[i];

                if (line == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(line.gameObject);
                }
                else
                {
                    DestroyImmediate(line.gameObject);
                }
            }

            playerConnectionLines.Clear();
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
            playerConnectionThickness =
                Mathf.Max(
                    1f,
                    playerConnectionThickness);

            if (playerConnectionLinePrefab != null &&
                playerConnectionLinePrefab
                    .GetComponent<RectTransform>() == null)
            {
                Debug.LogWarning(
                    "[StarConnectionGameController] The assigned " +
                    "Player Connection Line Prefab does not contain " +
                    "a RectTransform.",
                    this);
            }
        }
#endif
    }
}