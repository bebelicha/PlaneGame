using UnityEngine;

public class Diretor : MonoBehaviour
{
    private Aviao aviao;
    private Pontuacao pontuacao;
    public InterfaceGameOver interfaceGameOver;
    private Menu menu;
    private ConversiaGameBridge conversiaBridge;

    private void Start()
    {
        aviao = GameObject.FindObjectOfType<Aviao>();
        pontuacao = GameObject.FindObjectOfType<Pontuacao>();
        interfaceGameOver = GameObject.FindObjectOfType<InterfaceGameOver>();
        menu = GameObject.FindObjectOfType<Menu>();
        conversiaBridge = ConversiaGameBridge.Instance;
    }

    public void IniciarJogo()
    {
        if (conversiaBridge == null)
        {
            conversiaBridge = ConversiaGameBridge.Instance;
            if (conversiaBridge == null)
            {
                conversiaBridge = GameObject.FindObjectOfType<ConversiaGameBridge>();
            }
        }

        if (conversiaBridge != null)
        {
            conversiaBridge.StartGameFromMenuButton();
            return;
        }

        if (menu == null)
        {
            menu = GameObject.FindObjectOfType<Menu>();
        }

        if (menu != null)
        {
            menu.IniciarJogo();
        }
        else
        {
            ReiniciarJogo();
        }
    }

    public void FinalizarJogo()
    {
        Time.timeScale = 0;

        if (pontuacao != null)
        {
            pontuacao.SalvarRecorde();
        }
        if (interfaceGameOver != null)
        {
            interfaceGameOver.MostrarInterface();
        }
        if (menu == null)
        {
            menu = GameObject.FindObjectOfType<Menu>();
        }
        menu?.MostrarGameOver();
        conversiaBridge?.ReportGameOver();
    }

    public void ReiniciarJogo()
    {
        if (interfaceGameOver != null)
        {
            interfaceGameOver.EsconderInterface();
        }
        Time.timeScale = 1;
        aviao?.Reiniciar();
        DestruirObstaculos();
        pontuacao?.Reiniciar();
        conversiaBridge?.ReportGameRestarted();
    }


    private void DestruirObstaculos()
    {
        var obstaculos = GameObject.FindObjectsByType<Obstaculo>(FindObjectsSortMode.None);
        foreach (var obstaculo in obstaculos)
        {
            obstaculo.Destruir();
        }
    }
}
