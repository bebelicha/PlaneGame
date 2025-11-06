using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DateReceiver : MonoBehaviour
{
    public Text textField;
    private Aviao aviao;
    public Menu menu;
    public InterfaceGameOver interfaceGameOver;

    [SerializeField]
    private UnityEvent aoPressionarTecla;

    private ConversiaGameBridge bridge;

    private void Start()
    {
        aviao = GameObject.FindObjectOfType<Aviao>();
        menu = GameObject.FindObjectOfType<Menu>();
        interfaceGameOver = GameObject.FindObjectOfType<InterfaceGameOver>();
        bridge = ConversiaGameBridge.Instance;
        if (bridge != null)
        {
            bridge.TriggerReceived += OnTriggerMessage;
            bridge.CommandReceived += OnCommandMessage;
        }

    }

    private void OnDestroy()
    {
        if (bridge != null)
        {
            bridge.TriggerReceived -= OnTriggerMessage;
            bridge.CommandReceived -= OnCommandMessage;
        }
    }

    public void Date(string jsonData)
    {
        Debug.LogWarning("[DateReceiver] Deprecated Date() call received.");
    }

    private void OnTriggerMessage(ConversiaGameBridge.TriggerMessage message)
    {
        if (message == null)
        {
            return;
        }

        if (string.Equals(message.kind, "movement", System.StringComparison.OrdinalIgnoreCase))
        {
            HandleMovementTrigger(message);
        }
        else if (string.Equals(message.kind, "combo", System.StringComparison.OrdinalIgnoreCase))
        {
            HandleComboTrigger(message);
        }
    }

    private void OnCommandMessage(ConversiaGameBridge.CommandMessage message)
    {
        if (message == null)
        {
            return;
        }
        if (string.Equals(message.command, "exit", System.StringComparison.OrdinalIgnoreCase))
        {
            if (menu != null)
            {
                menu.VoltarMenuInicial();
            }
        }
    }

    private void HandleMovementTrigger(ConversiaGameBridge.TriggerMessage message)
    {
        if (!string.Equals(message.state, "start", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!IsPrimaryTrigger(message.name))
        {
            return;
        }

        if (menu == null)
        {
            menu = GameObject.FindObjectOfType<Menu>();
        }

        if (interfaceGameOver == null)
        {
            interfaceGameOver = GameObject.FindObjectOfType<InterfaceGameOver>();
        }

        if (menu != null)
        {
            var estaNoMenuInicial = !menu.JogoRodando;

            if (interfaceGameOver != null && interfaceGameOver.gameObject.activeInHierarchy && interfaceGameOver.imagemGameOver.activeSelf)
            {
                interfaceGameOver.EsconderInterface();
                menu.VoltarMenuInicial();
                return;
            }

            if (estaNoMenuInicial)
            {
                var conversia = ConversiaGameBridge.Instance;
                if (conversia != null)
                {
                    conversia.StartGameFromMenuButton();
                }
                else
                {
                    menu.IniciarJogo();
                }
                return;
            }

            if (menu.JogoRodando)
            {
                if (aviao == null)
                {
                    aviao = GameObject.FindObjectOfType<Aviao>();
                }
                if (aviao != null)
                {
                    aviao.DarImpulso();
                }
            }
        }
    }

    private void HandleComboTrigger(ConversiaGameBridge.TriggerMessage message)
    {
        var isStart = string.Equals(message.state, "start", System.StringComparison.OrdinalIgnoreCase);
        var isEnd = string.Equals(message.state, "end", System.StringComparison.OrdinalIgnoreCase);

        if (!isStart && !isEnd)
        {
            return;
        }

        if (aviao == null)
        {
            aviao = GameObject.FindObjectOfType<Aviao>();
        }
        if (aviao == null)
        {
            return;
        }

        if (isEnd)
        {
            aviao.DesativarCombo();
            return;
        }

        aviao.AtivarCombo();
        ConversiaGameBridge.Instance?.ReportComboActivated(message.name);
    }

    private bool IsPrimaryTrigger(string movementName)
    {
        if (bridge != null)
        {
            return string.Equals(movementName, bridge.PrimaryTriggerId, System.StringComparison.Ordinal);
        }
        return string.Equals(movementName, "findMouthOpen", System.StringComparison.OrdinalIgnoreCase);
    }
}