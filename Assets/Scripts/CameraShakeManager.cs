using UnityEngine;
using Cinemachine;
public class CameraShakeManager : MonoBehaviour
{
    [SerializeField] private float _globalShakeForce = 1f;
    public void CameraShake(CinemachineImpulseSource _impulseSource)
    {
        _impulseSource.GenerateImpulseWithForce(_globalShakeForce);
    }
}

