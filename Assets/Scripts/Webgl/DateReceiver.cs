using UnityEngine;
using UnityEngine.UI;

public class DateReceiver : MonoBehaviour
{
    public Text textField;

    [System.Serializable]
    public class DateObject
    {
        public string type;
        public float value;
        public int resolution;
        public long captureTime;
    }

    public void Date(string jsonData)
    {
        try
        {
            DateObject dateObject = JsonUtility.FromJson<DateObject>(jsonData);

            if (dateObject == null)
            {
                Debug.LogError("Erro: JSON desserializado como nulo.");
                return;
            }

            if (textField != null)
            {
                textField.text = $"Type: {dateObject.type}\n" +
                                 $"Value: {dateObject.value}\n" +
                                 $"Resolution: {dateObject.resolution}\n" +
                                 $"CaptureTime: {dateObject.captureTime}";
            }
            else
            {
                Debug.LogError("Campo de texto não atribuído.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erro ao processar JSON: {ex.Message}\nJSON recebido: {jsonData}");
        }
    }
}