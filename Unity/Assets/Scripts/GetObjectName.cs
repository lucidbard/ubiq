using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class GetObjectName : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public string aName = null;

    public void GetName()
    {
        
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.tag == "Default Object")
            {
                aName = hit.collider.gameObject.name;
            }
            else
                Debug.Log("Cannot get name of object");
        }
        else
            Debug.Log("Invalid Selection");
    }
}