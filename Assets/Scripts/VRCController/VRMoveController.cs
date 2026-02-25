using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using Unity.VisualScripting;

public class VRMoveController : SingletonMono<VRMoveController>
{
    // set init position
    private Vector3 initPosition = new(512f, 250f, 512f);

    // left hand
    private HandJointLocations LhandJointLocations;
    private Vector3f LhandPos;
    private Vector3f LlastHandPos;

    // right hand
    private HandJointLocations RhandJointLocations;
    private Vector3f RhandPos;
    private Vector3f RlastHandPos;

    private bool init = false;

    public Camera VRCamera;
    private CharacterController characterController;
    private readonly float lenthMin = 1.0f;
    private readonly float lenthMax = 20f;
    private float speed = 0;
    private Vector3 gravity = new(0, -9.8f, 0);
    private bool gravityFlag;
    private float timeCount = 0;
    private float LlenthCount = 0;
    private float RlenthCount = 0;
    public bool isRunning;
    private bool activate;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        this.transform.SetPositionAndRotation(initPosition, Quaternion.identity);
        SetGravity(false);

        LhandJointLocations = new HandJointLocations();
        RhandJointLocations = new HandJointLocations();

        StartCoroutine(Delay());
    }

    private void Update()
    {
        // inactivate when player enter the system
        if (!activate)
            return;

        // disable when chat panel is open
        if (ChatController.Instance.IsChatPanelActive())
            return;

        // gravity
        CalGravity();

        // check hand posture move
        CalHandMove();
    }

    private IEnumerator Delay()
    {
        activate = false;

        yield return new WaitForSeconds(10);
        activate = true;
    }

    public void SetGravity(bool flag)
    {
        gravityFlag = flag;
    }

    private void CalGravity()
    {
        if (gravityFlag)
        {
            // move on water surface 
            if (SceneGenerator.Instance.crestWater.activeSelf && this.transform.position.y < SceneGenerator.Instance.GetCurrentWaterLevel() + 2)
                return;

            characterController.Move(gravity * Time.deltaTime);
        }
    }

    private float HandMoveLen(Vector3f pos, Vector3f lastPos)
    {
        float x = pos.x - lastPos.x;
        float y = pos.y - lastPos.y;
        float z = pos.z - lastPos.z;

        return Mathf.Sqrt(x * x + y * y + z * z);
    }

    private void CalHandMove()
    {
        // check hand track
        if (!PXR_HandTracking.GetJointLocations(HandType.HandLeft, ref LhandJointLocations)
            || !PXR_HandTracking.GetJointLocations(HandType.HandRight, ref RhandJointLocations))
        {
            timeCount = 0;
            LlenthCount = 0;
            RlenthCount = 0;
            init = false;
            return;
        }

        // get hand position
        LhandPos = LhandJointLocations.jointLocations[0].pose.Position;
        RhandPos = RhandJointLocations.jointLocations[0].pose.Position;

        // init lastpos
        if (!init)
        {
            LlastHandPos = LhandPos;
            RlastHandPos = RhandPos;
            init = true;
        }

        // count swing move length in 1s
        if (timeCount < 1.0f)
        {
            timeCount += Time.deltaTime;
            LlenthCount += HandMoveLen(LhandPos, LlastHandPos);
            RlenthCount += HandMoveLen(RhandPos, RlastHandPos);
            LlastHandPos = LhandPos;
            RlastHandPos = RhandPos;
        }
        else
        {
            if (LlenthCount < lenthMin || RlenthCount < lenthMin)
            {
                speed = 0;
                isRunning = false;
            } 
            else
            {
                // 0 < swingingSpeed < speedMax
                speed = Mathf.Clamp(((LlenthCount + RlenthCount) / 2 - lenthMin) * 10, 0, lenthMax);
                isRunning = true;
            }

            timeCount = 0;
            LlenthCount = 0;
            RlenthCount = 0;
        }

        // movement
        if (speed != 0)
        {
            Vector3 forward = VRCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            characterController.Move(forward * speed * Time.deltaTime);
        }
    }

    // reset user position when the scene has been changed
    public void ReSetPosition()
    {
        if (ChatController.Instance.IsChatPanelActive())
        {
            ChatController.Instance.SwitchChatPanelState();
        }

        this.transform.SetPositionAndRotation(initPosition, Quaternion.identity);

        if (!ChatController.Instance.IsChatPanelActive())
        {
            ChatController.Instance.SwitchChatPanelState();
        }
    }
}
