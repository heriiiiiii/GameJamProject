using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class NF_CameraShakeManager : MonoBehaviour
{
    public static NF_CameraShakeManager instance;
    [SerializeField] private float globalShakeForce = 1f;

    private void Awake()
    {
        if(instance == null)
        {
            instance=this;
        }
    }
    public void CameraShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}
