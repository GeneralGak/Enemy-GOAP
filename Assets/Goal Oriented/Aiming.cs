using UnityEngine;
using UnityEngine.Events;

public class Aiming : MonoBehaviour
{
	[SerializeField] protected bool invertFlipping;
	[SerializeField] protected bool disableSpriteFlipping;
	[SerializeField] protected float rotationSpeed = 25;
	[SerializeField] float overRotationDegrees = 10;
	[SerializeField] private Transform handTransform;
	[SerializeField] GameObject spriteObject;
	public UnityEvent<Vector2> onAimDirectionUpdated;

	protected int interruptCount;
	protected Quaternion targetRotation;
	protected Quaternion currentRotation;

	public bool IsFlipped { get; set; }
	public Transform HandTransform { get { return handTransform; } set { handTransform = value; } }
	public Vector2 CurrentDirection { get; protected set; }
	public Vector2 TargetDirection { get; protected set; }
	public bool SetHandRotation { get; set; } = true;
	public float RotationSpeedMultiplier { get; set; } = 1;

	// Update is called once per frame
	protected virtual void Update()
	{
		if (interruptCount > 0) { return; }

		// Smooth rotation (no instant snapping)
		currentRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * RotationSpeedMultiplier * Time.deltaTime);
		CurrentDirection = currentRotation * Vector3.right * TargetDirection.magnitude;
		onAimDirectionUpdated?.Invoke(CurrentDirection);

		if (!disableSpriteFlipping)
		{
			UpdateFlipDirection();
		}

		if (SetHandRotation)
		{
			// Inverts weapon rotation if flipped
			if (IsFlipped) // Playing looking left
			{
				// Flips weapon rotation when looking left:
				Vector3 euler = currentRotation.eulerAngles;
				handTransform.rotation = Quaternion.Euler(euler.x, euler.y, (euler.z + 180) % 360);
			}
			else // Player looking right
			{
				handTransform.rotation = currentRotation;
			}
		}
	}

	public void SetHandDirectionInstant(Vector2 _dir)
	{
		// Inverts weapon rotation if flipped
		if (IsFlipped) // Playing looking left
		{
			// Flips weapon rotation when looking left:
			Vector3 euler = Quaternion.FromToRotation(Vector2.right, _dir).eulerAngles;
			handTransform.rotation = Quaternion.Euler(euler.x, euler.y, (euler.z + 180) % 360);
		}
		else // Player looking right
		{
			handTransform.rotation = Quaternion.FromToRotation(Vector2.right, _dir);
		}
	}

	public void SetDirectionInstant(Vector2 _dir)
	{
		TargetDirection = _dir;
		CurrentDirection = _dir;
		float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
		currentRotation = Quaternion.AngleAxis(angle, Vector3.forward);
		targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
		Flip(_dir.x < 0);
	}

	public virtual void UpdateTargetDirection(Vector2 _dir)
	{
		TargetDirection = _dir;
		UpdateTargetRotation();
	}

	protected void UpdateTargetRotation()
	{
		float angle = Mathf.Atan2(TargetDirection.y, TargetDirection.x) * Mathf.Rad2Deg;
		targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	public void UpdateFlipDirection()
	{
		float zRot = currentRotation.eulerAngles.z;

		if (IsFlipped && (zRot < 90 - overRotationDegrees || zRot > 270 + overRotationDegrees))
		{
			Flip(false);
		}
		else if (!IsFlipped && ((zRot >= 90 + overRotationDegrees && zRot < 180) || (zRot <= 270 - overRotationDegrees && zRot > 180)))
		{
			Flip(true);
		}
	}

	protected void Flip(bool _lookingLeft)
	{
		if (_lookingLeft && !invertFlipping || !_lookingLeft && invertFlipping)
		{
			spriteObject.transform.localScale = new Vector3(-1, 1, 1);
		}
		else
		{
			spriteObject.transform.localScale = new Vector3(1, 1, 1);
		}
		IsFlipped = _lookingLeft;

		//if (spriteObject.transform.parent.name != "Robin Player(Clone)")
		//      {
		//          Debug.Log(spriteObject.transform.localScale);
		//      }
	}

	public void Interrupt(bool _interrupt)
	{
		if (_interrupt)
		{
			interruptCount++;
		}
		else
		{
			interruptCount--;
			if (interruptCount < 0)
				interruptCount = 0;
		}
	}

	protected virtual void ResetClass()
	{
		interruptCount = 0;
	}
}
