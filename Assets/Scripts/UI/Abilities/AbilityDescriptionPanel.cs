using UnityEngine.UI;
using UnityEngine;

public class AbilityDescriptionPanel : MonoBehaviour
{
    [SerializeField] private Text titleObject = null;
    [SerializeField] private Text descriptionObject = null;

    public void SetTitle(string textValue)
    {
        titleObject.text = textValue;
    }

    public void SetDescription(string textValue)
    {
        descriptionObject.text = textValue;
    }
}
