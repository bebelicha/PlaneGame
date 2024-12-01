using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private Canvas menuInicial;
    [SerializeField]
    private Button botaoIniciar;
    [SerializeField]
    private Button botaoConfiguracoes;
    [SerializeField]
    private Button voltarMenu;
    [SerializeField]
    private Button voltarMenu2;
    [SerializeField]
    private Canvas menuConfiguracoes;
    [SerializeField]
    private Canvas menuGameOver;
    [SerializeField]
    private Slider sliderGravidade;
    [SerializeField]
    private Slider sliderVolume;
    [SerializeField]
    private VariavelCompartilhadaFloat gravidadeAviao;
    [SerializeField]
    private Diretor diretor;
    [SerializeField]
    private AudioSource trilhaSonora;

    private void Start()
    {
        PausarJogo();
        botaoIniciar.onClick.AddListener(IniciarJogo);
        botaoConfiguracoes.onClick.AddListener(AbrirConfiguracoes);
        voltarMenu.onClick.AddListener(VoltarMenuInicial);
        voltarMenu2.onClick.AddListener(VoltarMenuInicial);
        sliderGravidade.onValueChanged.AddListener(AlterarGravidade);
        sliderVolume.onValueChanged.AddListener(AlterarVolume);
    }

private void IniciarJogo()
{
    menuInicial.gameObject.SetActive(false);
    menuConfiguracoes.gameObject.SetActive(false);
    menuGameOver.gameObject.SetActive(false);
    diretor.ReiniciarJogo();
    RetomarJogo();
}

private void AbrirConfiguracoes()
{
    PausarJogo();
    menuInicial.gameObject.SetActive(false);
    menuConfiguracoes.gameObject.SetActive(true);
   
}

private void VoltarMenuInicial()
{
    PausarJogo();
    menuInicial.gameObject.SetActive(true);
    menuConfiguracoes.gameObject.SetActive(false);
    
}

private void AlterarGravidade(float valor)
{
    gravidadeAviao.valor = valor;
}

private void AlterarVolume(float valor)
{
    trilhaSonora.volume = valor;
}
private void PausarJogo()
{
    Time.timeScale = 0;
}

private void RetomarJogo()
{
    Time.timeScale = 1;
}
}