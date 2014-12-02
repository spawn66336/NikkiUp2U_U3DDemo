using UnityEngine;
using System.Collections;

public interface ISpriteImageProcessStrategy
{
    Texture2D[] Process(AtlasSpriteImage[] imgs);    
}
