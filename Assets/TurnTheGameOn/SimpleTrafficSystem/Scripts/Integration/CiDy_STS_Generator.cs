namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public static class CiDy_STS_Generator
    {
#if UNITY_EDITOR
#if CiDy
        public static CiDy_STS_GeneratedContent contentHolder
        {
            get
            {
                CiDy_STS_GeneratedContent _contentHolder = GameObject.FindObjectOfType<CiDy_STS_GeneratedContent>();
                if (_contentHolder == null)
                {
                    _contentHolder = new GameObject("CiDy_STS_GeneratedContent", typeof(CiDy_STS_GeneratedContent)).GetComponent<CiDy_STS_GeneratedContent>();
                }
                return _contentHolder;
            }
        }

        public static void Generate()
        {
            contentHolder.Generate();
        }

        public static void Clear()
        {
            contentHolder.ClearAllGeneratedRoutes();
        }
#endif
#endif
    }
}