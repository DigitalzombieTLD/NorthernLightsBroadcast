using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using AudioMgr;
using Il2CppInterop.Runtime;


namespace NorthernLightsBroadcast
{
    [RegisterTypeInIl2Cpp]
    public class TVButton : MonoBehaviour
    {
        public TVManager manager;
        public Shot tvClickShot;
        public MeshRenderer meshRenderer;
        public Color32 emissionColorOn = new Color32(106, 7, 7, 255);
        public Color32 emissionColorOff = new Color32(0, 0, 0, 0);
        public bool isMoving = false;
        public bool isGlowing = false;
        public Vector3 outPosition;
        public Vector3 inPosition;
        public bool isSetup = false;

        public TVButton(IntPtr intPtr) : base(intPtr)
        {
        }

        public void Awake()
        {
            if (isSetup)
            {
                return;
            }

            tvClickShot = AudioMaster.CreateShot(this.gameObject, AudioMaster.SourceType.SFX);
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", emissionColorOff);
            outPosition = transform.localPosition;
            inPosition = new Vector3(outPosition.x, outPosition.y, outPosition.z - 0.006f);

            isSetup = true;
        }

        [HideFromIl2Cpp]
        public IEnumerator PressButtonAnimation (float speed)
        {
            //yield return new WaitForSeconds(waitTime);
            isMoving = true;

            System.Action<ITween<Vector3>> updateMoveIn = (t) =>
            {
                manager.redbutton.transform.localPosition = t.CurrentValue;
                
            };

            System.Action<ITween<Vector3>> moveInCompleted = (t) =>
            {
                manager.redbutton.transform.localPosition = inPosition;
                tvClickShot.PlayOneshot(NorthernLightsBroadcastMain.tvAudioManager.GetClip("click"));
                Glow(!isGlowing);

                if (manager.currentState == TVManager.TVState.Off)
                {
                    manager.SwitchState(TVManager.TVState.Static);
                }
                else
                {
                    manager.SwitchState(TVManager.TVState.Off);
                }
            };

            System.Action<ITween<Vector3>> updateMoveOut = (t) =>
            {
                manager.redbutton.transform.localPosition = t.CurrentValue;               
            };

            System.Action<ITween<Vector3>> moveOutCompleted = (t) =>
            {
                manager.redbutton.transform.localPosition = outPosition;
                isMoving = false;
            };

            manager.redbutton.gameObject.Tween(manager.redbutton.gameObject, outPosition, inPosition, speed, TweenScaleFunctions.SineEaseInOut, updateMoveIn, moveInCompleted)
            .ContinueWith(new Vector3Tween().Setup(inPosition, outPosition, speed, TweenScaleFunctions.SineEaseInOut, updateMoveOut, moveOutCompleted));

            yield return null;
        }

        [HideFromIl2Cpp]
        public void Glow(bool enabled)
        {
            if(enabled)
            {
                meshRenderer.material.EnableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", emissionColorOn);
                isGlowing = true;
            }
            else
            {
                meshRenderer.material.DisableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", emissionColorOff);
                isGlowing = false;
            }
        }


        [HideFromIl2Cpp]
        public void TogglePower()
        {
            if (isMoving)
            {
                return;
            }
            
            MelonCoroutines.Start(PressButtonAnimation(0.5f));
        }
    }
}