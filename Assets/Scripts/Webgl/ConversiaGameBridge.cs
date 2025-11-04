    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class ConversiaGameBridge : MonoBehaviour
    {
        public static ConversiaGameBridge Instance { get; private set; }

        public void SendHudFocus(RectTransform target, string targetId = null, string targetLabel = null)
    {
        if (!TryComputeHudRect(target, out var snapshot))
        {
            SendHudFocusSnapshot(default, false, string.Empty, string.Empty);
            return;
        }

        var resolvedId = string.IsNullOrEmpty(targetId) ? (target != null ? target.gameObject.name : string.Empty) : targetId;
        var resolvedLabel = string.IsNullOrEmpty(targetLabel) && target != null ? target.gameObject.name : targetLabel;
        SendHudFocusSnapshot(snapshot, true, resolvedId, resolvedLabel);
    }

    public void ClearHudFocus()
    {
        SendHudFocusSnapshot(default, false, string.Empty, string.Empty);
    }

    private void SendHudFocusSnapshot(HudRectSnapshot snapshot, bool hasRect, string targetId, string targetLabel)
    {
        var normalizedRect = hasRect
            ? new Vector4(snapshot.normalizedX, snapshot.normalizedY, snapshot.normalizedWidth, snapshot.normalizedHeight)
            : new Vector4(-1f, -1f, -1f, -1f);

        var normalizedTargetId = targetId ?? string.Empty;

        if (!HasHudFocusChanged(normalizedRect, normalizedTargetId, hasRect))
        {
            return;
        }

        if (!hasRect)
        {
            lastHudFocusRect = new Vector4(-1f, -1f, -1f, -1f);
            lastHudFocusTargetId = string.Empty;
            var clearMessage = new HudFocusMessage
            {
                x = 0f,
                y = 0f,
                width = 0f,
                height = 0f,
                pixelX = 0f,
                pixelY = 0f,
                pixelWidth = 0f,
                pixelHeight = 0f,
                referenceWidth = 0f,
                referenceHeight = 0f,
                normalized = true,
                timestamp = CurrentTimestamp(),
            };
            PostJson(clearMessage);
            return;
        }

        lastHudFocusRect = normalizedRect;
        lastHudFocusTargetId = normalizedTargetId;

        var message = new HudFocusMessage
        {
            x = snapshot.normalizedX,
            y = snapshot.normalizedY,
            width = snapshot.normalizedWidth,
            height = snapshot.normalizedHeight,
            pixelX = snapshot.pixelX,
            pixelY = snapshot.pixelY,
            pixelWidth = snapshot.pixelWidth,
            pixelHeight = snapshot.pixelHeight,
            referenceWidth = snapshot.referenceWidth,
            referenceHeight = snapshot.referenceHeight,
            normalized = true,
            targetId = string.IsNullOrEmpty(normalizedTargetId) ? null : normalizedTargetId,
            targetLabel = string.IsNullOrEmpty(targetLabel) ? null : targetLabel,
            timestamp = CurrentTimestamp(),
        };
        PostJson(message);
    }

    private bool TryComputeHudRect(RectTransform target, out HudRectSnapshot snapshot)
    {
        snapshot = default;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return false;
        }

        var screenWidth = (float)Screen.width;
        var screenHeight = (float)Screen.height;
        if (screenWidth <= 0f || screenHeight <= 0f)
        {
            return false;
        }

        target.GetWorldCorners(hudFocusCorners);
        var camera = ResolveCanvasCamera(target);
        var bottomLeft = RectTransformUtility.WorldToScreenPoint(camera, hudFocusCorners[0]);
        var topRight = RectTransformUtility.WorldToScreenPoint(camera, hudFocusCorners[2]);

        var pixelWidth = topRight.x - bottomLeft.x;
        var pixelHeight = topRight.y - bottomLeft.y;
        if (pixelWidth <= 0f || pixelHeight <= 0f)
        {
            return false;
        }

        var clampedPixelWidth = Mathf.Max(0f, pixelWidth);
        var clampedPixelHeight = Mathf.Max(0f, pixelHeight);
        var clampedPixelX = Mathf.Max(0f, bottomLeft.x);
        var pixelTop = Mathf.Max(0f, topRight.y);
        var clampedPixelY = Mathf.Max(0f, screenHeight - pixelTop);

        var normalizedWidth = clampedPixelWidth / screenWidth;
        var normalizedHeight = clampedPixelHeight / screenHeight;
        var normalizedX = clampedPixelX / screenWidth;
        var normalizedY = clampedPixelY / screenHeight;

        snapshot = new HudRectSnapshot
        {
            normalizedX = Mathf.Clamp01(normalizedX),
            normalizedY = Mathf.Clamp01(normalizedY),
            normalizedWidth = Mathf.Clamp01(normalizedWidth),
            normalizedHeight = Mathf.Clamp01(normalizedHeight),
            pixelX = clampedPixelX,
            pixelY = clampedPixelY,
            pixelWidth = clampedPixelWidth,
            pixelHeight = clampedPixelHeight,
            referenceWidth = screenWidth,
            referenceHeight = screenHeight,
        };

        return snapshot.normalizedWidth > 0f && snapshot.normalizedHeight > 0f;
    }

    private bool HasHudFocusChanged(Vector4 rect, string targetId, bool hasRect)
    {
        if (!hasRect)
        {
            return lastHudFocusRect.w >= 0f || !string.IsNullOrEmpty(lastHudFocusTargetId);
        }

        if (lastHudFocusRect.w < 0f)
        {
            return true;
        }

        if (!string.Equals(lastHudFocusTargetId, targetId, StringComparison.Ordinal))
        {
            return true;
        }

        if (Mathf.Abs(lastHudFocusRect.x - rect.x) > HudFocusEpsilon)
        {
            return true;
        }

        if (Mathf.Abs(lastHudFocusRect.y - rect.y) > HudFocusEpsilon)
        {
            return true;
        }

        if (Mathf.Abs(lastHudFocusRect.z - rect.z) > HudFocusEpsilon)
        {
            return true;
        }

        if (Mathf.Abs(lastHudFocusRect.w - rect.w) > HudFocusEpsilon)
        {
            return true;
        }

        return false;
    }

    public bool TryBuildHudLayoutEntry(RectTransform target, string targetId, string targetLabel, out HudLayoutEntry entry, out float referenceWidth, out float referenceHeight)
    {
        entry = null;
        referenceWidth = 0f;
        referenceHeight = 0f;

        if (!TryComputeHudRect(target, out var snapshot))
        {
            return false;
        }

        referenceWidth = snapshot.referenceWidth;
        referenceHeight = snapshot.referenceHeight;

        entry = new HudLayoutEntry
        {
            id = string.IsNullOrEmpty(targetId) ? (target != null ? target.gameObject.name : string.Empty) : targetId,
            label = string.IsNullOrEmpty(targetLabel) ? null : targetLabel,
            x = snapshot.normalizedX,
            y = snapshot.normalizedY,
            width = snapshot.normalizedWidth,
            height = snapshot.normalizedHeight,
            pixelX = snapshot.pixelX,
            pixelY = snapshot.pixelY,
            pixelWidth = snapshot.pixelWidth,
            pixelHeight = snapshot.pixelHeight,
        };

        return true;
    }

    public void SendHudLayoutSnapshot(string groupId, IList<HudLayoutEntry> entries, float referenceWidth, float referenceHeight)
    {
        if (entries == null || entries.Count == 0)
        {
            return;
        }

        var payload = new HudLayoutEntry[entries.Count];
        for (int i = 0; i < entries.Count; i++)
        {
            payload[i] = entries[i];
        }

        var message = new HudLayoutMessage
        {
            groupId = string.IsNullOrEmpty(groupId) ? null : groupId,
            referenceWidth = referenceWidth,
            referenceHeight = referenceHeight,
            targets = payload,
            timestamp = CurrentTimestamp(),
        };
        PostJson(message);
    }
    [Serializable]
    private class GameReadyMessage
    {
        public string source = "game";
        public string type = "ready";
        public long timestamp;
    }

    [Serializable]
    private class GameStatsMessage
    {
        public string source = "game";
        public string type = "stats";
        public int score;
        public float elapsedTime;
        public string level;
        public int combos;
        public long timestamp;
    }

    [Serializable]
    private class GamePreferencesMessage
    {
        public string source = "game";
        public string type = "preferencesUpdate";
        public string context = "update";
        public PreferenceEntry[] entries;
        public long timestamp;
    }

    [Serializable]
    private class GameRequestMessage
    {
        public string source = "game";
        public string type = "preferencesRequest";
        public long timestamp;
    }

    [Serializable]
    private class HudFocusMessage
    {
        public string source = "game";
        public string type = "hudFocus";
        public float x;
        public float y;
        public float width;
        public float height;
        public float pixelX;
        public float pixelY;
        public float pixelWidth;
        public float pixelHeight;
        public float referenceWidth;
        public float referenceHeight;
        public bool normalized = true;
        public string targetId;
        public string targetLabel;
        public long timestamp;
    }

    [Serializable]
    private class HudLayoutMessage
    {
        public string source = "game";
        public string type = "hudLayout";
        public string groupId;
        public float referenceWidth;
        public float referenceHeight;
        public HudLayoutEntry[] targets;
        public long timestamp;
    }

    [Serializable]
    public class HudLayoutEntry
    {
        public string id;
        public string label;
        public float x;
        public float y;
        public float width;
        public float height;
        public float pixelX;
        public float pixelY;
        public float pixelWidth;
        public float pixelHeight;
    }

    [Serializable]
    public class PreferenceEntry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class PreferencesEnvelope
    {
        public string source;
        public string type;
        public string context;
        public PreferenceEntry[] entries;
        public long timestamp;
    }

    [Serializable]
    public class TriggerMessage
    {
        public string source;
        public string type;
        public string id;
        public string name;
        public string label;
        public string kind;
        public string state;
        public float level;
        public bool primary;
        public long timestamp;
    }

    [Serializable]
    public class AnalogMessage
    {
        public string source;
        public string type;
        public AnalogChannel[] channels;
        public long timestamp;
    }

    [Serializable]
    public class AnalogChannel
    {
        public string id;
        public string name;
        public string label;
        public float value;
    }

    [Serializable]
    public class CommandMessage
    {
        public string source;
        public string type;
        public string command;
        public string action;
        public string reason;
        public long timestamp;
    }

    [Serializable]
    public class InitMessage
    {
        public string source;
        public string type;
        public string version;
        public InitAnalogChannel[] analogChannels;
        public InitTrigger[] triggers;
        public PreferencesEnvelope preferences;
    }

    [Serializable]
    public class InitAnalogChannel
    {
        public string id;
        public string name;
        public string label;
        public string sourceMovement;
    }

    [Serializable]
    public class InitTrigger
    {
        public string id;
        public string name;
        public string label;
        public bool primary;
    }

    private struct HudRectSnapshot
    {
        public float normalizedX;
        public float normalizedY;
        public float normalizedWidth;
        public float normalizedHeight;
        public float pixelX;
        public float pixelY;
        public float pixelWidth;
        public float pixelHeight;
        public float referenceWidth;
        public float referenceHeight;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ConversiaBridge_PostMessage(string message);
#else
    private static void ConversiaBridge_PostMessage(string message)
    {
        Debug.Log($"[ConversiaBridge] {message}");
    }
#endif

    public event Action<TriggerMessage> TriggerReceived;
    public event Action<AnalogMessage> AnalogReceived;
    public event Action<PreferencesEnvelope> PreferencesReceived;
    public event Action<CommandMessage> CommandReceived;
    public event Action<InitMessage> InitReceived;
    public event Action<string> DifficultyChanged;
    public event Action<string> NavigationAnalogChanged;
    public event Action<float> NavigationLevelChanged;

    private Pontuacao pontuacao;
    private Menu menu;
    private Diretor diretor;
    private ControleDeDificuldade controleDeDificuldade;

    private float sessionStartTime;
    private int comboCount;
    private int lastScore;
    private readonly Dictionary<string, string> currentPreferences = new Dictionary<string, string>();
    private string primaryTriggerId = string.Empty;
    private readonly Dictionary<string, string> analogMovementMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private string currentNavigationMovement = string.Empty;
    private readonly Vector3[] hudFocusCorners = new Vector3[4];
    private Vector4 lastHudFocusRect = new Vector4(-1f, -1f, -1f, -1f);
    private string lastHudFocusTargetId = string.Empty;

    private readonly StringBuilder sharedBuilder = new StringBuilder(256);

    private const string DifficultyKey = "difficulty";
    private const string NavigationAnalogKey = "hudNavigationAnalog";
    private const float HudFocusEpsilon = 0.002f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        var go = new GameObject("ConversiaGameBridge");
        Instance = go.AddComponent<ConversiaGameBridge>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.name = "ConversiaGameBridge";
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        sessionStartTime = Time.time;
        LocateReferences();
        LogStatus("Aguardando Conversia...");
        SendReadyMessage();
        RequestPreferences();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LocateReferences();
    }

    public void OnConversiaInit(string json)
    {
        var message = Deserialize<InitMessage>(json);
        if (message == null)
        {
            return;
        }

        InitReceived?.Invoke(message);

        analogMovementMap.Clear();
        if (message.analogChannels != null)
        {
            foreach (var analog in message.analogChannels)
            {
                if (analog == null || string.IsNullOrEmpty(analog.name))
                {
                    continue;
                }

                analogMovementMap[analog.name] = analog.sourceMovement ?? string.Empty;
            }
        }
        UpdateNavigationMovementFromAnalog();

        if (message.triggers != null)
        {
            foreach (var trigger in message.triggers)
            {
                if (trigger != null && trigger.primary)
                {
                    primaryTriggerId = trigger.id;
                    break;
                }
            }
        }

        if (message.preferences?.entries != null)
        {
            ApplyPreferences(message.preferences.entries, message.preferences.context ?? "init");
        }

        LogStatus($"Config recebida ({message.version})");
    }

    public void OnConversiaTrigger(string json)
    {
        var message = Deserialize<TriggerMessage>(json);
        if (message == null)
        {
            return;
        }

        ProcessNavigationTrigger(message);
        TriggerReceived?.Invoke(message);
        LogTrigger(message);
    }

    public void OnConversiaAnalog(string json)
    {
        var message = Deserialize<AnalogMessage>(json);
        if (message == null)
        {
            return;
        }

        AnalogReceived?.Invoke(message);
        ProcessNavigationAnalogMessage(message);
        LogAnalog(message);
    }

    public void OnConversiaPreferences(string json)
    {
        var message = Deserialize<PreferencesEnvelope>(json);
        if (message == null)
        {
            return;
        }

        PreferencesReceived?.Invoke(message);
        ApplyPreferences(message.entries, message.context ?? "sync");
    }

    public void OnConversiaCommand(string json)
    {
        var message = Deserialize<CommandMessage>(json);
        if (message == null)
        {
            return;
        }

        CommandReceived?.Invoke(message);

        var command = message.command ?? string.Empty;
        switch (command.ToLowerInvariant())
        {
            case "exit":
                HandleExitCommand(message.reason);
                break;
            case "hudaction":
                HandleActionCommand(message);
                break;
        }
    }

    public string PrimaryTriggerId => primaryTriggerId;

    public string CurrentDifficulty
    {
        get
        {
            if (currentPreferences.TryGetValue(DifficultyKey, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            return "normal";
        }
    }

    public string CurrentNavigationAnalog { get; private set; } = string.Empty;

    public void ReportComboActivated(string comboName)
    {
        comboCount++;
        LogStats();
        SendStats();
    }

    public void ReportScore(int score)
    {
        lastScore = score;
        LogStats();
        SendStats();
    }

    public void ReportGameRestarted()
    {
        sessionStartTime = Time.time;
        comboCount = 0;
        LogStats();
        SendStats();
    }

    public void ReportGameOver()
    {
        LogStatus("Game Over registrado");
        SendStats();
    }

    public void RequestPreferences()
    {
        var message = new GameRequestMessage
        {
            timestamp = CurrentTimestamp(),
        };
        PostJson(message);
    }

    public void SendPreferencesUpdate()
    {
        var entries = BuildPreferenceEntries(currentPreferences);
        var message = new GamePreferencesMessage
        {
            entries = entries,
            timestamp = CurrentTimestamp(),
        };
        PostJson(message);
    }

    private void LocateReferences()
    {
        pontuacao = FindObjectOfType<Pontuacao>();
        menu = FindObjectOfType<Menu>();
        diretor = FindObjectOfType<Diretor>();
        controleDeDificuldade = FindObjectOfType<ControleDeDificuldade>();

        if (pontuacao != null)
        {
            pontuacao.AdicionarListenerPontuacao(OnPontuacaoAtualizada);
        }
    }

    private void OnPontuacaoAtualizada()
    {
        if (pontuacao != null)
        {
            ReportScore(pontuacao.Pontos);
        }
    }

    private void HandleExitCommand(string reason)
    {
        if (menu != null)
        {
            menu.VoltarMenuInicial();
        }

    var message = string.IsNullOrEmpty(reason) ? "Saida solicitada" : $"Saida: {reason}";
        LogStatus(message);
    }

    private void ApplyPreferences(PreferenceEntry[] entries, string context)
    {
        if (entries == null)
        {
            return;
        }

        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.key))
            {
                continue;
            }

            currentPreferences[entry.key] = entry.value ?? string.Empty;
            ApplyPreferenceEffect(entry.key, entry.value);
        }

        LogPreferences();
        LogStatus($"Preferencias atualizadas ({context})");
    }
    private void ApplyPreferenceEffect(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        switch (key)
        {
            case DifficultyKey:
                ApplyDifficultyPreference(value);
                break;
            case "preferredTrigger":
                if (!string.IsNullOrEmpty(value))
                {
                    primaryTriggerId = value;
                }
                break;
            case NavigationAnalogKey:
                ApplyNavigationAnalogPreference(value);
                break;
            default:
                break;
        }
    }

    private void ApplyDifficultyPreference(string value)
    {
        if (controleDeDificuldade == null)
        {
            controleDeDificuldade = FindObjectOfType<ControleDeDificuldade>();
        }

        if (controleDeDificuldade == null)
        {
            LogStatus("Nao foi possivel aplicar a dificuldade: controlador ausente");
            DifficultyChanged?.Invoke(CurrentDifficulty);
            return;
        }

        float tempo;
        switch (value)
        {
            case "easy":
                tempo = 120f;
                break;
            case "hard":
                tempo = 60f;
                break;
            default:
                tempo = 90f;
                break;
        }

        controleDeDificuldade.DefinirTempoParaDificuldadeMaxima(tempo);
        LogStatus($"Dificuldade aplicada: {value}");
        DifficultyChanged?.Invoke(CurrentDifficulty);
    }

    private void ApplyNavigationAnalogPreference(string value)
    {
        var normalized = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        if (string.Equals(CurrentNavigationAnalog, normalized, StringComparison.Ordinal))
        {
            return;
        }

        CurrentNavigationAnalog = normalized;
        if (!string.IsNullOrEmpty(CurrentNavigationAnalog))
        {
            LogStatus($"Canal de navegacao configurado: {CurrentNavigationAnalog}");
        }

        UpdateNavigationMovementFromAnalog();
        NavigationAnalogChanged?.Invoke(CurrentNavigationAnalog);
    }
    
    private void UpdateNavigationMovementFromAnalog()
    {
        if (string.IsNullOrEmpty(CurrentNavigationAnalog))
        {
            currentNavigationMovement = string.Empty;
            return;
        }

        if (analogMovementMap.TryGetValue(CurrentNavigationAnalog, out var movement) && !string.IsNullOrEmpty(movement))
        {
            currentNavigationMovement = movement;
            return;
        }

        currentNavigationMovement = CurrentNavigationAnalog;
    }

    private void ProcessNavigationAnalogMessage(AnalogMessage message)
    {
        if (message?.channels == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(CurrentNavigationAnalog))
        {
            return;
        }

        foreach (var channel in message.channels)
        {
            if (channel == null)
            {
                continue;
            }

            if (string.Equals(channel.name, CurrentNavigationAnalog, StringComparison.OrdinalIgnoreCase))
            {
                NavigationLevelChanged?.Invoke(Mathf.Clamp01(channel.value));
                break;
            }
        }
    }

    private void ProcessNavigationTrigger(TriggerMessage message)
    {
        if (message == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(currentNavigationMovement))
        {
            return;
        }

        if (!string.Equals(message.kind, "movement", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(message.name, currentNavigationMovement, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(message.state, "update", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(message.state, "start", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        NavigationLevelChanged?.Invoke(Mathf.Clamp01(message.level));
    }

    private Camera ResolveCanvasCamera(RectTransform rect)
    {
        if (rect == null)
        {
            return Camera.main;
        }

        var canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return Camera.main;
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera ?? Camera.main;
    }

    private void SendReadyMessage()
    {
        var message = new GameReadyMessage
        {
            timestamp = CurrentTimestamp(),
        };
        PostJson(message);
    }

    private void SendStats()
    {
        var message = new GameStatsMessage
        {
            score = lastScore,
            elapsedTime = Mathf.Max(0f, Time.time - sessionStartTime),
            level = GetGamePhaseLabel(),
            combos = comboCount,
            timestamp = CurrentTimestamp(),
        };
        PostJson(message);
    }

    private void StartGameInternal()
    {
        if (menu == null)
        {
            menu = FindObjectOfType<Menu>();
        }

        if (menu == null)
        {
        LogStatus("Menu nao encontrado para iniciar o jogo");
            return;
        }

        if (menu.JogoRodando)
        {
            menu.VoltarMenuInicial();
        }

        menu.IniciarJogo();
        ReportGameRestarted();
    LogStatus("Jogo iniciado a partir do comando");
    }

    private void ToggleDifficulty()
    {
        currentPreferences.TryGetValue(DifficultyKey, out var currentValue);
        var nextValue = NextDifficultyValue(currentValue);
        currentPreferences[DifficultyKey] = nextValue;
        ApplyDifficultyPreference(nextValue);
        LogPreferences();
        SendPreferencesUpdate();
    }

    private static string NextDifficultyValue(string current)
    {
        switch (current)
        {
            case "easy":
                return "normal";
            case "normal":
                return "hard";
            default:
                return "easy";
        }
    }

    private void LogTrigger(TriggerMessage message)
    {
        if (message == null)
        {
            return;
        }

        if (string.Equals(message.kind, "combo", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[ConversiaBridge] Combo {message.label ?? message.name}: {message.state}");
        }
    }

    private void LogAnalog(AnalogMessage message)
    {
        if (message?.channels == null)
        {
            return;
        }

        sharedBuilder.Length = 0;
        foreach (var channel in message.channels)
        {
            if (channel == null)
            {
                continue;
            }

            sharedBuilder.Append(channel.label ?? channel.name);
            sharedBuilder.Append(':');
            sharedBuilder.Append(' ');
            sharedBuilder.Append(channel.value.ToString("0.00"));
            sharedBuilder.Append(" | ");
        }

        if (sharedBuilder.Length > 3)
        {
            sharedBuilder.Length -= 3;
        }

            Debug.Log($"[ConversiaBridge] Analogicos -> {sharedBuilder}");
    }

    private void LogStats()
    {
        var elapsed = Mathf.Max(0f, Time.time - sessionStartTime);
          Debug.Log($"[ConversiaBridge] Pontuacao: {lastScore}, Combos: {comboCount}, Tempo: {elapsed:0.0}s");
    }

    private void LogPreferences()
    {
        sharedBuilder.Length = 0;
        foreach (var kv in currentPreferences)
        {
            sharedBuilder.Append(kv.Key);
            sharedBuilder.Append('=');
            sharedBuilder.Append(string.IsNullOrEmpty(kv.Value) ? "(vazio)" : kv.Value);
            sharedBuilder.Append(';');
            sharedBuilder.Append(' ');
        }

            Debug.Log($"[ConversiaBridge] Preferencias -> {sharedBuilder}");
    }

    private void LogStatus(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        Debug.Log($"[ConversiaBridge] {message}");
    }

    private string GetGamePhaseLabel()
    {
        if (menu == null)
        {
            return "desconhecido";
        }

        return menu.JogoRodando ? "gameplay" : "menu";
    }

    private void HandleActionCommand(CommandMessage message)
    {
        if (message == null)
        {
            return;
        }

        var action = (message.action ?? message.reason ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(action))
        {
            return;
        }

        switch (action)
        {
            case "start":
            case "start-game":
            case "iniciar":
                StartGameInternal();
                break;
            case "difficulty":
            case "toggle-difficulty":
            case "change-difficulty":
            case "alterar-dificuldade":
                ToggleDifficulty();
                break;
            case "exit":
            case "sair":
            case "quit":
                ExitGame();
                break;
        }
    }
    public void StartGameFromMenuButton()
    {
        StartGameInternal();
    }

    public void ToggleDifficultyFromMenu()
    {
        ToggleDifficulty();
    }

    public void ExitGame()
    {
        var message = new CommandMessage
        {
            source = "game",
            type = "command",
            command = "exit",
            reason = "menu-request",
            timestamp = CurrentTimestamp(),
        };
        PostJson(message);
        HandleExitCommand("menu-request");
    }

    private void PostJson(object payload)
    {
        if (payload == null)
        {
            return;
        }

        try
        {
            var json = JsonUtility.ToJson(payload);
            ConversiaBridge_PostMessage(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ConversiaBridge] Failed to send message: {ex.Message}");
        }
    }

    private PreferenceEntry[] BuildPreferenceEntries(Dictionary<string, string> source)
    {
        var list = new List<PreferenceEntry>(source.Count);
        foreach (var kv in source)
        {
            list.Add(new PreferenceEntry { key = kv.Key, value = kv.Value });
        }

        return list.ToArray();
    }

    private static long CurrentTimestamp()
    {
    return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private static T Deserialize<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ConversiaBridge] Failed to parse json for {typeof(T).Name}: {ex.Message}");
            return null;
        }
    }
}



