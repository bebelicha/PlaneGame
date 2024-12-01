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
    private Slider sliderAmortecimentoAngular;
    [SerializeField]
    private Slider sliderGravidade;
    [SerializeField]
    private Slider sliderVolume;
    [SerializeField]
    private Slider sliderGravityScale; 
    [SerializeField]
    private Slider sliderTempoParaDificuldadeMaxima; 
    [SerializeField]
    private VariavelCompartilhadaFloat gravidadeAviao;
    [SerializeField]
    private Diretor diretor;
    [SerializeField]
    private AudioSource trilhaSonora;
    [SerializeField]
    private Aviao aviao; 
    [SerializeField]
    private ControleDeDificuldade controleDeDificuldade; 

    private void Start()
    {
        PausarJogo();
        botaoIniciar.onClick.AddListener(IniciarJogo);
        botaoConfiguracoes.onClick.AddListener(AbrirConfiguracoes);
        voltarMenu.onClick.AddListener(VoltarMenuInicial);
        voltarMenu2.onClick.AddListener(VoltarMenuInicial);
        sliderGravidade.onValueChanged.AddListener(AlterarGravidade);
        sliderVolume.onValueChanged.AddListener(AlterarVolume);
        sliderGravityScale.onValueChanged.AddListener(AlterarGravityScale); 
        sliderAmortecimentoAngular.onValueChanged.AddListener(AlterarAmortecimentoAngular);
        sliderTempoParaDificuldadeMaxima.onValueChanged.AddListener(AlterarTempoParaDificuldadeMaxima); 
         CarregarConfiguracoes();
    }
    private void CarregarConfiguracoes()
    {
        if (PlayerPrefs.HasKey("gravidade"))
        {
            gravidadeAviao.valor = PlayerPrefs.GetFloat("gravidade");
            sliderGravidade.value = gravidadeAviao.valor;
        }
        if (PlayerPrefs.HasKey("amortecimentoAngular"))
        {
            aviao.AlterarAngularDamping(PlayerPrefs.GetFloat("amortecimentoAngular"));
            sliderAmortecimentoAngular.value = PlayerPrefs.GetFloat("amortecimentoAngular");
        }
        if (PlayerPrefs.HasKey("volume"))
        {
            trilhaSonora.volume = PlayerPrefs.GetFloat("volume");
            sliderVolume.value = trilhaSonora.volume;
        }
        if (PlayerPrefs.HasKey("gravityScale"))
        {
            aviao.AlterarGravityScale(PlayerPrefs.GetFloat("gravityScale"));
            sliderGravityScale.value = PlayerPrefs.GetFloat("gravityScale");
        }
        if (PlayerPrefs.HasKey("tempoParaDificuldadeMaxima"))
        {
            controleDeDificuldade.DefinirTempoParaDificuldadeMaxima(PlayerPrefs.GetFloat("tempoParaDificuldadeMaxima"));
            sliderTempoParaDificuldadeMaxima.value = PlayerPrefs.GetFloat("tempoParaDificuldadeMaxima");
        }
    }
    public void IniciarJogo()
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
        PlayerPrefs.SetFloat("gravidade", valor);
    }

    private void AlterarAmortecimentoAngular(float valor)
    {
        aviao.AlterarAngularDamping(valor);
        PlayerPrefs.SetFloat("amortecimentoAngular", valor);
    }

    private void AlterarVolume(float valor)
    {
        trilhaSonora.volume = valor;
        PlayerPrefs.SetFloat("volume", valor);
    }

    private void AlterarGravityScale(float valor)
    {
        aviao.AlterarGravityScale(valor);
        PlayerPrefs.SetFloat("gravityScale", valor);
    }


    private void AlterarTempoParaDificuldadeMaxima(float valor)
    {
        controleDeDificuldade.DefinirTempoParaDificuldadeMaxima(valor);
        PlayerPrefs.SetFloat("tempoParaDificuldadeMaxima", valor);
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