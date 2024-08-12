using UnityEngine;

namespace Assets.Scripts.Mapamundi
{
    [CreateAssetMenu(fileName = "", menuName = "")]
    public class SelectableAreaData : ScriptableObject
    {
        [Tooltip("The name of the selectable area. It is case sensitive")]
        public string Name = null;

        [Tooltip("Info to show about the selectable area.")]
        [TextArea(5, 10)] public string Description = null;

        [Tooltip("Color that the selectable are will be painted on")]
        public Color SelectionColor = default;

        [Tooltip("Flag representing the selectable area")]
        public Sprite Flag = null;
    }
}
