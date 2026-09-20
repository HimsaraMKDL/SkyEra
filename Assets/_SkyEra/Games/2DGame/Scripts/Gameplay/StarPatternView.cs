using System;
using System.Collections.Generic;
using UnityEngine;
using SkyEra.Games.TwoDGame.Data;

namespace SkyEra.Games.TwoDGame.Gameplay
{
    public class StarPatternView : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private RectTransform gameplayArea;
        [SerializeField] private StarNodeView starNodePrefab;
        [SerializeField] private ConnectionLineView connectionLinePrefab;

        private readonly List<StarNodeView> spawnedStars =
            new List<StarNodeView>();

        private readonly List<ConnectionLineView> spawnedGuideLines =
            new List<ConnectionLineView>();

        private readonly Dictionary<string, StarNodeView> starsById =
            new Dictionary<string, StarNodeView>(
                StringComparer.OrdinalIgnoreCase);

        private StarPatternData currentPattern;

        public event Action<StarNodeView> StarClicked;

        public StarPatternData CurrentPattern => currentPattern;

        public IReadOnlyList<StarNodeView> SpawnedStars =>
            spawnedStars;

        public IReadOnlyList<ConnectionLineView> SpawnedGuideLines =>
            spawnedGuideLines;

        public int SpawnedStarCount => spawnedStars.Count;

        public int SpawnedGuideLineCount =>
            spawnedGuideLines.Count;

        public bool HasPattern => currentPattern != null;

        public bool GuideLinesVisible =>
            spawnedGuideLines.Count > 0;

        public void BuildPattern(
            StarPatternData pattern,
            bool showLabels,
            bool enableInteraction = true,
            bool showGuideLines = true)
        {
            ClearPattern();

            if (pattern == null)
            {
                Debug.LogError(
                    "[StarPatternView] Cannot build pattern because " +
                    "the StarPatternData reference is null.",
                    this);

                return;
            }

            if (gameplayArea == null)
            {
                Debug.LogError(
                    "[StarPatternView] Gameplay Area is not assigned.",
                    this);

                return;
            }

            if (starNodePrefab == null)
            {
                Debug.LogError(
                    "[StarPatternView] Star Node Prefab is not assigned.",
                    this);

                return;
            }

            if (connectionLinePrefab == null)
            {
                Debug.LogError(
                    "[StarPatternView] Connection Line Prefab is not assigned.",
                    this);

                return;
            }

            currentPattern = pattern;

            CreateStars(
                pattern,
                showLabels,
                enableInteraction);

            if (showGuideLines)
            {
                CreateGuideConnections(pattern);
            }
        }

        public void ClearPattern()
        {
            ClearGuideConnections();

            for (int i = spawnedStars.Count - 1; i >= 0; i--)
            {
                StarNodeView star = spawnedStars[i];

                if (star == null)
                {
                    continue;
                }

                star.Clicked -= HandleStarClicked;

                if (Application.isPlaying)
                {
                    Destroy(star.gameObject);
                }
                else
                {
                    DestroyImmediate(star.gameObject);
                }
            }

            spawnedStars.Clear();
            starsById.Clear();
            currentPattern = null;
        }

        public bool TryGetStar(
            string starId,
            out StarNodeView starView)
        {
            starView = null;

            if (string.IsNullOrWhiteSpace(starId))
            {
                return false;
            }

            return starsById.TryGetValue(
                starId,
                out starView);
        }

        public void SetAllInteractions(bool interactable)
        {
            for (int i = 0; i < spawnedStars.Count; i++)
            {
                StarNodeView star = spawnedStars[i];

                if (star != null)
                {
                    star.SetInteractable(interactable);
                }
            }
        }

        public void SetAllLabelsVisible(bool visible)
        {
            for (int i = 0; i < spawnedStars.Count; i++)
            {
                StarNodeView star = spawnedStars[i];

                if (star != null)
                {
                    star.SetLabelVisible(visible);
                }
            }
        }

        public void SetGuideLinesVisible(bool visible)
        {
            if (!visible)
            {
                ClearGuideConnections();
                return;
            }

            if (currentPattern == null)
            {
                return;
            }

            if (spawnedGuideLines.Count > 0)
            {
                return;
            }

            CreateGuideConnections(currentPattern);
        }

        public void ResetAllStarStates()
        {
            for (int i = 0; i < spawnedStars.Count; i++)
            {
                StarNodeView star = spawnedStars[i];

                if (star != null)
                {
                    star.ResetState();
                }
            }
        }

        public void RefreshGuideConnections()
        {
            for (int i = 0;
                 i < spawnedGuideLines.Count;
                 i++)
            {
                ConnectionLineView line =
                    spawnedGuideLines[i];

                if (line != null)
                {
                    line.RefreshLine();
                }
            }
        }

        private void CreateStars(
            StarPatternData pattern,
            bool showLabels,
            bool enableInteraction)
        {
            for (int i = 0; i < pattern.Stars.Count; i++)
            {
                StarNodeDefinition starDefinition =
                    pattern.Stars[i];

                if (starDefinition == null)
                {
                    Debug.LogWarning(
                        $"[StarPatternView] Pattern " +
                        $"'{pattern.DisplayName}' contains a null " +
                        $"star entry at index {i}.",
                        this);

                    continue;
                }

                CreateStar(
                    starDefinition,
                    showLabels,
                    enableInteraction);
            }
        }

        private void CreateStar(
            StarNodeDefinition starDefinition,
            bool showLabel,
            bool enableInteraction)
        {
            if (string.IsNullOrWhiteSpace(
                    starDefinition.StarId))
            {
                Debug.LogError(
                    $"[StarPatternView] Pattern " +
                    $"'{currentPattern.DisplayName}' contains a " +
                    "star with an empty Star ID.",
                    this);

                return;
            }

            if (starsById.ContainsKey(
                    starDefinition.StarId))
            {
                Debug.LogError(
                    $"[StarPatternView] Duplicate star ID " +
                    $"'{starDefinition.StarId}' was found while " +
                    $"building pattern " +
                    $"'{currentPattern.DisplayName}'.",
                    this);

                return;
            }

            StarNodeView starView = Instantiate(
                starNodePrefab,
                gameplayArea);

            starView.name =
                $"Star_{starDefinition.StarId}";

            starView.Configure(
                starDefinition,
                showLabel,
                enableInteraction);

            PositionStar(
                starView.RectTransform,
                starDefinition.NormalizedPosition);

            starView.Clicked += HandleStarClicked;

            spawnedStars.Add(starView);

            starsById.Add(
                starDefinition.StarId,
                starView);
        }

        private void CreateGuideConnections(
            StarPatternData pattern)
        {
            for (int i = 0;
                 i < pattern.Connections.Count;
                 i++)
            {
                StarConnectionDefinition connection =
                    pattern.Connections[i];

                if (connection == null)
                {
                    Debug.LogWarning(
                        $"[StarPatternView] Pattern " +
                        $"'{pattern.DisplayName}' contains a null " +
                        $"connection entry at index {i}.",
                        this);

                    continue;
                }

                CreateGuideConnection(
                    connection,
                    i);
            }
        }

        private void CreateGuideConnection(
            StarConnectionDefinition connection,
            int connectionIndex)
        {
            if (!TryGetStar(
                    connection.FromStarId,
                    out StarNodeView fromStar))
            {
                Debug.LogError(
                    $"[StarPatternView] Cannot create guide " +
                    $"connection {connectionIndex} because star " +
                    $"'{connection.FromStarId}' was not spawned.",
                    this);

                return;
            }

            if (!TryGetStar(
                    connection.ToStarId,
                    out StarNodeView toStar))
            {
                Debug.LogError(
                    $"[StarPatternView] Cannot create guide " +
                    $"connection {connectionIndex} because star " +
                    $"'{connection.ToStarId}' was not spawned.",
                    this);

                return;
            }

            ConnectionLineView lineView =
                Instantiate(
                    connectionLinePrefab,
                    gameplayArea);

            lineView.name =
                $"GuideLine_{connection.FromStarId}_" +
                $"{connection.ToStarId}";

            lineView.Configure(
                fromStar,
                toStar);

            // Guide lines always render behind stars.
            lineView.transform.SetAsFirstSibling();

            spawnedGuideLines.Add(lineView);
        }

        private void ClearGuideConnections()
        {
            for (int i = spawnedGuideLines.Count - 1;
                 i >= 0;
                 i--)
            {
                ConnectionLineView line =
                    spawnedGuideLines[i];

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

            spawnedGuideLines.Clear();
        }

        private static void PositionStar(
            RectTransform starRect,
            Vector2 normalizedPosition)
        {
            if (starRect == null)
            {
                return;
            }

            Vector2 clampedPosition =
                new Vector2(
                    Mathf.Clamp01(normalizedPosition.x),
                    Mathf.Clamp01(normalizedPosition.y));

            starRect.anchorMin = clampedPosition;
            starRect.anchorMax = clampedPosition;

            starRect.pivot =
                new Vector2(0.5f, 0.5f);

            starRect.anchoredPosition =
                Vector2.zero;

            starRect.localRotation =
                Quaternion.identity;

            starRect.localScale =
                Vector3.one;
        }

        private void HandleStarClicked(
            StarNodeView starView)
        {
            if (starView == null)
            {
                return;
            }

            StarClicked?.Invoke(starView);
        }

        private void OnDestroy()
        {
            for (int i = 0;
                 i < spawnedStars.Count;
                 i++)
            {
                StarNodeView star =
                    spawnedStars[i];

                if (star != null)
                {
                    star.Clicked -=
                        HandleStarClicked;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (starNodePrefab != null &&
                starNodePrefab.GetComponent<RectTransform>()
                == null)
            {
                Debug.LogWarning(
                    "[StarPatternView] The assigned Star Node " +
                    "Prefab does not contain a RectTransform.",
                    this);
            }

            if (connectionLinePrefab != null &&
                connectionLinePrefab
                    .GetComponent<RectTransform>() == null)
            {
                Debug.LogWarning(
                    "[StarPatternView] The assigned Connection " +
                    "Line Prefab does not contain a RectTransform.",
                    this);
            }
        }
#endif
    }
}