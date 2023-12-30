namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEditor;
    using UnityEngine;

    public class Editor_EditorLinks
    {
        [MenuItem("Tools/Simple Traffic System/Discussion Forum", false, 0)]
        public static void DiscussionForum()
        {
            Application.OpenURL("https://forum.unity.com/threads/simple-traffic-system.794268/");
        }

        [MenuItem("Tools/Simple Traffic System/Documentation", false, 1)]
        public static void Documentation()
        {
            Application.OpenURL("https://simpletrafficsystem.turnthegameon.com/");
        }

    }
}