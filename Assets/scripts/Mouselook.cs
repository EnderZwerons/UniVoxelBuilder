using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouselook : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform playerBody;

    private float xRotation = 0f;

	void Start()
    {
        //this is stupid I dont know why the editor sens is so slow but it is for some reason
        if (Application.isEditor)
        {
            mouseSensitivity *= 5f;
        }

        //lock cursor and set the clip planes
        Helper.LockCursor(true);
        GetComponent<Camera>().farClipPlane = PlayerPrefs.GetFloat("renderDistance");
	}
	
	void Update()
    {
        if (UIController.inventoryOpen)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

