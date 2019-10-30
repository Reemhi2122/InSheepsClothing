using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Grabber : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    public float grabBegin = 0.55f;
    public float grabEnd = 0.35f;

    private Hand _hand;
    public Hand Hand => _hand;

    private SteamVR_Action_Boolean _triggerButton;

    private FixedJoint m_Joint = null;

    protected GrabbableObject m_grabbedObj = null;

    protected Dictionary<GrabbableObject, int> m_grabCandidates = new Dictionary<GrabbableObject, int>();
    [SerializeField]
    protected Transform m_gripTransform = null;

    [SerializeField]
    protected bool m_parentHeldObject = false;

    protected bool m_grabVolumeEnabled = true;

    protected Vector3 m_lastPos;
    protected Quaternion m_lastRot;

    protected Vector3 m_grabbedObjectPosOff;
    protected Quaternion m_grabbedObjectRotOff;

    [SerializeField]
    protected Collider[] m_grabVolumes = null;

    private void Awake()
    {
        _hand = GetComponent<Hand>();
        _triggerButton = SteamVR_Actions._default.GrabPinch;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3.124542345689273489f);
        ControllerButtonHints.ShowTextHint(_hand, _triggerButton, "Pick up object");
    }

    private void Update()
    {
        if(_triggerButton[_hand.handType].state)
        {
            GrabBegin();
            _animator.SetBool("Grabs", true);
        }
        if(_triggerButton[_hand.handType].stateUp)
        {
            ControllerButtonHints.HideTextHint(_hand, _triggerButton);
            GrabEnd();
            _animator.SetBool("Grabs", false);
        }
    }

    protected virtual void GrabBegin()
    {
        float closestMagSq = float.MaxValue;
        GrabbableObject closestGrabbable = null;
        Collider closestGrabbableCollider = null;

        // Iterate grab candidates and find the closest grabbable candidate
        foreach (GrabbableObject grabbable in m_grabCandidates.Keys)
        {
            bool canGrab = !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            if (!canGrab)
            {
                continue;
            }

            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];
                // Store the closest grabbable
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                    closestGrabbableCollider = grabbableCollider;
                }
            }
        }

        // Disable grab volumes to prevent overlaps
        GrabVolumeEnable(false);

        if (closestGrabbable != null)
        {
            if (closestGrabbable.isGrabbed)
            {
                closestGrabbable.grabbedBy.OffhandGrabbed(closestGrabbable);
            }

            m_grabbedObj = closestGrabbable;
            m_grabbedObj.GrabBegin(this, closestGrabbableCollider);

            m_lastPos = transform.position;
            m_lastRot = transform.rotation;

            // Set up offsets for grabbed object desired position relative to hand.
            if (m_grabbedObj.snapPosition)
            {
                if (m_grabbedObj.snapOffset)
                {
                    Vector3 snapOffset = -m_grabbedObj.snapOffset.localPosition;
                    Vector3 snapOffsetScale = m_grabbedObj.snapOffset.lossyScale;
                    snapOffset = new Vector3(snapOffset.x * snapOffsetScale.x, snapOffset.y * snapOffsetScale.y, snapOffset.z * snapOffsetScale.z);
                    if (_hand.handType == SteamVR_Input_Sources.LeftHand)
                    {
                        snapOffset.x = -snapOffset.x;
                    }
                    m_grabbedObjectPosOff = snapOffset;
                }
                else
                {
                    m_grabbedObjectPosOff = Vector3.zero;
                }
            }
            else
            {
                Vector3 relPos = m_grabbedObj.transform.position - transform.position;
                relPos = Quaternion.Inverse(transform.rotation) * relPos;
                m_grabbedObjectPosOff = relPos;
            }

            if (m_grabbedObj.snapOrientation)
            {
                if (m_grabbedObj.snapOffset)
                {
                    m_grabbedObjectRotOff = Quaternion.Inverse(m_grabbedObj.snapOffset.localRotation);
                }
                else
                {
                    m_grabbedObjectRotOff = Quaternion.identity;
                }
            }
            else
            {
                Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
                m_grabbedObjectRotOff = relOri;
            }

            // Note: force teleport on grab, to avoid high-speed travel to dest which hits a lot of other objects at high
            // speed and sends them flying. The grabbed object may still teleport inside of other objects, but fixing that
            // is beyond the scope of this demo.
            MoveGrabbedObject(m_lastPos, m_lastRot, true);
            if (m_parentHeldObject)
            {
                m_grabbedObj.transform.parent = transform;
            }
        }
    }

    protected virtual void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
    {
        if (m_grabbedObj == null)
        {
            return;
        }

        Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
        Vector3 grabbablePosition = pos + rot * m_grabbedObjectPosOff;
        Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;

        if (forceTeleport)
        {
            grabbedRigidbody.transform.position = grabbablePosition;
            grabbedRigidbody.transform.rotation = grabbableRotation;
        }
        else
        {
            grabbedRigidbody.MovePosition(grabbablePosition);
            grabbedRigidbody.MoveRotation(grabbableRotation);
        }
    }

    protected void GrabEnd()
    {
        if (m_grabbedObj != null)
        {
            Vector3 linearVelocity = _hand.GetTrackedObjectVelocity();
            Vector3 angularVelocity = _hand.GetTrackedObjectAngularVelocity();

            GrabbableRelease(linearVelocity, angularVelocity);
        }

        // Re-enable grab volumes to allow overlap events
        GrabVolumeEnable(true);
    }

    protected void GrabbableRelease(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        m_grabbedObj.GrabEnd(linearVelocity, angularVelocity);
        if (m_parentHeldObject) m_grabbedObj.transform.parent = null;
        m_grabbedObj = null;
    }

    private void OnDestroy()
    {
        if (m_grabbedObj != null)
        {
            GrabEnd();
        }
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        // Get the grab trigger
        GrabbableObject grabbable = otherCollider.GetComponent<GrabbableObject>() ?? otherCollider.GetComponentInParent<GrabbableObject>();
        if (grabbable == null) return;

        // Add the grabbable
        int refCount = 0;
        m_grabCandidates.TryGetValue(grabbable, out refCount);
        m_grabCandidates[grabbable] = refCount + 1;
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        GrabbableObject grabbable = otherCollider.GetComponent<GrabbableObject>() ?? otherCollider.GetComponentInParent<GrabbableObject>();
        if (grabbable == null) return;

        // Remove the grabbable
        int refCount = 0;
        bool found = m_grabCandidates.TryGetValue(grabbable, out refCount);
        if (!found)
        {
            return;
        }

        if (refCount > 1)
        {
            m_grabCandidates[grabbable] = refCount - 1;
        }
        else
        {
            m_grabCandidates.Remove(grabbable);
        }
    }

    protected virtual void GrabVolumeEnable(bool enabled)
    {
        if (m_grabVolumeEnabled == enabled)
        {
            return;
        }

        m_grabVolumeEnabled = enabled;
        for (int i = 0; i < m_grabVolumes.Length; ++i)
        {
            Collider grabVolume = m_grabVolumes[i];
            grabVolume.enabled = m_grabVolumeEnabled;
        }

        if (!m_grabVolumeEnabled)
        {
            m_grabCandidates.Clear();
        }
    }

    protected virtual void OffhandGrabbed(GrabbableObject grabbable)
    {
        if (m_grabbedObj == grabbable)
        {
            GrabbableRelease(Vector3.zero, Vector3.zero);
        }
    }

    public bool _canSign = true;

    public void Reset()
    {
        _canSign = true;
    }
}
