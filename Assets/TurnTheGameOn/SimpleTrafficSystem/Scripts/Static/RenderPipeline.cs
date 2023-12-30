namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine.Rendering;

    public static class RenderPipeline
    {
        public static bool IsDefaultRP
        {
            get
            {
                if (GraphicsSettings.currentRenderPipeline)
                {
                    return false;
                }
                else
                {
                    return true;
                };
            }
        }

        public static bool IsHDRP
        {
            get
            {
                if (GraphicsSettings.currentRenderPipeline)
                {
                    if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                    {
                        return true;
                    }
                    else // assuming only HDRP or URP exist
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                };
            }
        }

        public static bool IsURP
        {
            get
            {
                if (GraphicsSettings.currentRenderPipeline)
                {
                    if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                    {
                        return false;
                    }
                    else // assuming only HDRP or URP exist
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                };
            }
        }
        
    }
}