using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using System.Reflection;
using Il2Cpp;

namespace NorthernLightsBroadcast
{
	public static class TVLock
	{
        public static float m_StartCameraFOV;
        public static Vector2 m_StartPitchLimit;
        public static Vector2 m_StartYawLimit;
        public static Vector3 m_StartPlayerPosition;
        
        public static float m_StartAngleX;
        public static float m_StartAngleY;

        public static TVManager currentManager;
        public static bool lockedInTVView = false;
 

        public static void ToggleTVView(TVManager tvManager)
        {
            if (lockedInTVView)
            {
                ExitTVView();
            }
            else
            {
                EnterTVView(tvManager);
            }
        }

        public static void EnterTVView(TVManager tvManager)
        {
            if (tvManager.currentState == TVManager.TVState.Off)
            {
                return;
            }

            currentManager = tvManager;
            lockedInTVView= true;
            tvManager.ui.canvas.worldCamera = NorthernLightsBroadcastMain.eventCam;                 

            //CameraFade.StartAlphaFade(Color.black, true, 1, 0f, null);
            m_StartCameraFOV = GameManager.GetMainCamera().fieldOfView;
            m_StartPitchLimit = GameManager.GetVpFPSCamera().RotationPitchLimit;
            m_StartYawLimit = GameManager.GetVpFPSCamera().RotationYawLimit;
            m_StartPlayerPosition = GameManager.GetVpFPSPlayer().transform.position;

            m_StartAngleX = GameManager.GetVpFPSPlayer().transform.rotation.eulerAngles.x;
            m_StartAngleY = GameManager.GetVpFPSPlayer().transform.rotation.eulerAngles.y;            

            GameManager.GetPlayerManagerComponent().SetControlMode(PlayerControlMode.InVehicle);
            GameManager.GetPlayerManagerComponent().TeleportPlayer(tvManager.dummyCamera.transform.position - GameManager.GetVpFPSCamera().PositionOffset, tvManager.dummyCamera.transform.rotation);

            GameManager.GetVpFPSCamera().transform.position = tvManager.dummyCamera.transform.position;
            GameManager.GetVpFPSCamera().transform.localPosition = GameManager.GetVpFPSCamera().PositionOffset;
            GameManager.GetVpFPSCamera().SetAngle(tvManager.dummyCamera.transform.rotation.eulerAngles.y, tvManager.dummyCamera.transform.rotation.eulerAngles.x);
            GameManager.GetVpFPSCamera().SetPitchLimit(new Vector2(0, 0));
            GameManager.GetVpFPSCamera().SetFOVFromOptions(50); // 40
            GameManager.GetVpFPSCamera().SetNearPlaneOverride(0.001f);
            //GameManager.GetVpFPSCamera().m_Camera.nearClipPlane = 0.001f;

            GameManager.GetVpFPSCamera().SetYawLimit(tvManager.dummyCamera.transform.rotation, new Vector2(0, 0));
            GameManager.GetVpFPSCamera().LockRotationLimit();

            tvManager.objectRenderer.enabled = true;
        }

        public static void ExitTVView()
        {
            if(lockedInTVView == false)
            {
                return;
            }

            GameManager.GetVpFPSCamera().m_PanViewCamera.m_IsDetachedFromPlayer = false;
            GameManager.GetPlayerManagerComponent().SetControlMode(PlayerControlMode.Normal);
            GameManager.GetVpFPSCamera().UnlockRotationLimit();

            GameManager.GetVpFPSCamera().RotationPitchLimit = m_StartPitchLimit;
            GameManager.GetVpFPSCamera().RotationYawLimit = m_StartYawLimit;
            GameManager.GetVpFPSPlayer().transform.position = m_StartPlayerPosition;
            GameManager.GetVpFPSCamera().transform.localPosition = GameManager.GetVpFPSCamera().PositionOffset;
            GameManager.GetVpFPSCamera().SetAngle(m_StartAngleY, m_StartAngleX);
            GameManager.GetVpFPSCamera().SetFOVFromOptions(m_StartCameraFOV);
            GameManager.GetVpFPSCamera().UpdateCameraRotation();
            GameManager.GetPlayerManagerComponent().StickPlayerToGround();

            currentManager.ui.ActivateOSD(false);
            currentManager = null;
            lockedInTVView = false;
        }
    }
}