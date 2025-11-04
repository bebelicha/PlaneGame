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

    [Header("Feedback Visual do Combo")]
    [SerializeField]
    private Sprite spriteAlternativo;
    [SerializeField]
    private Color comboColor = Color.yellow;

    private Vector3 posicaoInicial;
    private Animator animacao;
    private bool deveImpulsionar;

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private Sprite spriteOriginal;
    private bool comboAtivo;

    private void Awake()
    {
        this.posicaoInicial = this.transform.position;
        this.fisica = this.GetComponent<Rigidbody2D>();
        this.animacao = this.GetComponent<Animator>();
        
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.corOriginal = this.spriteRenderer.color;
        this.spriteOriginal = this.spriteRenderer.sprite;
        this.spriteRenderer.color = this.corOriginal;
        this.comboAtivo = false;
    }

    private void Update()
    {
        if (this.animacao.enabled)
        {
            this.animacao.SetFloat("VelocidadeY", this.fisica.linearVelocity.y);
        }

        if (this.spriteRenderer != null)
        {
            this.spriteRenderer.color = this.comboAtivo ? this.comboColor : this.corOriginal;
        }
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
        this.DesativarCombo();
    }
    public void AlterarGravityScale(float valor)
    {
        this.fisica.gravityScale = valor;
    }

     public void AlterarAngularDamping(float valor)
    {
       this.fisica.angularDamping = valor;
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
            this.fisica.linearVelocity = Vector2.zero;
        }
        else if (colisao.gameObject.CompareTag("Obstaculo"))
        {
            this.aoBater.Invoke();
            this.animacao.SetTrigger("bater");
            this.fisica.simulated = false;
            Object.FindFirstObjectByType<Diretor>().FinalizarJogo();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        this.aoPassarPeloObstaculo.Invoke();
    }
    public void AtivarCombo()
    {
        if (this.spriteRenderer == null)
        {
            return;
        }

        if (this.comboAtivo)
        {
            return;
        }

        this.comboAtivo = true;
        this.spriteRenderer.color = this.comboColor;
    }

    public void DesativarCombo()
    {
        if (this.spriteRenderer == null)
        {
            return;
        }

        if (!this.comboAtivo)
        {
            return;
        }

        this.comboAtivo = false;
        this.spriteRenderer.color = this.corOriginal;
    }

    public void TrocarSpriteTemporariamente()
    {
        Debug.Log("[Aviao.cs] Método TrocarSpriteTemporariamente() foi chamado!");
        StartCoroutine(PiscarSprite());
    }

    private IEnumerator PiscarSprite()
    {
        if (this.spriteAlternativo == null)
        {
            Debug.LogError("[Aviao.cs] ERRO: O 'Sprite Alternativo' não foi atribuído no Inspector do Unity!");
            yield break; 
        }

        Debug.Log("[Aviao.cs] Trocando para o sprite alternativo.");
        
        if (this.animacao != null)
        {
            this.animacao.enabled = false;
        }

        this.spriteRenderer.sprite = this.spriteAlternativo;

        yield return new WaitForSeconds(0.5f);

        Debug.Log("[Aviao.cs] Voltando para o sprite original.");
        
        this.spriteRenderer.sprite = this.spriteOriginal;

        if (this.animacao != null)
        {
            this.animacao.enabled = true;
        }
    }
}