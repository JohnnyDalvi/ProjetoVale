using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ValeTouchReceiver : MonoBehaviour
{
	public UnityEvent onTouchDown;
	public UnityEvent onTouchStay;
	public UnityEvent onTouchUp;
	public bool debugMode;
	[HideInInspector]
	public InputStatus status = InputStatus.None;
	void Awake()
	{
		status = InputStatus.None;
		onTouchDown.AddListener(() => status = InputStatus.Began);
		onTouchUp.AddListener(() => status = InputStatus.None);
		
		if (!debugMode)
			return;
		onTouchDown.AddListener(() => Debug.Log(gameObject.name + " On Touch Down Event"));
		onTouchStay.AddListener(() => Debug.Log(gameObject.name + " On Touch Stay Event"));
		onTouchUp.AddListener(() => Debug.Log(gameObject.name + " On Touch Up Event"));
	}

	void Update()
	{
		if (status == InputStatus.Holding)
			onTouchStay.Invoke();
		if (status == InputStatus.Began)
			status = InputStatus.Holding;

		// if (debugMode)
		// {
		// 	if (ValeTouchEventSystem.Instance.GetTouchDown())
		// 		Debug.Log("touch down");
		// 	if (ValeTouchEventSystem.Instance.GetTouchUp())
		// 		Debug.Log("touch up");
		// 	if (ValeTouchEventSystem.Instance.GetTouch())
		// 		Debug.Log("touch");
		// }
	}
}

public enum InputStatus
{
	Began = 0,
	Holding = 1,
	Ended = 2,
	None = 3
}