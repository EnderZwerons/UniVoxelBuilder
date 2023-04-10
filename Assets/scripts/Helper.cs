using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for now this is just for locking the cursor but it might have more later on
public class Helper : MonoBehaviour
{
    public static void LockCursor(bool locked)
    {
        //screen.lockcursor was easier but this works too... I guess..
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public static bool CursorLocked
    {
        get
        {
            return Cursor.lockState == CursorLockMode.Locked && !Cursor.visible;
        }
    }
}
