using UnityEngine;

namespace SkyEra.Games.TwoDGame.Data
{
    [CreateAssetMenu(
        fileName = "GameSessionData",
        menuName = "SkyEra/2D Game/Game Session Data",
        order = 2)]
    public class GameSessionData : ScriptableObject
    {
        [Header("Session Identity")]
        [SerializeField]
        private string sessionId;

        [SerializeField]
        private string displayName;

        [Min(0)]
        [SerializeField]
        private int displayOrder;

        [Header("Session Content")]
        [SerializeField]
        private StarPatternData starPattern;

        [SerializeField]
        private GameModeData gameMode;

        [Header("Availability")]
        [SerializeField]
        private bool available = true;

        public string SessionId => sessionId;
        public string DisplayName => displayName;
        public int DisplayOrder => displayOrder;

        public StarPatternData StarPattern => starPattern;
        public GameModeData GameMode => gameMode;

        public bool Available => available;

        public bool IsConfigured =>
            starPattern != null &&
            gameMode != null &&
            !string.IsNullOrWhiteSpace(sessionId);

        public string PatternName =>
            starPattern != null
                ? starPattern.DisplayName
                : string.Empty;

        public string ModeName =>
            gameMode != null
                ? gameMode.DisplayName
                : string.Empty;

#if UNITY_EDITOR
        private void OnValidate()
        {
            displayOrder = Mathf.Max(0, displayOrder);
        }
#endif
    }
}