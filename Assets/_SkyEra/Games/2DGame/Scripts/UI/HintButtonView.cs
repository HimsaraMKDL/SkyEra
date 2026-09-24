using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SkyEra.Games.TwoDGame.Gameplay;

namespace SkyEra.Games.TwoDGame.UI
{
    [DisallowMultipleComponent]
    public class HintButtonView : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField]
        private GameHintController hintController;

        [Header("UI References")]
        [SerializeField]
        private Button hintButton;

        [SerializeField]
        private TMP_Text hintLabel;

        [Header("Display")]
        [SerializeField]
        private string labelFormat = "Hint: {0}";

        private void OnEnable()
        {
            SubscribeToEvents();
            SubscribeToButton();

            RefreshCurrentState();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            UnsubscribeFromButton();
        }

        private void SubscribeToEvents()
        {
            if (hintController == null)
            {
                return;
            }

            hintController.HintsChanged -=
                HandleHintsChanged;

            hintController.HintsChanged +=
                HandleHintsChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (hintController == null)
            {
                return;
            }

            hintController.HintsChanged -=
                HandleHintsChanged;
        }

        private void SubscribeToButton()
        {
            if (hintButton == null)
            {
                return;
            }

            hintButton.onClick.RemoveListener(
                HandleHintClicked);

            hintButton.onClick.AddListener(
                HandleHintClicked);
        }

        private void UnsubscribeFromButton()
        {
            if (hintButton == null)
            {
                return;
            }

            hintButton.onClick.RemoveListener(
                HandleHintClicked);
        }

        private void HandleHintClicked()
        {
            if (hintController == null)
            {
                return;
            }

            hintController.UseHint();
        }

        private void HandleHintsChanged(
            int remaining,
            int total)
        {
            RefreshDisplay(
                remaining,
                total);
        }

        public void RefreshCurrentState()
        {
            if (hintController == null)
            {
                RefreshDisplay(0, 0);
                return;
            }

            RefreshDisplay(
                hintController.HintsRemaining,
                hintController.TotalHints);
        }

        private void RefreshDisplay(
            int remaining,
            int total)
        {
            remaining =
                Mathf.Max(0, remaining);

            total =
                Mathf.Max(0, total);

            if (hintLabel != null)
            {
                hintLabel.text =
                    string.Format(
                        labelFormat,
                        remaining);
            }

            if (hintButton != null)
            {
                hintButton.interactable =
                    total > 0 &&
                    remaining > 0;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (hintButton == null)
            {
                hintButton =
                    GetComponent<Button>();
            }

            if (string.IsNullOrWhiteSpace(labelFormat))
            {
                labelFormat = "Hint: {0}";
            }
        }
#endif
    }
}