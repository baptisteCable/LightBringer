using UnityEngine.UI;
using UnityEngine;

public class AbilityDescriptionPanel : MonoBehaviour
{
    [SerializeField] private Text titleObject;
    [SerializeField] private Text descriptionObject;

    public void SetTitle(string textValue)
    {
        titleObject.text = textValue;
    }

    public void SetDescription(string textValue)
    {
        descriptionObject.text = textValue;
    }
}
