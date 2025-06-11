using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static DateReceiver;

public class DateReceiver : MonoBehaviour
{
    public Text textField;
    private Aviao aviao;
    public Menu menu;
    public InterfaceGameOver interfaceGameOver;
    [System.Serializable]
    public class DateObject
    {
        public string type;
        public float value;
        public int resolution;
        public long captureTime;
    }

    [SerializeField]
    private UnityEvent aoPressionarTecla;

    public string teste;

    private void Start()
    {
        this.aviao = GameObject.FindObjectOfType<Aviao>(); 
        this.menu = GameObject.FindObjectOfType<Menu>();
        this.interfaceGameOver = GameObject.FindObjectOfType<InterfaceGameOver>(); 
    }
   
    public void Date(string jsonData)
    {
        DateObject dateObject = JsonUtility.FromJson<DateObject>(jsonData);
        if (dateObject.type == "10")
        {
           /*  textField.text = $"Type: {dateObject.type}, " +
                             //Type = 10 Dar impulso no avião
                             $"Value: {dateObject.value}, " +
                             $"Resolution: {dateObject.resolution}, " +
                             $"CaptureTime: {dateObject.captureTime}"; */
            teste = dateObject.type;
            if (menu == null)
            {
                menu = GameObject.FindObjectOfType<Menu>();
            }
            if (interfaceGameOver == null)
            {
                interfaceGameOver = GameObject.FindObjectOfType<InterfaceGameOver>();
            }
            if (menu != null)
            {
                if (menu.menuInicial.gameObject.activeInHierarchy)
                {
                    menu.IniciarJogo();
                }
                else if (menu.JogoRodando)
                {
                    if (aviao == null)
                        aviao = GameObject.FindObjectOfType<Aviao>();
                    if (aviao != null)
                        aviao.DarImpulso();
                }
            }
            if (interfaceGameOver != null && interfaceGameOver.gameObject.activeInHierarchy && interfaceGameOver.imagemGameOver.activeSelf)
                {
                    menu.VoltarMenuInicial();
                }
        }
    }
}