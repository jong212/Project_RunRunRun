using UnityEngine;
using UnityEngine.UI;
namespace tkitfacn.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleCard : MonoBehaviour
    {
        public Toggle toggle { get; private set; }
        private void Start()
        {
            toggle = GetComponent<Toggle>();
            CardSlider cs = GetComponentInParent<CardSlider>();
            toggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    cs.OpenCard(transform);
                }
            });

            cs.onCardChange.AddListener((value) =>
            {
                if (value == transform)
                {
                    toggle.isOn = true;
                }
            });
        }
    }
}