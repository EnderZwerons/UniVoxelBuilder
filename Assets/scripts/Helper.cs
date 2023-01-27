using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public static void LockCursor(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static bool CursorLocked
    {
        get
        {
            return Cursor.lockState == CursorLockMode.Locked && !Cursor.visible;
        }
    }
}
