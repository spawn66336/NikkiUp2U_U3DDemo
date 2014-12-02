using UnityEngine;
using System.Collections;

public class DefaultTexturePackagingStrategy : ITexturePackagingStrategy
{//打包算法封装

    public Texture2D Pack(Texture2D tex, Texture2D[] imgs, object config)
    {//打包纹理
        Rect[] rects;

#if UNITY_3_5 || UNITY_4_0
		int maxSize = 4096;
#else
        int maxSize = SystemInfo.maxTextureSize;
#endif
        rects = tex.PackTextures(imgs, 1, maxSize);

        for (int i = 0; i < imgs.Length; ++i)
        {
            Rect rect = ConvertToPixels(rects[i], tex.width, tex.height, true);

            // Make sure that we don't shrink the textures
            if (Mathf.RoundToInt(rect.width) != imgs[i].width) return null;
        }
        return tex;
    }

    static public Rect ConvertToPixels(Rect rect, int width, int height, bool round)
    {
        Rect final = rect;

        if (round)
        {
            final.xMin = Mathf.RoundToInt(rect.xMin * width);
            final.xMax = Mathf.RoundToInt(rect.xMax * width);
            final.yMin = Mathf.RoundToInt((1f - rect.yMax) * height);
            final.yMax = Mathf.RoundToInt((1f - rect.yMin) * height);
        }
        else
        {
            final.xMin = rect.xMin * width;
            final.xMax = rect.xMax * width;
            final.yMin = (1f - rect.yMax) * height;
            final.yMax = (1f - rect.yMin) * height;
        }
        return final;
    }
}
