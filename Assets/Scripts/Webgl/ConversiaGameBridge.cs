using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConversiaGameBridge : MonoBehaviour
{
    public static ConversiaGameBridge Instance { get; private set; }

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

    private Pontuacao pontuacao;
    private Menu menu;
    private Diretor diretor;
    private ControleDeDificuldade controleDeDificuldade;

    private float sessionStartTime;
    private int comboCount;
    private int lastScore;
    private readonly Dictionary<string, string> currentPreferences = new Dictionary<string, string>();
    private string primaryTriggerId = string.Empty;

    private readonly StringBuilder sharedBuilder = new StringBuilder(256);

    private const string DifficultyKey = "difficulty";

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
        LocateReferences();

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

    private void InvokeMenuPrimaryAction()
    {
        LocateReferences();

        if (menu == null)
        {
            menu = FindObjectOfType<Menu>();
        }

        if (menu == null)
        {
            LogStatus("Menu nao encontrado para acao primaria");
            return;
        }

        var estavaRodando = menu.JogoRodando;
        menu.OnPressionarTecla();

        if (!estavaRodando && menu.JogoRodando)
        {
            ReportGameRestarted();
            LogStatus("Jogo iniciado via acao primaria");
        }
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

        LocateReferences();

        var action = (message.action ?? message.reason ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(action))
        {
            // Some Conversia builds omit the action text but still expect the default button behaviour.
            InvokeMenuPrimaryAction();
            return;
        }

        switch (action)
        {
            case "start":
            case "start-game":
            case "iniciar":
            case "play":
            case "activate":
            case "activate-current":
            case "confirm":
            case "submit":
                InvokeMenuPrimaryAction();
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
            default:
                LocateReferences();

                if (menu != null)
                {
                    var menuVisivel = !menu.JogoRodando;
                    if (!menuVisivel && menu.menuGameOver != null)
                    {
                        menuVisivel = menu.menuGameOver.gameObject.activeInHierarchy;
                    }

                    if (menuVisivel)
                    {
                        InvokeMenuPrimaryAction();
                    }
                }
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



