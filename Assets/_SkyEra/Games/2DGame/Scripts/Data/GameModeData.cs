using UnityEngine;

namespace SkyEra.Games.TwoDGame.Data
{
    [CreateAssetMenu(
        fileName = "GameModeData",
        menuName = "SkyEra/2D Game/Game Mode Data",
        order = 1)]
    public class GameModeData : ScriptableObject
    {
        [Header("Mode Identity")]
        [SerializeField]
        private string modeId;

        [SerializeField]
        private string displayName;

        [TextArea(2, 5)]
        [SerializeField]
        private string instructions;

        [Header("Session Rules")]
        [Min(0f)]
        [SerializeField]
        private float timeLimitSeconds = 60f;

        [Min(0)]
        [SerializeField]
        private int startingScore = 0;

        [Min(0)]
        [SerializeField]
        private int correctConnectionScore = 10;

        [Min(0)]
        [SerializeField]
        private int incorrectConnectionPenalty = 0;

        [Min(0)]
        [SerializeField]
        private int completionBonus = 50;

        [Header("Player Assistance")]
        [Min(0)]
        [SerializeField]
        private int availableHints = 0;

        [SerializeField]
        private bool showStarLabels;

        [SerializeField]
        private bool showGuideLines;

        [SerializeField]
        private bool highlightNextValidStar;

        [Header("Interaction Rules")]
        [SerializeField]
        private bool allowConnectionUndo = true;

        [SerializeField]
        private bool allowClearAll = true;

        [SerializeField]
        private bool randomizeStartingStar;

        public string ModeId => modeId;
        public string DisplayName => displayName;
        public string Instructions => instructions;

        public float TimeLimitSeconds => timeLimitSeconds;
        public int StartingScore => startingScore;
        public int CorrectConnectionScore => correctConnectionScore;
        public int IncorrectConnectionPenalty => incorrectConnectionPenalty;
        public int CompletionBonus => completionBonus;

        public int AvailableHints => availableHints;
        public bool ShowStarLabels => showStarLabels;
        public bool ShowGuideLines => showGuideLines;
        public bool HighlightNextValidStar => highlightNextValidStar;

        public bool AllowConnectionUndo => allowConnectionUndo;
        public bool AllowClearAll => allowClearAll;
        public bool RandomizeStartingStar => randomizeStartingStar;

        public bool UsesTimer => timeLimitSeconds > 0f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            timeLimitSeconds = Mathf.Max(0f, timeLimitSeconds);
            startingScore = Mathf.Max(0, startingScore);
            correctConnectionScore = Mathf.Max(0, correctConnectionScore);
            incorrectConnectionPenalty =
                Mathf.Max(0, incorrectConnectionPenalty);
            completionBonus = Mathf.Max(0, completionBonus);
            availableHints = Mathf.Max(0, availableHints);
        }
#endif
    }
}