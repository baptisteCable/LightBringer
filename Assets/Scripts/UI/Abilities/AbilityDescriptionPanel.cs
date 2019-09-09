using UnityEngine.UI;
using UnityEngine;

public class AbilityDescriptionPanel : MonoBehaviour
{
    [SerializeField] private Text textObject;

    public void SetText(string textValue)
    {
        textObject.text = textValue;
    }
}
