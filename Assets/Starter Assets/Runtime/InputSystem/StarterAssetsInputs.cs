using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool walk;
		public bool sprint;
		public bool dodge;
		public bool transformation;
		public bool drawArcana;
		[SerializeField] float tapWindow = 0.2f;
		float _pressTime;
		bool _pressed;
		public bool lightAttack;
		public bool heavyAttack;
		public bool block;

		[Header("Movement Settings")]
		public bool analogMovement;
		public bool lookFromTouch;

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
				lookFromTouch = false;
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}
		
		public void OnWalk(InputValue value)
		{
			WalkInput(value.isPressed);
		}

		void Update()
		{
			if (_pressed && !sprint && Time.time - _pressTime >= tapWindow)
				sprint = true;
		}
		
		public void OnSprint(InputValue value)
		{
			if (value.isPressed)
			{
				_pressed = true;
				_pressTime = Time.time;
				return;
			}
			if (_pressed && !sprint)
				dodge = true;
			_pressed = false;
			sprint = false;
		}

		public void OnLightAttack(InputValue value)
		{
			LightAttackInput(value.isPressed);
		}
		
		public void OnHeavyAttack(InputValue value)
		{
			HeavyAttackInput(value.isPressed);
		}
		
		public void OnBlock(InputValue value)
		{
			BlockInput(value.isPressed);
		}

		public void OnTransformation(InputValue value)
		{
			TransformationInput(value.isPressed);
		}

		public void OnDrawArcana(InputValue value)
		{
			DrawArcanaInput(value.isPressed);
		}
		
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void WalkInput(bool newWalkState)
		{
			walk = newWalkState;
		}
		
		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		public void LightAttackInput(bool newLightAttackState)
		{
			lightAttack = newLightAttackState;
		}
		
		public void HeavyAttackInput(bool newHeavyAttackState)
		{
			heavyAttack = newHeavyAttackState;
		}
		
		public void BlockInput(bool newBlockState)
		{
			block = newBlockState;
		}

		public void TransformationInput(bool newTransformationState)
		{
			transformation = newTransformationState;
		}

		public void DrawArcanaInput(bool newDrawArcanaState)
		{
			drawArcana = newDrawArcanaState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}