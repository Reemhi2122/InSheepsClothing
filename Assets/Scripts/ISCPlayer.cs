using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ISCPlayer : MonoBehaviour
{ 
    public static ISCPlayer Instance = null;

    [HideInInspector] public Grabber[] Paws;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        Paws = GetComponentsInChildren<Grabber>();
    }

    public ISCPlayer GetPlayer()
    {
        return this;
    }
}

public class VoteResult
{
    public bool HasVoted = false;
    public bool Vote = false;
}