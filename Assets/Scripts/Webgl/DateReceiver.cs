using UnityEngine;
using UnityEngine.UI;

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
        public long captureTime;
    }

    private void Start()
    {
        this.aviao = FindObjectOfType<Aviao>();
        this.menu = FindObjectOfType<Menu>();
        this.interfaceGameOver = FindObjectOfType<InterfaceGameOver>();
    }

    public void Date(string jsonData)
    {
        DateObject dateObject = JsonUtility.FromJson<DateObject>(jsonData);

        if (dateObject.type == "mouthOpen" && dateObject.value == 1)
        {
            if (textField != null)
            {
                textField.text = $"Type: {dateObject.type}, " +
                                 $"Value: {dateObject.value}, " +
                                 $"CaptureTime: {dateObject.captureTime}";
            }

            if (menu == null)
            {
                menu = FindObjectOfType<Menu>();
            }
            if (interfaceGameOver == null)
            {
                interfaceGameOver = FindObjectOfType<InterfaceGameOver>();
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
                        aviao = FindObjectOfType<Aviao>();
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