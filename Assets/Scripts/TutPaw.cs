using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

public class TutPaw : MonoBehaviour
{
    private Hand _hand;
    public Animator BoardAnim;

    private SteamVR_Action_Boolean _triggerButton;

    private void Awake()
    {
        _hand = GetComponent<Hand>();
        _triggerButton = SteamVR_Actions._default.GrabPinch;
    }

    private void Update()
    {
        if (_triggerButton[_hand.handType].stateDown)
        {
            Debug.Log("kut");
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red, 10);

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Play"))
                    BoardAnim.SetTrigger("Turn");

                if (hit.collider.CompareTag("Exit"))
                    Application.Quit();
            }
        }
    }
}
