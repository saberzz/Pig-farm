using UnityEngine;

namespace PigFarm.UI.Screens
{
    [CreateAssetMenu(menuName = "Pig Farm/UI Theme", fileName = "PigFarmUITheme")]
    public sealed class PigFarmUIThemeConfig : ScriptableObject
    {
        [SerializeField] Color background = new Color(0.10f, 0.22f, 0.15f, 1f);
        [SerializeField] Color surface = new Color(0.95f, 0.88f, 0.69f, 1f);
        [SerializeField] Color accent = new Color(0.84f, 0.34f, 0.18f, 1f);
        [SerializeField] Color primaryText = new Color(0.15f, 0.12f, 0.08f, 1f);
        [SerializeField] Color secondaryText = new Color(0.38f, 0.31f, 0.21f, 1f);

        public Color Background { get { return background; } }
        public Color Surface { get { return surface; } }
        public Color Accent { get { return accent; } }
        public Color PrimaryText { get { return primaryText; } }
        public Color SecondaryText { get { return secondaryText; } }
    }
}
