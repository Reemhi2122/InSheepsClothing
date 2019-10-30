using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

public class PlayerCamera : GrabbableObject
{
    private AudioSource _audioSource;
    private Animator _animator;

    private SteamVR_Action_Boolean _middleButton;

    private bool _hasPickedUp = false, _canTakePicture = true;

    public override void GrabBegin(Grabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
    }

    private void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _middleButton = SteamVR_Actions._default.Teleport;
    }

    private IEnumerator TakePicture()
    {
        _canTakePicture = false;
        if (GameManager.Instance.IsBeingLookedAt())
        {
            GameManager.Instance.LoseLife();
        }
        else
        {
            _audioSource.Play();
            _animator.SetTrigger("TakePicture");
            ScoreManager.Instance.AddScore(20);
        }
        yield return new WaitForSeconds(1);
        _canTakePicture = true;
    }

    private void Update()
    {
        if (isGrabbed && _middleButton[m_grabbedBy.Hand.handType].stateDown && _canTakePicture)
        {
            StartCoroutine(TakePicture());
            Debug.Log("Kut");
            ControllerButtonHints.HideTextHint(m_grabbedBy.Hand, _middleButton);
        }
    }

    protected override void PickUp()
    {
        if (!_hasPickedUp)
        {
            _hasPickedUp = true;
            ControllerButtonHints.ShowTextHint(m_grabbedBy.Hand, _middleButton, "Take photo");
        }
    }
}
