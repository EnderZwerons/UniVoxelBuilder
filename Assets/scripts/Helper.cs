using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public static void LockCursor(bool locked)
    {
        //screen.lockcursor was easier but this works too... I guess..
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public static Sprite TexToSprite(Texture2D texture)
    {
        // Create a new sprite from the given texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public static Sprite TexToSprite(Texture texture)
    {
        // Create a new sprite from the given texture
        Sprite sprite = Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public static bool CursorLocked
    {
        get
        {
            return Cursor.lockState == CursorLockMode.Locked && !Cursor.visible;
        }
    }
}
