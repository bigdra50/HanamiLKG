using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

[ExecuteInEditMode]
public class SetLeapHandScale : MonoBehaviour
{
    public LeapServiceProvider leapController;
    public bool scaleTheHands = true;
    public Transform[] hands;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (leapController != null)
        {
            foreach (Transform hand in hands)
            {
                hand.transform.localScale = leapController.transform.localScale;
            }
        }
    }
}