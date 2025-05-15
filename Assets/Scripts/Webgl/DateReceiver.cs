using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static DateReceiver;

public class DateReceiver : MonoBehaviour
{
    public Text textField;
    private Aviao aviao;

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
    }
   
    public void Date(string jsonData)
    {
        DateObject dateObject = JsonUtility.FromJson<DateObject>(jsonData);
        if (dateObject.type == "10")
        {
            textField.text = $"Type: {dateObject.type}, " +
                             //Type = 10 Dar impulso no avião
                             $"Value: {dateObject.value}, " +
                             $"Resolution: {dateObject.resolution}, " +
                             $"CaptureTime: {dateObject.captureTime}";
            teste = dateObject.type;
            this.aoPressionarTecla.Invoke();
        }

    }

    
}