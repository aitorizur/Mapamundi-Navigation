using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Assets.Scripts.Mapamundi
{
    public class SelectableAreaUI : MonoBehaviour
    {
        public TextMeshProUGUI SelectableAreaName = null;
        public TextMeshProUGUI SelectableAreaDescription = null;
        public Image SelectableAreaFlag = null;

        public void ShowAreaInfo(SelectableAreaData desiredSelectableAreaData)
        {
            this.gameObject.SetActive(true);
            this.SelectableAreaName.text = desiredSelectableAreaData.Name;
            this.SelectableAreaDescription.text = desiredSelectableAreaData.Description;
            this.SelectableAreaFlag.sprite = desiredSelectableAreaData.Flag;
        }

        public void HideAreaInfo()
        {
            this.gameObject.SetActive(false);
        }
    }
}