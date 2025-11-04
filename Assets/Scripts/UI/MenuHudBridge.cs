using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuHudBridge : MonoBehaviour
{
    [Header("Label opcional")]
    [SerializeField] private TMP_Text difficultyLabel;
    [SerializeField] private Text legacyDifficultyLabel;
    [SerializeField] private string easyLabel = "Dificuldade: Facil";
    [SerializeField] private string normalLabel = "Dificuldade: Normal";
    [SerializeField] private string hardLabel = "Dificuldade: Dificil";

    [Header("Navegacao")]
    [SerializeField] private Selectable[] navigationTargets;
    [SerializeField] private bool autoSelectFirst = true;
    [SerializeField] private RectTransform[] focusTargets;

    private ConversiaGameBridge bridge;
    private int currentSelectionIndex = -1;

    private void Awake()
    {
        AttachBridgeIfNeeded();
    }

    private void Start()
    {
        AttachBridgeIfNeeded();
        RefreshDifficultyLabel(bridge?.CurrentDifficulty);
        EnsureInitialSelection();
    }

    private void OnEnable()
    {
        AttachBridgeIfNeeded();
        RefreshDifficultyLabel(bridge?.CurrentDifficulty);
        EnsureInitialSelection();
    }

    private void OnDisable()
    {
        if (bridge != null)
        {
            bridge.ClearHudFocus();
            bridge.DifficultyChanged -= HandleDifficultyChanged;
            bridge.NavigationAnalogChanged -= HandleNavigationAnalogChanged;
            bridge.NavigationLevelChanged -= HandleNavigationLevelChanged;
            bridge = null;
        }
    }

    public void StartGame()
    {
        AttachBridgeIfNeeded();
        bridge?.StartGameFromMenuButton();
    }

    public void ToggleDifficulty()
    {
        AttachBridgeIfNeeded();
        bridge?.ToggleDifficultyFromMenu();
    }

    public void ExitGame()
    {
        AttachBridgeIfNeeded();
        bridge?.ExitGame();
    }

    private void AttachBridgeIfNeeded()
    {
        if (bridge != null)
        {
            return;
        }

        bridge = ConversiaGameBridge.Instance;
        if (bridge == null)
        {
            bridge = FindObjectOfType<ConversiaGameBridge>();
        }

        if (bridge != null)
        {
            bridge.DifficultyChanged += HandleDifficultyChanged;
            bridge.NavigationAnalogChanged += HandleNavigationAnalogChanged;
            bridge.NavigationLevelChanged += HandleNavigationLevelChanged;
            SendFocusForCurrentSelection();
        }
    }

    private void HandleDifficultyChanged(string difficulty)
    {
        RefreshDifficultyLabel(difficulty);
        SendFocusForCurrentSelection();
    }

    private void HandleNavigationAnalogChanged(string channel)
    {
        if (!string.IsNullOrEmpty(channel))
        {
            EnsureInitialSelection();
        }
        else
        {
            bridge?.ClearHudFocus();
        }
    }

    private void HandleNavigationLevelChanged(float value)
    {
        ApplyNavigationValue(value);
    }

    private void RefreshDifficultyLabel(string difficulty)
    {
        if (string.IsNullOrEmpty(difficulty))
        {
            difficulty = "normal";
        }

        if (difficultyLabel == null && legacyDifficultyLabel == null)
        {
            return;
        }

        var text = difficulty switch
        {
            "easy" => easyLabel,
            "hard" => hardLabel,
            _ => normalLabel,
        };

        if (difficultyLabel != null)
        {
            difficultyLabel.text = text;
        }

        if (legacyDifficultyLabel != null)
        {
            legacyDifficultyLabel.text = text;
        }
    }

    private void ApplyNavigationValue(float value)
    {
        if (navigationTargets == null || navigationTargets.Length == 0)
        {
            bridge?.ClearHudFocus();
            return;
        }

        var clamped = Mathf.Clamp01(value);
        var scaled = clamped * navigationTargets.Length;
        var targetIndex = Mathf.Clamp(Mathf.FloorToInt(scaled >= navigationTargets.Length ? navigationTargets.Length - 1 : scaled), 0, navigationTargets.Length - 1);
        SetSelectedIndex(targetIndex);
    }

    private void EnsureInitialSelection()
    {
        if (navigationTargets == null || navigationTargets.Length == 0)
        {
            bridge?.ClearHudFocus();
            return;
        }

        if (!autoSelectFirst && currentSelectionIndex >= 0 && currentSelectionIndex < navigationTargets.Length)
        {
            SetSelectedIndex(currentSelectionIndex);
            return;
        }

        if (!autoSelectFirst && currentSelectionIndex < 0)
        {
            return;
        }

        SetSelectedIndex(Mathf.Max(0, currentSelectionIndex));
    }

    private void SetSelectedIndex(int index)
    {
        if (navigationTargets == null || navigationTargets.Length == 0)
        {
            bridge?.ClearHudFocus();
            return;
        }

        var clamped = Mathf.Clamp(index, 0, navigationTargets.Length - 1);
        var changed = clamped != currentSelectionIndex;
        currentSelectionIndex = clamped;
        SendFocusForCurrentSelection();

        var selectable = navigationTargets[currentSelectionIndex];
        if (selectable == null)
        {
            return;
        }

        if (changed)
        {
            selectable.Select();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            }
        }
    }

    public void ActivateCurrentSelection()
    {
        AttachBridgeIfNeeded();

        if (navigationTargets == null || navigationTargets.Length == 0)
        {
            bridge?.StartGameFromMenuButton();
            return;
        }

        if (currentSelectionIndex < 0 || currentSelectionIndex >= navigationTargets.Length)
        {
            currentSelectionIndex = Mathf.Clamp(currentSelectionIndex, 0, navigationTargets.Length - 1);
        }

        var current = navigationTargets[currentSelectionIndex];
        if (current == null)
        {
            return;
        }

        if (EventSystem.current != null)
        {
            ExecuteEvents.Execute(current.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            return;
        }

        if (current is Button directButton)
        {
            directButton.onClick.Invoke();
            return;
        }

        var fallbackButton = current.GetComponent<Button>();
        fallbackButton?.onClick.Invoke();
    }

    private void SendFocusForCurrentSelection()
    {
        if (bridge == null)
        {
            return;
        }

        if (currentSelectionIndex < 0 || navigationTargets == null || navigationTargets.Length == 0)
        {
            bridge.ClearHudFocus();
            return;
        }

        RectTransform target = null;
        if (focusTargets != null && currentSelectionIndex < focusTargets.Length)
        {
            target = focusTargets[currentSelectionIndex];
        }

        if (target == null)
        {
            var selectable = navigationTargets[currentSelectionIndex];
            target = selectable != null ? selectable.transform as RectTransform : null;
        }

        bridge.SendHudFocus(target);
    }
}