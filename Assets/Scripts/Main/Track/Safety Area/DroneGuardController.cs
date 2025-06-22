using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using AudioSystem;

public class DroneGuardController : MonoBehaviour
{
    [SerializeField] ElectricRoomController room;

    private ParticleSystem _lightningParticle;
    private bool _hit;

    private void Start()
    {
        _hit = false;
        _lightningParticle = GetComponentInChildren<ParticleSystem>();
        _lightningParticle.Stop();
    }

    public void OnHit()
    {
        if (_hit)
            return;
        _hit = true;

        _lightningParticle.Play();
        GetComponent<Rigidbody>().useGravity = true;

        Transform DroneGuard_Bone_Main = transform.GetChild(1).GetChild(0);
        Transform Down_Half = DroneGuard_Bone_Main.GetChild(0);
        Transform Left_Arm = DroneGuard_Bone_Main.GetChild(1);
        Transform Right_Arm = DroneGuard_Bone_Main.GetChild(2);

        Down_Half.DOLocalMoveY(-0.0067f, 1f);
        Left_Arm.DOLocalMoveX(-0.0045f, 1f);
        Right_Arm.DOLocalMoveX(0.0045f, 1f);

        AudioManager.Instance.PlaySfx2D(SfxType.BlackDroneCrash);
        room.RemovedBlackDrone();
    }
}
