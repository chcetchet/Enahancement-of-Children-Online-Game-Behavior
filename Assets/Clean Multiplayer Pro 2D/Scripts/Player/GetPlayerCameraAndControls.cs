#if CMPSETUP_COMPLETE
using UnityEngine;
using Fusion;
using StarterAssets;
using UnityEngine.InputSystem;

namespace AvocadoShark
{
    public class GetPlayerCameraAndControls : NetworkBehaviour
    {
        [SerializeField] StarterAssetsInputs AssetInputs;
        [SerializeField] PlayerInput PlayerInput;
        public bool UseMobileControls;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {

                if (UseMobileControls)
                {
                    var mobileControls = FindObjectOfType<UICanvasControllerInput>(true);
                    mobileControls.starterAssetsInputs = AssetInputs;
                    mobileControls.GetComponent<MobileDisableAutoSwitchControls>().playerInput = PlayerInput;
                }
            }
        }
    }
}
#endif
