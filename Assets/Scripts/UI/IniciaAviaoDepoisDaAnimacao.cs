using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IniciaAviaoDepoisDaAnimacao : MonoBehaviour {
    [SerializeField]
    private UnityEvent aoTerminarAnimacao;
    void Start()
    {
        IniciarJogo();
    }
	public void IniciarJogo()
    {
        this.aoTerminarAnimacao.Invoke();
    }
}
