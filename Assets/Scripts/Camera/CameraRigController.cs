// Assets/Scripts/Camera/CameraRigController.cs
using UnityEngine;
using Cinemachine;

public class CameraRigController : MonoBehaviour
{
    public CinemachineFreeLook freeLook;
    public CinemachineVirtualCamera vcam; // альтернатива
    public float baseFov = 60f;
    public float sprintFov = 68f;
    public float fovLerp = 8f;

    MovementController controller;

    void Awake()
    {
        controller = GetComponentInParent<MovementController>();
        //hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!controller) return;
        float target = controller.IsSprinting ? sprintFov : baseFov;

        if (freeLook)
        {
            freeLook.m_Lens.FieldOfView = Mathf.Lerp(freeLook.m_Lens.FieldOfView, target, Time.deltaTime * fovLerp);
        }
        if (vcam)
        {
            var lens = vcam.m_Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, target, Time.deltaTime * fovLerp);
            vcam.m_Lens = lens;
        }

        // При Lock-On можно тут подтягивать камеру к цели (ограничения угла и т.д.) — заготовка
    }
}
