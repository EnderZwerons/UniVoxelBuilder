using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnimations : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(AnimationLoop());
    }

    public Vector4 walkAnimation;

    public IEnumerator AnimationLoop()
    {
        for (;;)
        {
            if (PlayerMovement.instance.isWalking)
            {
                Vector3 walkAnim = ifBoolDivideAnimation(!PlayerMovement.instance.isSprinting, walkAnimation);
                GetComponent<Rotator>().rotationAxes.z = walkAnim.z;
                GetComponent<Rotator>().rotationAxes.x = walkAnim.x;
                GetComponent<Rotator>().rotationAxes.y = walkAnim.y;
                yield return new WaitForSeconds(walkAnimation.w);
                GetComponent<Rotator>().rotationAxes.z = -walkAnim.z;
                GetComponent<Rotator>().rotationAxes.x = -walkAnim.x;
                GetComponent<Rotator>().rotationAxes.y = -walkAnim.y;
                yield return new WaitForSeconds(walkAnimation.w);
                GetComponent<Rotator>().rotationAxes = Vector3.zero;
            }
            yield return null;
        }
    }

    public Vector3 ifBoolDivideAnimation(bool b, Vector3 animation)
    {
        if (b)
        {
            return new Vector3(animation.y / 2, animation.x / 2, animation.z / 2);
        }
        return animation;
    }
}
