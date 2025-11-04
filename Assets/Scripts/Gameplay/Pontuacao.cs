using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Pontuacao : MonoBehaviour
{
    public int Pontos { get; private set; }

    [SerializeField]
    private Text textoPontuacao;
    [SerializeField]
    private UnityEvent aoPontuar;
    [SerializeField]
    private AudioClip somPontuacao;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void AdicionarPontos()
    {
        Pontos++;
        if (textoPontuacao != null)
        {
            textoPontuacao.text = Pontos.ToString();
        }
        aoPontuar?.Invoke();
        if (audioSource != null && somPontuacao != null)
        {
            audioSource.PlayOneShot(somPontuacao);
        }
    }

    public void Reiniciar()
    {
        Pontos = 0;
        if (textoPontuacao != null)
        {
            textoPontuacao.text = Pontos.ToString();
        }
    }

    public void SalvarRecorde()
    {
        var recordeAtual = PlayerPrefs.GetInt("recorde");
        if (Pontos > recordeAtual)
        {
            PlayerPrefs.SetInt("recorde", Pontos);
        }
    }

    public void AdicionarListenerPontuacao(UnityAction listener)
    {
        if (listener == null)
        {
            return;
        }
        aoPontuar.AddListener(listener);
    }
}
