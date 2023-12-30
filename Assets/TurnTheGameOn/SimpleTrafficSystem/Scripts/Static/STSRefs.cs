#if UNITY_EDITOR
namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEditor;

    public static class STSRefs
    {
        public static AssetReferences AssetReferences
        {
            get {
                if (m_AssetReferences == null)
                {
                    var guids = AssetDatabase.FindAssets("t:AssetReferences");
                    if (guids.Length > 0)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<AssetReferences>(AssetDatabase.GUIDToAssetPath(guids[0]));
                        m_AssetReferences = asset;
                    }
                    return m_AssetReferences;
                }
                else
                {
                    return m_AssetReferences;
                }
            }
        }
        static AssetReferences m_AssetReferences;
    }
}
#endif