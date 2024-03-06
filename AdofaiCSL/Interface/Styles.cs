using UnityEngine;

namespace AdofaiCSL.Interface
{
    internal static class Styles
    {
        internal static GUIStyle Header = new(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 15,
            normal = new GUIStyleState()
            {
                textColor = new Color(0.2f, 0.667f, 0.9f)
            }
        };

        internal static GUIStyle Warning = new(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = new Color(0.9f, 0, 0.05f)
            }
        };
    }
}
