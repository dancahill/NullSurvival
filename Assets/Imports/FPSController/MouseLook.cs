using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class MouseLook
	{
		public float XSensitivity = 2f;
		public float YSensitivity = 2f;
		public bool clampVerticalRotation = true;
		public float MinimumX = -90F;
		public float MaximumX = 90F;
		public bool smooth;
		public float smoothTime = 5f;
#if MOBILE_INPUT
		public bool lockCursor = false;
#else
		public bool lockCursor = true;
#endif
		private Quaternion m_CharacterTargetRot;
		private Quaternion m_CameraTargetRot;
		private bool m_cursorIsLocked = true;

		public void Init(Transform character, Transform camera)
		{
			m_CharacterTargetRot = character.localRotation;
			m_CameraTargetRot = camera.localRotation;
		}

		public void LookRotation(Transform character, Transform camera, Vector2 rot)
		{
			//float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
			//float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

			m_CharacterTargetRot *= Quaternion.Euler(0f, rot.y, 0f);
			m_CameraTargetRot *= Quaternion.Euler(-rot.x, 0f, 0f);

			if (clampVerticalRotation)
				m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

			if (smooth)
			{
				character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
				    smoothTime * Time.deltaTime);
				camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
				    smoothTime * Time.deltaTime);
			}
			else
			{
				character.localRotation = m_CharacterTargetRot;
				camera.localRotation = m_CameraTargetRot;
			}

			UpdateCursorLock();
		}

		public void LookRotation(Transform character, Transform camera)
		{
			Vector2 rot = new Vector2(CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity, CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity);
			LookRotation(character, camera, rot);
		}

		public void SetCursorLock(bool value)
		{
#if MOBILE_INPUT
#else
		lockCursor = value;
			if (!lockCursor)
			{//we force unlock the cursor if the user disable the cursor locking helper
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
#endif
		}

		public void UpdateCursorLock()
		{
#if MOBILE_INPUT
#else
			//if the user set "lockCursor" we check & properly lock the cursos
			if (lockCursor)
				InternalLockUpdate();
#endif
		}

		private void InternalLockUpdate()
		{
			if (CanvasManager.IsCharPanelActive()) m_cursorIsLocked = false;

			if (Input.GetKeyUp(KeyCode.Escape))
			{
				m_cursorIsLocked = false;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				m_cursorIsLocked = true;
			}

			if (m_cursorIsLocked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else if (!m_cursorIsLocked)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		Quaternion ClampRotationAroundXAxis(Quaternion q)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1.0f;

			float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

			angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

			q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

			return q;
		}

	}
}
