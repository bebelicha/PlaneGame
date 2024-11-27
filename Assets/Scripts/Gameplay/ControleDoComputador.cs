using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleDoComputador : MonoBehaviour
{
    [SerializeField]
    private float intervalo;
    private Aviao aviao;
    // Start is called before the first frame update
    private void Start()
    {
        this.aviao = this.GetComponent<Aviao>();
        StartCoroutine(this.Impulsionar());
    }

    // Update is called once per frame
   
    private IEnumerator Impulsionar()
    {
        while(true)
        {
              yield return new WaitForSeconds(intervalo);
                this.aviao.DarImpulso();
        }
      
    }
}
