using UnityEngine;
using UnityEngine.UI;

public class DateReceiver : MonoBehaviour
{
    public Text textField; 

    public void Date(string formattedDate)
    {
        if (textField != null)
        {
            textField.text = formattedDate;
        }
        else
        {
            Debug.LogError("Campo de texto não atribuído.");
        }
    }
}
