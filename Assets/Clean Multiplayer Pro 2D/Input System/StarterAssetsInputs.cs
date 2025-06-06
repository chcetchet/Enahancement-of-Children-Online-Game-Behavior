#if CMPSETUP_COMPLETE
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		public bool isPlayerWritingChat = false;

		public InputActionReference PushToTalkAction,moveAction,lookAction,jumpAction;
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool crouch;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Voice Input")]
		public bool pushToTalk;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}
		public void OnCrouch(InputValue value)
		{
            CrouchInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void EnablePushToTalk(InputAction.CallbackContext context) {
            if (isPlayerWritingChat)
                return;
            pushToTalk = true;
		}

		public void DisablePushToTalk(InputAction.CallbackContext context) {
            if (isPlayerWritingChat)
                return;
            pushToTalk = false;
        }
#endif


		public void MoveInput(Vector2 newMoveDirection) {
			if (isPlayerWritingChat)
				return;
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection) {
            if (isPlayerWritingChat)
                return;
            look = newLookDirection;
		}

		public void JumpInput(bool newJumpState) {
            if (isPlayerWritingChat)
                return;
            jump = newJumpState;
		}

		public void SprintInput(bool newSprintState) {
            if (isPlayerWritingChat)
                return;
            sprint = newSprintState;
		}
		public void CrouchInput(bool newCrouchState) {
            if (isPlayerWritingChat)
                return;
            crouch = newCrouchState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

		public void DisablePlayerInput()
		{
			moveAction.action.Disable();
			lookAction.action.Disable();
			jumpAction.action.Disable();
		}
		public void EnablePlayerInput()
		{
			moveAction.action.Enable();
			lookAction.action.Enable();
			jumpAction.action.Enable();
		}

		private void Awake() {
			PushToTalkAction.action.performed += EnablePushToTalk;
            PushToTalkAction.action.canceled += DisablePushToTalk;
        }
    }
	
}
#endif