using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;

    public float gravity = -9.81f;

    public float jumpHeight = 3f;

    public Transform groundCheck;

    public LayerMask groundMask;

    public static PlayerMovement instance;

    public static bool isFlying;

    Vector3 velocity;

    private bool isGrounded
    {
        get
        {
            return Physics.CheckSphere(groundCheck.position, 0.01f, groundMask);
        }
    }

    public bool isSprinting
    {
        get
        {
            return Input.GetKey(sprintKey) && Input.GetKey("w") && !Input.GetKey("s");
        }
    }

    public bool isWalking
    {
        get
        {
            return updateKeyboardControls() != Vector2.zero;
        }
    }

    void Awake()
    {
        instance = this;
    }

    public Vector2 updateKeyboardControls()
	{
		float num = default(float);
		float num2 = default(float);
		if (Input.GetKey("s"))
		{
			num = 0.3f;
		}
		if (Input.GetKey("w"))
		{
			num = -0.3f;
		}
		if (Input.GetKey("d"))
		{
			num2 = -0.3f;
		}
		if (Input.GetKey("a"))
		{
			num2 = 0.3f;
		}
        if (isSprinting)
        {
            num *= 1.75f;
            num2 *= 1.75f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                num *= 0.75f;
                num2 *= 0.75f;
            }
        }
        if (isFlying)
        {
            num *= 3f;
            num2 *= 3f;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            num *= 1.35f;
            num2 *= 1.35f;
        }
		return new Vector2(num2, num);
	}

    public void SetFly(bool fly)
    {
        isFlying = fly;
        GetComponent<BoxCollider>().enabled = fly;
        GetComponent<CharacterController>().height = (fly ? 0f : 1.35f);
        GetComponent<CharacterController>().center = new Vector3(0f, (fly) ? 99999f : -0.337f, 0f);
    }

    public KeyCode sprintKey
    {
        get
        {
            return (PlayerPrefs.GetInt("badcontrols") == 1 ? KeyCode.LeftControl : KeyCode.LeftShift);
        }
    }

    public KeyCode crouchKey
    {
        get
        {
            return (PlayerPrefs.GetInt("badcontrols") == 1 ? KeyCode.LeftShift : KeyCode.C);
        }
    }

    void Update()
    {
        if (UIController.inventoryOpen)
        {
            return;
        }
        if (isSprinting && Camera.main.fieldOfView < 100f)
        {
            Camera.main.fieldOfView += Time.deltaTime * 150f;
        }
        else if (!isSprinting && Camera.main.fieldOfView > 90f)
        {
            Camera.main.fieldOfView -= 150f * Time.deltaTime;
        }
        if (!isFlying)
        {
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            float x = -updateKeyboardControls().x ;
            float z = -updateKeyboardControls().y ;
            Vector3 move = transform.right * x + transform.forward * z;       
            if (move.magnitude > 1)
            {
                move /= move.magnitude;
            }
            controller.Move(move * speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.Space) && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            return;
        }
        float x2 = -updateKeyboardControls().x;
        float z2 = -updateKeyboardControls().y;
        Vector3 move2 = transform.right * x2 + transform.forward * z2;
        move2.y += (Input.GetKey(KeyCode.Space) ? 3f : (Input.GetKey(crouchKey) ? -3f : 0f));
        if (move2.magnitude > 1)
        {
            move2 /= move2.magnitude;
        }
        controller.Move(move2 * speed * Time.deltaTime);
    }
}
