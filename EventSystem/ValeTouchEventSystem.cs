using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ValeTouchEventSystem : MonoBehaviour
{
	bool isWindowMode = false;
	public static ValeTouchEventSystem Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType<ValeTouchEventSystem>();
			if (_instance == null)
				_instance = (Resources.Load("Vale Event System") as GameObject).GetComponent<ValeTouchEventSystem>();
			return _instance;
		}
	}
	public static ValeTouchEventSystem _instance;

	public bool touchEnabled = false;
	public Camera raycastCamera;
	public Vector3 mousetipSize = new Vector3(0.1f,0.1f,0.1f);
	public LayerMask layers;
	ValeTouchReceiver activatedReceiver;
	[Header("Essa variável só vai ser utilizada na build final, não precisa setar.")]
	public RectTransform windowPanel;
	InputStatus status;
	int touchID;

	public Vector3 mousePosition
	{
		get
		{
			if (screenBounds==null && isWindowMode)
				screenBounds = new Bounds(windowPanel.transform.position, new Vector3(windowPanel.rect.size.x,windowPanel.rect.size.y,100));
			if (touchEnabled&&isWindowMode)
				return LocalizeScreenPosition(activeTouch.position);
			return isWindowMode?LocalizeScreenPosition(Input.mousePosition):Input.mousePosition;
		}
	}

	Touch activeTouch;
	bool hasActiveTouch = false;

	Bounds? screenBounds;
	void Awake()
	{
		isWindowMode = raycastCamera != Camera.main;
	}

	void Update()
	{
		hasActiveTouch = false;
		ValeTouchReceiver newlyActivatedReceiver = null;
		if (isWindowMode)
		{
			UpdateScreenRelationship();
			if (touchEnabled)
			{
				foreach (Touch touch in Input.touches)
				{
					if (!screenBounds.Value.Contains(touch.position))
						continue;
					if (!hasActiveTouch)
					{
						activeTouch = touch;
						hasActiveTouch = true;
					}
					if (touch.phase==TouchPhase.Began)
						newlyActivatedReceiver = RaycastForReceiver(LocalizeScreenPosition(touch.position));
					if (newlyActivatedReceiver!=null)
					{
						touchID = touch.fingerId;
						break;
					}
				}
			}
			else if (Input.GetMouseButtonDown(0))
			{
				if (screenBounds.Value.Contains(Input.mousePosition))
					newlyActivatedReceiver = RaycastForReceiver(mousePosition);
			}
		}
		else
		{
			if (touchEnabled)
			{
				foreach (Touch touch in Input.touches)
				{
					if (!hasActiveTouch)
					{
						activeTouch = touch;
						hasActiveTouch = true;
					}
					if (touch.phase==TouchPhase.Began)
						newlyActivatedReceiver = RaycastForReceiver(touch.position);
					if (newlyActivatedReceiver!=null)
					{
						touchID = touch.fingerId;
						break;
					}
				}
			}
			else if (Input.GetMouseButtonDown(0))
			{
				newlyActivatedReceiver = RaycastForReceiver(Input.mousePosition);
			}
		}

		ProcessTouchEventsOnReceivers(newlyActivatedReceiver);
		UpdateTouchGlobalEvents();
	}

	public bool GetTouchDown()
	{
		return status == InputStatus.Began;
	}

	public bool GetTouchUp()
	{
		return status == InputStatus.Ended;
	}

	public bool GetTouch()
	{
		return status == InputStatus.Holding;
	}

	void UpdateTouchGlobalEvents()
	{
		status = InputStatus.None;
		if (touchEnabled)
		{
			foreach	(Touch touch in Input.touches)
			{
				if ((isWindowMode && screenBounds.Value.Contains(touch.position))||!isWindowMode)
				{
					switch(touch.phase)
					{
						case TouchPhase.Ended:
							status = InputStatus.Ended;
							return;
						case TouchPhase.Began:
							status = InputStatus.Began;
							return;
						default:
							status = InputStatus.Holding;
							return;
					}
				}
			}
		}
		else
		{
			if (isWindowMode && screenBounds.Value.Contains(Input.mousePosition)||!isWindowMode)
			{
				if (Input.GetMouseButtonDown(0))
					status = InputStatus.Began;

				else if (Input.GetMouseButtonUp(0))
					status = InputStatus.Ended;

				else if (Input.GetMouseButton(0))
					status = InputStatus.Holding;
			}
		}
	}

	void UpdateScreenRelationship()
	{
		screenBounds = new Bounds(windowPanel.transform.position, new Vector3(windowPanel.rect.size.x,windowPanel.rect.size.y,100));
		//Debug.Log(screenBounds);
	}

	void ProcessTouchEventsOnReceivers(ValeTouchReceiver newlyActivatedReceiver)
	{
		if (newlyActivatedReceiver!=null)
			newlyActivatedReceiver.onTouchDown.Invoke();


		if (activatedReceiver!=null)
		{
			if (touchEnabled)
			{
				foreach(Touch touch in Input.touches)
				{
					if (touch.fingerId == touchID)
					{
						if (touch.phase == TouchPhase.Ended)
						{
							activatedReceiver.onTouchUp.Invoke();
							activatedReceiver = newlyActivatedReceiver;
						}
					}
				}
			}
			else
			{
				if (Input.GetMouseButtonUp(0))
				{
					activatedReceiver.onTouchUp.Invoke();
					activatedReceiver = newlyActivatedReceiver;					
				}
			}
		}
		if (activatedReceiver == null && newlyActivatedReceiver!=null)
			activatedReceiver = newlyActivatedReceiver;
	}

	public Vector3 LocalizeScreenPosition(Vector3 screenPosition)
	{
		if (isWindowMode)
			return screenPosition - new Vector3(screenBounds.Value.min.x, screenBounds.Value.min.y, 0);
		else return screenPosition;
	}

	ValeTouchReceiver RaycastForReceiver(Vector3 localizedScreenPosition)
	{
		//Debug.Log(localizedScreenPosition);
		Vector3 mouseWorldPos = raycastCamera.ScreenToWorldPoint(localizedScreenPosition + Vector3.forward*raycastCamera.nearClipPlane);
		//Debug.Log(mouseWorldPos);
		Collider2D collider = Physics2D.OverlapArea(mouseWorldPos - mousetipSize / 2, mouseWorldPos + mousetipSize / 2,layers);
		if (collider == null)
			return null;
		ValeTouchReceiver hitMouseInteractable = collider.GetComponent<ValeTouchReceiver>();
		if (hitMouseInteractable != null)
		{
			return hitMouseInteractable;
		}
		return null;
	}
}