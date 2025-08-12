using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Pontuacao : MonoBehaviour {
    public int Pontos{ get;private  set; }

    [SerializeField]
    private Text textoPontuacao;
    [SerializeField]
    private UnityEvent aoPontuar;
    [SerializeField]
    private AudioClip somPontuacao; 
    private AudioSource audioSource; 
    private void Awake()
    {
        this.audioSource = this.GetComponent<AudioSource>();
    }
    public void AdicionarPontos()
    {
        this.Pontos++;
        this.textoPontuacao.text = this.Pontos.ToString();
        this.aoPontuar.Invoke();
         if (this.audioSource != null && this.somPontuacao != null)
        {
            this.audioSource.PlayOneShot(this.somPontuacao);
        }
    }

    public void Reiniciar()
    {
        this.Pontos = 0;
        this.textoPontuacao.text = this.Pontos.ToString();
    }

    public void SalvarRecorde()
    {
        int recordeAtual = PlayerPrefs.GetInt("recorde");
        
        if (this.Pontos > recordeAtual)
        {
            PlayerPrefs.SetInt("recorde", this.Pontos);
        }
    }
}

