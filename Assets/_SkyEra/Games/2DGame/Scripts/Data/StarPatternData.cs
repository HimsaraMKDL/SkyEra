using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyEra.Games.TwoDGame.Data
{
    [CreateAssetMenu(
        fileName = "StarPatternData",
        menuName = "SkyEra/2D Game/Star Pattern Data",
        order = 0)]
    public class StarPatternData : ScriptableObject
    {
        [Header("Pattern Identity")]
        [SerializeField]
        private string patternId;

        [SerializeField]
        private string displayName;

        [TextArea(2, 5)]
        [SerializeField]
        private string educationalDescription;

        [Header("Pattern Definition")]
        [SerializeField]
        private List<StarNodeDefinition> stars = new List<StarNodeDefinition>();

        [SerializeField]
        private List<StarConnectionDefinition> connections =
            new List<StarConnectionDefinition>();

        public string PatternId => patternId;
        public string DisplayName => displayName;
        public string EducationalDescription => educationalDescription;

        public IReadOnlyList<StarNodeDefinition> Stars => stars;
        public IReadOnlyList<StarConnectionDefinition> Connections => connections;

        public int StarCount => stars != null ? stars.Count : 0;
        public int ConnectionCount => connections != null ? connections.Count : 0;

        public bool TryGetStar(string starId, out StarNodeDefinition star)
        {
            star = null;

            if (string.IsNullOrWhiteSpace(starId) || stars == null)
            {
                return false;
            }

            for (int i = 0; i < stars.Count; i++)
            {
                StarNodeDefinition candidate = stars[i];

                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(
                        candidate.StarId,
                        starId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    star = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool ContainsStar(string starId)
        {
            return TryGetStar(starId, out _);
        }

        public bool ContainsConnection(string firstStarId, string secondStarId)
        {
            if (connections == null)
            {
                return false;
            }

            for (int i = 0; i < connections.Count; i++)
            {
                StarConnectionDefinition connection = connections[i];

                if (connection == null)
                {
                    continue;
                }

                if (connection.Matches(firstStarId, secondStarId))
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (stars == null)
            {
                stars = new List<StarNodeDefinition>();
            }

            if (connections == null)
            {
                connections = new List<StarConnectionDefinition>();
            }

            for (int i = 0; i < stars.Count; i++)
            {
                if (stars[i] == null)
                {
                    continue;
                }

                stars[i].ClampNormalizedPosition();
            }
        }
#endif
    }

    [Serializable]
    public class StarNodeDefinition
    {
        [SerializeField]
        private string starId;

        [SerializeField]
        private string displayName;

        [Tooltip(
            "Position inside the GameplayArea using normalized coordinates. " +
            "(0,0) is bottom-left and (1,1) is top-right.")]
        [SerializeField]
        private Vector2 normalizedPosition = new Vector2(0.5f, 0.5f);

        [SerializeField]
        private bool importantStar;

        public string StarId => starId;
        public string DisplayName => displayName;
        public Vector2 NormalizedPosition => normalizedPosition;
        public bool ImportantStar => importantStar;

        public void ClampNormalizedPosition()
        {
            normalizedPosition = new Vector2(
                Mathf.Clamp01(normalizedPosition.x),
                Mathf.Clamp01(normalizedPosition.y));
        }
    }

    [Serializable]
    public class StarConnectionDefinition
    {
        [SerializeField]
        private string fromStarId;

        [SerializeField]
        private string toStarId;

        public string FromStarId => fromStarId;
        public string ToStarId => toStarId;

        public bool Matches(string firstStarId, string secondStarId)
        {
            bool forwardMatch =
                string.Equals(
                    fromStarId,
                    firstStarId,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    toStarId,
                    secondStarId,
                    StringComparison.OrdinalIgnoreCase);

            bool reverseMatch =
                string.Equals(
                    fromStarId,
                    secondStarId,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    toStarId,
                    firstStarId,
                    StringComparison.OrdinalIgnoreCase);

            return forwardMatch || reverseMatch;
        }
    }
}