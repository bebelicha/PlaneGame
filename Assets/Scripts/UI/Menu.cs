using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField]
    public RectTransform menuInicial;
    [SerializeField]
    private Button botaoIniciar;
    [SerializeField]
    private Button botaoConfiguracoes;
    [SerializeField]
    private Button voltarMenu;
    [SerializeField]
    private Button voltarMenu2;
    [SerializeField]
    private RectTransform menuConfiguracoes;
    [SerializeField]
    public Canvas menuGameOver;
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
    [SerializeField]
    private GameObject canvasJogador;
    [SerializeField]
    private Pontuacao pontuacao;

    private bool jogoRodando = false;
    public bool JogoRodando => jogoRodando;
    private bool aguardandoPrimeiroPonto;

    private void Awake()
    {
        aguardandoPrimeiroPonto = false;
        GarantirReferencias();
        OcultarCanvasJogador();
    }

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
        RegistrarPontuacao();
        if (menuGameOver != null)
        {
            menuGameOver.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!jogoRodando || !aguardandoPrimeiroPonto)
        {
            return;
        }

        VerificarCanvasJogadorPorPontuacao();
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
        else{
            trilhaSonora.volume = 0.1f;
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
        if (menuGameOver != null)
        {
            menuGameOver.gameObject.SetActive(false);
        }
        diretor.ReiniciarJogo();
        RetomarJogo();
        jogoRodando = true;
        aguardandoPrimeiroPonto = true;
        MostrarCanvasJogador();
    }

    private void AbrirConfiguracoes()
    {
        PausarJogo();
        menuInicial.gameObject.SetActive(false);
        menuConfiguracoes.gameObject.SetActive(true);
    }

    public void VoltarMenuInicial()
    {
        PausarJogo();
        menuInicial.gameObject.SetActive(true);
        menuConfiguracoes.gameObject.SetActive(false);
        if (menuGameOver != null)
        {
            menuGameOver.gameObject.SetActive(false);
        }
        OcultarCanvasJogador();
        jogoRodando = false;
        aguardandoPrimeiroPonto = false;
    }

    public void MostrarGameOver()
    {
        PausarJogo();
        menuInicial.gameObject.SetActive(false);
        menuConfiguracoes.gameObject.SetActive(false);
        if (menuGameOver != null)
        {
            menuGameOver.gameObject.SetActive(true);
        }
        OcultarCanvasJogador();
        jogoRodando = false;
        aguardandoPrimeiroPonto = false;
    }

    public void OnPressionarTecla()
    {
        if (menuGameOver != null && menuGameOver.gameObject.activeInHierarchy)
        {
            VoltarMenuInicial();
        }
        else if (menuInicial.gameObject.activeInHierarchy)
        {
            IniciarJogo();
        }
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

    private void RegistrarPontuacao()
    {
        GarantirReferencias();

        if (pontuacao != null)
        {
            pontuacao.AdicionarListenerPontuacao(QuandoPontuar);
            VerificarCanvasJogadorPorPontuacao();
        }
    }

    private void QuandoPontuar()
    {
        VerificarCanvasJogadorPorPontuacao();
    }

    private void MostrarCanvasJogador()
    {
        if (canvasJogador != null)
        {
            canvasJogador.SetActive(true);
        }
    }

    private void OcultarCanvasJogador()
    {
        if (canvasJogador != null)
        {
            canvasJogador.SetActive(false);
        }
    }

    private void GarantirReferencias()
    {
        if (canvasJogador == null)
        {
            var encontrados = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var candidato in encontrados)
            {
                if (candidato == null || candidato.name != "CanvasJogador")
                {
                    continue;
                }

                if (!candidato.scene.IsValid())
                {
                    continue;
                }

                canvasJogador = candidato;
                break;
            }
        }

        if (pontuacao == null)
        {
            pontuacao = FindObjectOfType<Pontuacao>();
        }
    }

    private void VerificarCanvasJogadorPorPontuacao()
    {
        GarantirReferencias();

        if (pontuacao == null)
        {
            return;
        }

        if (pontuacao.Pontos > 0)
        {
            aguardandoPrimeiroPonto = false;
            OcultarCanvasJogador();
        }
    }
}