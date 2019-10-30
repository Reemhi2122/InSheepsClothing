using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class GrabbableSign : GrabbableObject
{
    public bool IsYes;

    public override void GrabBegin(Grabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);

        GameManager.Instance.HoldSign(IsYes);
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
    }

    public bool GetAnswerType()
    {
        return IsYes;
    }
}
