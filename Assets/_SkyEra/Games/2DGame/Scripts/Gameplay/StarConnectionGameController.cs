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
        private sealed class CompletedConnectionRecord
        {
            public string ConnectionKey { get; }
            public StarNodeView FromStar { get; }
            public StarNodeView ToStar { get; }
            public ConnectionLineView LineView { get; }

            public CompletedConnectionRecord(
                string connectionKey,
                StarNodeView fromStar,
                StarNodeView toStar,
                ConnectionLineView lineView)
            {
                ConnectionKey = connectionKey;
                FromStar = fromStar;
                ToStar = toStar;
                LineView = lineView;
            }
        }

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

        private readonly List<CompletedConnectionRecord>
            completedConnectionHistory =
                new List<CompletedConnectionRecord>();

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

        public event Action<StarNodeView, StarNodeView>
            PlayerConnectionUndone;

        public event Action PatternCompleted;
        public event Action GameplayTimedOut;
        public event Action PlayerProgressCleared;

        public StarNodeView SelectedStar => selectedStar;

        public IReadOnlyList<ConnectionLineView> PlayerConnectionLines =>
            playerConnectionLines;

        public int CompletedConnectionCount =>
            completedConnectionKeys.Count;

        public bool IsPatternCompleted => patternCompleted;

        public bool IsGameplayLocked => gameplayLocked;

        public bool HasPlayerProgress =>
            completedConnectionKeys.Count > 0 ||
            selectedStar != null;

        public bool CanUndoConnection =>
            !gameplayLocked &&
            !patternCompleted &&
            completedConnectionHistory.Count > 0;

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

            ConnectionLineView lineView =
                CreatePlayerConnectionLine(
                    fromStar,
                    toStar);

            if (lineView == null)
            {
                ClearSelection();
                return;
            }

            completedConnectionKeys.Add(
                connectionKey);

            completedConnectionHistory.Add(
                new CompletedConnectionRecord(
                    connectionKey,
                    fromStar,
                    toStar,
                    lineView));

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

        private ConnectionLineView CreatePlayerConnectionLine(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            if (playerConnectionLinePrefab == null)
            {
                Debug.LogError(
                    "[StarConnectionGameController] Player Connection " +
                    "Line Prefab is not assigned.",
                    this);

                return null;
            }

            if (gameplayArea == null)
            {
                Debug.LogError(
                    "[StarConnectionGameController] Gameplay Area " +
                    "is not assigned.",
                    this);

                return null;
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

            PlacePlayerLineBehindStars(
                lineView);

            playerConnectionLines.Add(
                lineView);

            return lineView;
        }

        private void PlacePlayerLineBehindStars(
            ConnectionLineView lineView)
        {
            if (lineView == null ||
                starPatternView == null ||
                gameplayArea == null)
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

            StarSelected?.Invoke(
                selectedStar);
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

        public void UndoLastConnection()
        {
            if (gameplayLocked ||
                patternCompleted)
            {
                return;
            }

            ClearSelection();

            if (completedConnectionHistory.Count == 0)
            {
                return;
            }

            int lastIndex =
                completedConnectionHistory.Count - 1;

            CompletedConnectionRecord lastConnection =
                completedConnectionHistory[lastIndex];

            completedConnectionHistory.RemoveAt(
                lastIndex);

            completedConnectionKeys.Remove(
                lastConnection.ConnectionKey);

            if (lastConnection.LineView != null)
            {
                playerConnectionLines.Remove(
                    lastConnection.LineView);

                DestroyPlayerConnectionLine(
                    lastConnection.LineView);
            }

            if (starPatternView != null)
            {
                starPatternView.ResetAllStarStates();
                starPatternView.SetAllInteractions(true);
            }

            PlayerConnectionUndone?.Invoke(
                lastConnection.FromStar,
                lastConnection.ToStar);

            Debug.Log(
                $"[StarConnectionGameController] Undid connection " +
                $"'{lastConnection.FromStar.StarId}' → " +
                $"'{lastConnection.ToStar.StarId}'. " +
                $"Remaining connections: " +
                $"{completedConnectionKeys.Count}.",
                this);
        }

        public void ClearAllPlayerProgress()
        {
            if (gameplayLocked ||
                patternCompleted)
            {
                return;
            }

            ClearSelection();
            ClearPlayerConnectionLines();

            completedConnectionKeys.Clear();
            completedConnectionHistory.Clear();

            if (starPatternView != null)
            {
                starPatternView.ResetAllStarStates();
                starPatternView.SetAllInteractions(true);
            }

            PlayerProgressCleared?.Invoke();

            Debug.Log(
                "[StarConnectionGameController] Player progress cleared.",
                this);
        }

        public void ResetGameplayState()
        {
            ClearSelection();
            ClearPlayerConnectionLines();

            completedConnectionKeys.Clear();
            completedConnectionHistory.Clear();

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

                DestroyPlayerConnectionLine(
                    line);
            }

            playerConnectionLines.Clear();
        }

        private static void DestroyPlayerConnectionLine(
            ConnectionLineView line)
        {
            if (line == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(
                    line.gameObject);
            }
            else
            {
                DestroyImmediate(
                    line.gameObject);
            }
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