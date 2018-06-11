﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using UnityEngine;
using Valve.VR;

public class SteamVR_TrackedStaticObject : MonoBehaviour {
    public enum EIndex {
        None = -1,
        Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
        Device1,
        Device2,
        Device3,
        Device4,
        Device5,
        Device6,
        Device7,
        Device8,
        Device9,
        Device10,
        Device11,
        Device12,
        Device13,
        Device14,
        Device15
    }

    public EIndex index;

    [Tooltip("If not set, relative to parent")]
    public Transform origin;

    public bool isValid { get; private set; }

    private void OnNewPoses(TrackedDevicePose_t[] poses) {
        if (index == EIndex.None)
            return;

        var i = (int)index;

        isValid = false;
        if (poses.Length <= i)
            return;

        if (!poses[i].bDeviceIsConnected)
            return;

        if (!poses[i].bPoseIsValid)
            return;

        isValid = true;

        var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

        if (origin != null) {
            Vector3 actualPos = origin.transform.TransformPoint(pose.pos);
            transform.position = new Vector3(0, 0, actualPos.z);
            transform.rotation = origin.rotation * pose.rot;
        } else {
            //transform.localPosition = pose.pos;
            transform.localPosition = new Vector3(0, 0, pose.pos.z);
            transform.localRotation = pose.rot;
        }
    }

    SteamVR_Events.Action newPosesAction;

    SteamVR_TrackedStaticObject() {
        newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
    }

    void OnEnable() {
        var render = SteamVR_Render.instance;
        if (render == null) {
            enabled = false;
            return;
        }

        newPosesAction.enabled = true;
    }

    void OnDisable() {
        newPosesAction.enabled = false;
        isValid = false;
    }

    public void SetDeviceIndex(int index) {
        if (System.Enum.IsDefined(typeof(EIndex), index))
            this.index = (EIndex)index;
    }
}

