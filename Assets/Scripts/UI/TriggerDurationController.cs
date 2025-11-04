using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class TriggerDurationController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Slider durationSlider;

    [Header("Filtro do Conversia")]
    [SerializeField] private string movementName = "findMouthOpen";

    [Header("Eventos")]
    [SerializeField] private UnityEvent onStart;
    [SerializeField] private UnityEvent onEnd;

    private bool isActive;
    private float currentLevel;

    private void Awake()
    {
        if (durationSlider == null)
        {
            Debug.LogError("[TriggerDurationController] Slider não atribuído.");
        }
    }

    public void ResetState()
    {
        isActive = false;
        currentLevel = 0f;
        UpdateSlider(0f);
    }

    public void SetLevel(float level)
    {
        var clamped = Mathf.Clamp01(level);
        currentLevel = clamped;
        UpdateSlider(clamped);
    }

    public void SetMovementName(string movementId)
    {
        if (!string.IsNullOrEmpty(movementId))
        {
            movementName = movementId;
        }
    }

    public void HandleTrigger(ConversiaGameBridge.TriggerMessage message)
    {
        if (message == null)
        {
            return;
        }

        if (!string.Equals(message.kind, "movement", StringComparison.Ordinal))
        {
            return;
        }

        if (!MatchesTarget(message.name))
        {
            return;
        }

        SetLevel(message.level);

        switch (message.state)
        {
            case "start":
                Activate();
                break;
            case "end":
                Deactivate();
                break;
        }
    }

    public void HandleTriggerPayload(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return;
        }

        TriggerPayload data;

        try
        {
            data = JsonUtility.FromJson<TriggerPayload>(payload);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[TriggerDurationController] Não foi possível interpretar payload: {ex.Message}");
            return;
        }

        if (data == null || !MatchesTarget(data))
        {
            return;
        }

        SetLevel(data.level);

        switch (data.state)
        {
            case "start":
                Activate();
                break;
            case "end":
                Deactivate();
                break;
        }
    }

    public void Activate()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;
        onStart?.Invoke();
    }

    public void Deactivate()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        onEnd?.Invoke();
    }

    private void UpdateSlider(float value)
    {
        if (durationSlider == null)
        {
            return;
        }

        durationSlider.value = value;
    }

    private bool MatchesTarget(TriggerPayload payload)
    {
        if (payload == null)
        {
            return false;
        }

        return string.Equals(payload.kind, "movement", StringComparison.Ordinal) && MatchesTarget(payload.name);
    }

    private bool MatchesTarget(string current)
    {
        if (string.IsNullOrEmpty(movementName))
        {
            return true;
        }
        return string.Equals(current, movementName, StringComparison.Ordinal);
    }

    [System.Serializable]
    private class TriggerPayload
    {
        public string kind;
        public string name;
        public string state;
        public float level;
        public long timestamp;
    }
}