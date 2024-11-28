using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Aviao : MonoBehaviour
{
    private Rigidbody2D fisica;
    [SerializeField]
    private float forca;
    [SerializeField]
    private UnityEvent aoBater;

    [SerializeField]
    private UnityEvent aoPassarPeloObstaculo;
    private Vector3 posicaoInicial;
    private bool deveImpulsionar;
    private Animator animacao;

    private void Awake()
    {
        this.posicaoInicial = this.transform.position;
        this.fisica = this.GetComponent<Rigidbody2D>();
        this.animacao = this.GetComponent<Animator>();
    }

    private void Update()
    {
        this.animacao.SetFloat("VelocidadeY", this.fisica.linearVelocity.y);
    }

    private void FixedUpdate()
    {
        if (this.deveImpulsionar)
        {
            this.Impulsionar();
        }
    }

    public void DarImpulso()
    {
        this.deveImpulsionar = true;
    }

    public void Reiniciar()
    {
        this.transform.position = this.posicaoInicial;
        this.fisica.simulated = true;
    }

    private void Impulsionar()
    {
        this.fisica.linearVelocity = Vector2.zero;
        this.fisica.AddForce(Vector2.up * this.forca, ForceMode2D.Impulse);
        this.deveImpulsionar = false;
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.CompareTag("Barreira"))
        {
            // Lógica para barrar o jogador sem destruir o objeto nem terminar o jogo
            this.fisica.linearVelocity = Vector2.zero; // Exemplo: parar o movimento do jogador
        }
        else if (colisao.gameObject.CompareTag("Obstaculo"))
        {
            // Lógica normal de colisão
            this.aoBater.Invoke();
            this.animacao.SetTrigger("bater");
            this.fisica.simulated = false;
            Object.FindFirstObjectByType<Diretor>().FinalizarJogo();
        }
    }

    private void OnTriggerEnter2D()
    {
        this.aoPassarPeloObstaculo.Invoke();
    }
}
