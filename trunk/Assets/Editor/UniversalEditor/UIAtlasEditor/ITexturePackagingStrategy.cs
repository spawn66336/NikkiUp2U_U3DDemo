using UnityEngine;
using System.Collections;

public interface ITexturePackagingStrategy 
{
    Texture2D Pack(Texture2D tex, Texture2D[] imgs, object config);	
}
