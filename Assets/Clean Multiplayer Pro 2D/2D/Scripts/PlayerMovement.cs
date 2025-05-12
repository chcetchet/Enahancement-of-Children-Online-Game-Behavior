#if CMPSETUP_COMPLETE
using Cinemachine;
using Fusion;
using StarterAssets;
using UnityEngine;

namespace AvocadoShark
{
	public class PlayerMovement : NetworkBehaviour
	{
		[SerializeField] private CinemachineVirtualCamera m_VirtualCamera;
		[SerializeField] private StarterAssetsInputs inputs;
		[SerializeField] private CharacterController2D controller;
		[SerializeField] private Animator animator;

		[SerializeField] private float runSpeed = 40f;
		float horizontalMove = 0f;

		public override void Spawned()
		{
			if (!HasStateAuthority)
			{
				controller.enabled = false;
				enabled = false;
			}
			else
			{
				m_VirtualCamera.gameObject.SetActive(true);
			}
		}
		// Update is called once per frame
		void Update()
		{

			horizontalMove = inputs.move.x * runSpeed;
			animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

			if (inputs.jump)
			{
				animator.SetBool("IsJumping", true);
			}
		}

		public void OnLanding()
		{
			animator.SetBool("IsJumping", false);
		}

		public void OnCrouching(bool isCrouching)
		{
			animator.SetBool("IsCrouching", isCrouching);
		}

		void FixedUpdate()
		{
			// Move our character
			controller.Move(horizontalMove * Time.fixedDeltaTime, inputs.crouch, inputs.jump);
			inputs.jump = false;
		}
	}
}
#endif