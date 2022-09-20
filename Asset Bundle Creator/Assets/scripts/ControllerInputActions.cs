using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class ControllerInputActions : IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct ControlsActions
	{
		private ControllerInputActions m_Wrapper;

		public InputAction Move
		{
			get
			{
				return m_Wrapper.m_Controls_Move;
			}
		}

		public InputAction Look
		{
			get
			{
				return m_Wrapper.m_Controls_Look;
			}
		}

		public InputAction TriggerLook
		{
			get
			{
				return m_Wrapper.m_Controls_TriggerLook;
			}
		}

		public InputAction SwapCamera
		{
			get
			{
				return m_Wrapper.m_Controls_SwapCamera;
			}
		}

		public InputAction Use
		{
			get
			{
				return m_Wrapper.m_Controls_Use;
			}
		}

		public InputAction Interact
		{
			get
			{
				return m_Wrapper.m_Controls_Interact;
			}
		}

		public InputAction Jump
		{
			get
			{
				return m_Wrapper.m_Controls_Jump;
			}
		}

		public InputAction Other
		{
			get
			{
				return m_Wrapper.m_Controls_Other;
			}
		}

		public InputAction RB
		{
			get
			{
				return m_Wrapper.m_Controls_RB;
			}
		}

		public InputAction LB
		{
			get
			{
				return m_Wrapper.m_Controls_LB;
			}
		}

		public InputAction Inventory
		{
			get
			{
				return m_Wrapper.m_Controls_Inventory;
			}
		}

		public InputAction Journal
		{
			get
			{
				return m_Wrapper.m_Controls_Journal;
			}
		}

		public InputAction DropItem
		{
			get
			{
				return m_Wrapper.m_Controls_DropItem;
			}
		}

		public InputAction MousePosition
		{
			get
			{
				return m_Wrapper.m_Controls_MousePosition;
			}
		}

		public InputAction SwapToController
		{
			get
			{
				return m_Wrapper.m_Controls_SwapToController;
			}
		}

		public InputAction SwapToKeyboard
		{
			get
			{
				return m_Wrapper.m_Controls_SwapToKeyboard;
			}
		}

		public InputAction VehicleAccelerate
		{
			get
			{
				return m_Wrapper.m_Controls_VehicleAccelerate;
			}
		}

		public InputAction VehicleUse
		{
			get
			{
				return m_Wrapper.m_Controls_VehicleUse;
			}
		}

		public InputAction VehicleInteract
		{
			get
			{
				return m_Wrapper.m_Controls_VehicleInteract;
			}
		}

		public InputAction OtherKeyboard
		{
			get
			{
				return m_Wrapper.m_Controls_OtherKeyboard;
			}
		}

		public InputAction NumKeys
		{
			get
			{
				return m_Wrapper.m_Controls_NumKeys;
			}
		}

		public InputAction RBKeyBoard
		{
			get
			{
				return m_Wrapper.m_Controls_RBKeyBoard;
			}
		}

		public InputAction LBKeyBoard
		{
			get
			{
				return m_Wrapper.m_Controls_LBKeyBoard;
			}
		}

		public bool enabled
		{
			get
			{
				return Get().enabled;
			}
		}

		public ControlsActions(ControllerInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Controls;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(ControlsActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IControlsActions instance)
		{
			if (m_Wrapper.m_ControlsActionsCallbackInterface != null)
			{
				Move.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnMove;
				Move.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnMove;
				Move.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnMove;
				Look.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLook;
				Look.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLook;
				Look.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLook;
				TriggerLook.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnTriggerLook;
				TriggerLook.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnTriggerLook;
				TriggerLook.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnTriggerLook;
				SwapCamera.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapCamera;
				SwapCamera.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapCamera;
				SwapCamera.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapCamera;
				Use.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnUse;
				Use.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnUse;
				Use.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnUse;
				Interact.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInteract;
				Interact.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInteract;
				Interact.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInteract;
				Jump.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJump;
				Jump.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJump;
				Jump.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJump;
				Other.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnOther;
				Other.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnOther;
				Other.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnOther;
				RB.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRB;
				RB.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRB;
				RB.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRB;
				LB.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLB;
				LB.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLB;
				LB.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLB;
				Inventory.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventory;
				Inventory.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventory;
				Inventory.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventory;
				Journal.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJournal;
				Journal.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJournal;
				Journal.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJournal;
				DropItem.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnDropItem;
				DropItem.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnDropItem;
				DropItem.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnDropItem;
				MousePosition.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnMousePosition;
				MousePosition.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnMousePosition;
				MousePosition.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnMousePosition;
				SwapToController.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapToController;
				SwapToController.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapToController;
				SwapToController.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapToController;
				SwapToKeyboard.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapToKeyboard;
				SwapToKeyboard.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapToKeyboard;
				SwapToKeyboard.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSwapToKeyboard;
				VehicleAccelerate.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleAccelerate;
				VehicleAccelerate.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleAccelerate;
				VehicleAccelerate.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleAccelerate;
				VehicleUse.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleUse;
				VehicleUse.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleUse;
				VehicleUse.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleUse;
				VehicleInteract.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleInteract;
				VehicleInteract.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleInteract;
				VehicleInteract.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnVehicleInteract;
				OtherKeyboard.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnOtherKeyboard;
				OtherKeyboard.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnOtherKeyboard;
				OtherKeyboard.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnOtherKeyboard;
				NumKeys.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnNumKeys;
				NumKeys.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnNumKeys;
				NumKeys.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnNumKeys;
				RBKeyBoard.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRBKeyBoard;
				RBKeyBoard.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRBKeyBoard;
				RBKeyBoard.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRBKeyBoard;
				LBKeyBoard.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLBKeyBoard;
				LBKeyBoard.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLBKeyBoard;
				LBKeyBoard.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnLBKeyBoard;
			}
			m_Wrapper.m_ControlsActionsCallbackInterface = instance;
			if (instance != null)
			{
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Look.started += instance.OnLook;
				Look.performed += instance.OnLook;
				Look.canceled += instance.OnLook;
				TriggerLook.started += instance.OnTriggerLook;
				TriggerLook.performed += instance.OnTriggerLook;
				TriggerLook.canceled += instance.OnTriggerLook;
				SwapCamera.started += instance.OnSwapCamera;
				SwapCamera.performed += instance.OnSwapCamera;
				SwapCamera.canceled += instance.OnSwapCamera;
				Use.started += instance.OnUse;
				Use.performed += instance.OnUse;
				Use.canceled += instance.OnUse;
				Interact.started += instance.OnInteract;
				Interact.performed += instance.OnInteract;
				Interact.canceled += instance.OnInteract;
				Jump.started += instance.OnJump;
				Jump.performed += instance.OnJump;
				Jump.canceled += instance.OnJump;
				Other.started += instance.OnOther;
				Other.performed += instance.OnOther;
				Other.canceled += instance.OnOther;
				RB.started += instance.OnRB;
				RB.performed += instance.OnRB;
				RB.canceled += instance.OnRB;
				LB.started += instance.OnLB;
				LB.performed += instance.OnLB;
				LB.canceled += instance.OnLB;
				Inventory.started += instance.OnInventory;
				Inventory.performed += instance.OnInventory;
				Inventory.canceled += instance.OnInventory;
				Journal.started += instance.OnJournal;
				Journal.performed += instance.OnJournal;
				Journal.canceled += instance.OnJournal;
				DropItem.started += instance.OnDropItem;
				DropItem.performed += instance.OnDropItem;
				DropItem.canceled += instance.OnDropItem;
				MousePosition.started += instance.OnMousePosition;
				MousePosition.performed += instance.OnMousePosition;
				MousePosition.canceled += instance.OnMousePosition;
				SwapToController.started += instance.OnSwapToController;
				SwapToController.performed += instance.OnSwapToController;
				SwapToController.canceled += instance.OnSwapToController;
				SwapToKeyboard.started += instance.OnSwapToKeyboard;
				SwapToKeyboard.performed += instance.OnSwapToKeyboard;
				SwapToKeyboard.canceled += instance.OnSwapToKeyboard;
				VehicleAccelerate.started += instance.OnVehicleAccelerate;
				VehicleAccelerate.performed += instance.OnVehicleAccelerate;
				VehicleAccelerate.canceled += instance.OnVehicleAccelerate;
				VehicleUse.started += instance.OnVehicleUse;
				VehicleUse.performed += instance.OnVehicleUse;
				VehicleUse.canceled += instance.OnVehicleUse;
				VehicleInteract.started += instance.OnVehicleInteract;
				VehicleInteract.performed += instance.OnVehicleInteract;
				VehicleInteract.canceled += instance.OnVehicleInteract;
				OtherKeyboard.started += instance.OnOtherKeyboard;
				OtherKeyboard.performed += instance.OnOtherKeyboard;
				OtherKeyboard.canceled += instance.OnOtherKeyboard;
				NumKeys.started += instance.OnNumKeys;
				NumKeys.performed += instance.OnNumKeys;
				NumKeys.canceled += instance.OnNumKeys;
				RBKeyBoard.started += instance.OnRBKeyBoard;
				RBKeyBoard.performed += instance.OnRBKeyBoard;
				RBKeyBoard.canceled += instance.OnRBKeyBoard;
				LBKeyBoard.started += instance.OnLBKeyBoard;
				LBKeyBoard.performed += instance.OnLBKeyBoard;
				LBKeyBoard.canceled += instance.OnLBKeyBoard;
			}
		}
	}

	public struct UIActions
	{
		private ControllerInputActions m_Wrapper;

		public InputAction Navigate
		{
			get
			{
				return m_Wrapper.m_UI_Navigate;
			}
		}

		public InputAction Select
		{
			get
			{
				return m_Wrapper.m_UI_Select;
			}
		}

		public InputAction UIAlt
		{
			get
			{
				return m_Wrapper.m_UI_UIAlt;
			}
		}

		public InputAction SelectActiveConfirmButton
		{
			get
			{
				return m_Wrapper.m_UI_SelectActiveConfirmButton;
			}
		}

		public InputAction Cancel
		{
			get
			{
				return m_Wrapper.m_UI_Cancel;
			}
		}

		public InputAction Point
		{
			get
			{
				return m_Wrapper.m_UI_Point;
			}
		}

		public InputAction ScrollWheel
		{
			get
			{
				return m_Wrapper.m_UI_ScrollWheel;
			}
		}

		public InputAction MiddleClick
		{
			get
			{
				return m_Wrapper.m_UI_MiddleClick;
			}
		}

		public InputAction RightClick
		{
			get
			{
				return m_Wrapper.m_UI_RightClick;
			}
		}

		public InputAction TrackedDevicePosition
		{
			get
			{
				return m_Wrapper.m_UI_TrackedDevicePosition;
			}
		}

		public InputAction TrackedDeviceOrientation
		{
			get
			{
				return m_Wrapper.m_UI_TrackedDeviceOrientation;
			}
		}

		public InputAction OpenMap
		{
			get
			{
				return m_Wrapper.m_UI_OpenMap;
			}
		}

		public InputAction OpenChat
		{
			get
			{
				return m_Wrapper.m_UI_OpenChat;
			}
		}

		public InputAction KeyboardUpperCase
		{
			get
			{
				return m_Wrapper.m_UI_KeyboardUpperCase;
			}
		}

		public bool enabled
		{
			get
			{
				return Get().enabled;
			}
		}

		public UIActions(ControllerInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterface != null)
			{
				Navigate.started -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
				Navigate.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
				Navigate.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
				Select.started -= m_Wrapper.m_UIActionsCallbackInterface.OnSelect;
				Select.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnSelect;
				Select.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnSelect;
				UIAlt.started -= m_Wrapper.m_UIActionsCallbackInterface.OnUIAlt;
				UIAlt.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnUIAlt;
				UIAlt.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnUIAlt;
				SelectActiveConfirmButton.started -= m_Wrapper.m_UIActionsCallbackInterface.OnSelectActiveConfirmButton;
				SelectActiveConfirmButton.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnSelectActiveConfirmButton;
				SelectActiveConfirmButton.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnSelectActiveConfirmButton;
				Cancel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
				Cancel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
				Cancel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
				Point.started -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
				Point.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
				Point.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
				ScrollWheel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
				ScrollWheel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
				ScrollWheel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
				MiddleClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
				MiddleClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
				MiddleClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
				RightClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
				RightClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
				RightClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
				TrackedDevicePosition.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
				TrackedDevicePosition.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
				TrackedDevicePosition.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
				TrackedDeviceOrientation.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
				OpenMap.started -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenMap;
				OpenMap.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenMap;
				OpenMap.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenMap;
				OpenChat.started -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenChat;
				OpenChat.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenChat;
				OpenChat.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenChat;
				KeyboardUpperCase.started -= m_Wrapper.m_UIActionsCallbackInterface.OnKeyboardUpperCase;
				KeyboardUpperCase.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnKeyboardUpperCase;
				KeyboardUpperCase.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnKeyboardUpperCase;
			}
			m_Wrapper.m_UIActionsCallbackInterface = instance;
			if (instance != null)
			{
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
				Select.started += instance.OnSelect;
				Select.performed += instance.OnSelect;
				Select.canceled += instance.OnSelect;
				UIAlt.started += instance.OnUIAlt;
				UIAlt.performed += instance.OnUIAlt;
				UIAlt.canceled += instance.OnUIAlt;
				SelectActiveConfirmButton.started += instance.OnSelectActiveConfirmButton;
				SelectActiveConfirmButton.performed += instance.OnSelectActiveConfirmButton;
				SelectActiveConfirmButton.canceled += instance.OnSelectActiveConfirmButton;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				ScrollWheel.started += instance.OnScrollWheel;
				ScrollWheel.performed += instance.OnScrollWheel;
				ScrollWheel.canceled += instance.OnScrollWheel;
				MiddleClick.started += instance.OnMiddleClick;
				MiddleClick.performed += instance.OnMiddleClick;
				MiddleClick.canceled += instance.OnMiddleClick;
				RightClick.started += instance.OnRightClick;
				RightClick.performed += instance.OnRightClick;
				RightClick.canceled += instance.OnRightClick;
				TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
				TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
				OpenMap.started += instance.OnOpenMap;
				OpenMap.performed += instance.OnOpenMap;
				OpenMap.canceled += instance.OnOpenMap;
				OpenChat.started += instance.OnOpenChat;
				OpenChat.performed += instance.OnOpenChat;
				OpenChat.canceled += instance.OnOpenChat;
				KeyboardUpperCase.started += instance.OnKeyboardUpperCase;
				KeyboardUpperCase.performed += instance.OnKeyboardUpperCase;
				KeyboardUpperCase.canceled += instance.OnKeyboardUpperCase;
			}
		}
	}

	public interface IControlsActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnLook(InputAction.CallbackContext context);

		void OnTriggerLook(InputAction.CallbackContext context);

		void OnSwapCamera(InputAction.CallbackContext context);

		void OnUse(InputAction.CallbackContext context);

		void OnInteract(InputAction.CallbackContext context);

		void OnJump(InputAction.CallbackContext context);

		void OnOther(InputAction.CallbackContext context);

		void OnRB(InputAction.CallbackContext context);

		void OnLB(InputAction.CallbackContext context);

		void OnInventory(InputAction.CallbackContext context);

		void OnJournal(InputAction.CallbackContext context);

		void OnDropItem(InputAction.CallbackContext context);

		void OnMousePosition(InputAction.CallbackContext context);

		void OnSwapToController(InputAction.CallbackContext context);

		void OnSwapToKeyboard(InputAction.CallbackContext context);

		void OnVehicleAccelerate(InputAction.CallbackContext context);

		void OnVehicleUse(InputAction.CallbackContext context);

		void OnVehicleInteract(InputAction.CallbackContext context);

		void OnOtherKeyboard(InputAction.CallbackContext context);

		void OnNumKeys(InputAction.CallbackContext context);

		void OnRBKeyBoard(InputAction.CallbackContext context);

		void OnLBKeyBoard(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnNavigate(InputAction.CallbackContext context);

		void OnSelect(InputAction.CallbackContext context);

		void OnUIAlt(InputAction.CallbackContext context);

		void OnSelectActiveConfirmButton(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnScrollWheel(InputAction.CallbackContext context);

		void OnMiddleClick(InputAction.CallbackContext context);

		void OnRightClick(InputAction.CallbackContext context);

		void OnTrackedDevicePosition(InputAction.CallbackContext context);

		void OnTrackedDeviceOrientation(InputAction.CallbackContext context);

		void OnOpenMap(InputAction.CallbackContext context);

		void OnOpenChat(InputAction.CallbackContext context);

		void OnKeyboardUpperCase(InputAction.CallbackContext context);
	}

	[CompilerGenerated]
	private readonly InputActionAsset _003Casset_003Ek__BackingField;

	private readonly InputActionMap m_Controls;

	private IControlsActions m_ControlsActionsCallbackInterface;

	private readonly InputAction m_Controls_Move;

	private readonly InputAction m_Controls_Look;

	private readonly InputAction m_Controls_TriggerLook;

	private readonly InputAction m_Controls_SwapCamera;

	private readonly InputAction m_Controls_Use;

	private readonly InputAction m_Controls_Interact;

	private readonly InputAction m_Controls_Jump;

	private readonly InputAction m_Controls_Other;

	private readonly InputAction m_Controls_RB;

	private readonly InputAction m_Controls_LB;

	private readonly InputAction m_Controls_Inventory;

	private readonly InputAction m_Controls_Journal;

	private readonly InputAction m_Controls_DropItem;

	private readonly InputAction m_Controls_MousePosition;

	private readonly InputAction m_Controls_SwapToController;

	private readonly InputAction m_Controls_SwapToKeyboard;

	private readonly InputAction m_Controls_VehicleAccelerate;

	private readonly InputAction m_Controls_VehicleUse;

	private readonly InputAction m_Controls_VehicleInteract;

	private readonly InputAction m_Controls_OtherKeyboard;

	private readonly InputAction m_Controls_NumKeys;

	private readonly InputAction m_Controls_RBKeyBoard;

	private readonly InputAction m_Controls_LBKeyBoard;

	private readonly InputActionMap m_UI;

	private IUIActions m_UIActionsCallbackInterface;

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Select;

	private readonly InputAction m_UI_UIAlt;

	private readonly InputAction m_UI_SelectActiveConfirmButton;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_ScrollWheel;

	private readonly InputAction m_UI_MiddleClick;

	private readonly InputAction m_UI_RightClick;

	private readonly InputAction m_UI_TrackedDevicePosition;

	private readonly InputAction m_UI_TrackedDeviceOrientation;

	private readonly InputAction m_UI_OpenMap;

	private readonly InputAction m_UI_OpenChat;

	private readonly InputAction m_UI_KeyboardUpperCase;

	private int m_KeyboardMouseSchemeIndex = -1;

	private int m_GamepadSchemeIndex = -1;

	public InputActionAsset asset
	{
		[CompilerGenerated]
		get
		{
			return _003Casset_003Ek__BackingField;
		}
	}

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes
	{
		get
		{
			return asset.controlSchemes;
		}
	}

	public ControlsActions Controls
	{
		get
		{
			return new ControlsActions(this);
		}
	}

	public UIActions UI
	{
		get
		{
			return new UIActions(this);
		}
	}

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public ControllerInputActions()
	{
		_003Casset_003Ek__BackingField = InputActionAsset.FromJson("{\n    \"name\": \"ControllerInputActions\",\n    \"maps\": [\n        {\n            \"name\": \"Controls\",\n            \"id\": \"6b9ac0aa-94bc-4d64-a6e9-163096760de6\",\n            \"actions\": [\n                {\n                    \"name\": \"Move\",\n                    \"type\": \"Value\",\n                    \"id\": \"2a2bb537-5cd4-4ef4-9d54-aed3773f422e\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Look\",\n                    \"type\": \"Value\",\n                    \"id\": \"9206a75a-1326-4904-8a52-8d684a754565\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"TriggerLook\",\n                    \"type\": \"Button\",\n                    \"id\": \"92aa74ba-02a8-4242-be4f-fc414131c849\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"SwapCamera\",\n                    \"type\": \"Button\",\n                    \"id\": \"b3018232-2f29-4877-b928-c9826286470d\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Use\",\n                    \"type\": \"Button\",\n                    \"id\": \"f6f276a4-dba0-4a67-9275-bef2b0586993\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Interact\",\n                    \"type\": \"Value\",\n                    \"id\": \"d2c8d314-6ea0-4c8b-8503-9d88d44e25aa\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Jump\",\n                    \"type\": \"Button\",\n                    \"id\": \"71906a43-45f6-4780-b019-2f8745330464\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Other\",\n                    \"type\": \"Button\",\n                    \"id\": \"390d1383-c590-45a5-937f-2774552a7b3f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"RB\",\n                    \"type\": \"Button\",\n                    \"id\": \"4f0d7701-d07e-47d4-a967-e774285a42ad\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"LB\",\n                    \"type\": \"Button\",\n                    \"id\": \"0db580a8-7679-4733-9f42-98075f6155aa\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Inventory\",\n                    \"type\": \"Button\",\n                    \"id\": \"d48dc8bd-231a-4f27-81be-c6c42be8508f\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Journal\",\n                    \"type\": \"Button\",\n                    \"id\": \"a8c6740c-677a-4dab-9170-9d20ccc7729d\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"DropItem\",\n                    \"type\": \"Button\",\n                    \"id\": \"5b769cf5-b689-4f9e-9377-35e941d4563d\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"MousePosition\",\n                    \"type\": \"Value\",\n                    \"id\": \"60f86b65-d4b5-409e-8cf4-a3f781cbbd50\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"SwapToController\",\n                    \"type\": \"Button\",\n                    \"id\": \"8239a610-c65a-4a9e-a063-995f676beeb1\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"SwapToKeyboard\",\n                    \"type\": \"Button\",\n                    \"id\": \"11c2aae1-67f8-4ef6-a681-3ca5ef2d78a8\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"VehicleAccelerate\",\n                    \"type\": \"Value\",\n                    \"id\": \"4d5e8afb-cf01-4723-8fb7-40fc3559bca5\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"VehicleUse\",\n                    \"type\": \"Button\",\n                    \"id\": \"d20dfa6e-1262-4e4f-a04e-66c53ba07681\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"VehicleInteract\",\n                    \"type\": \"Button\",\n                    \"id\": \"9871edac-36cc-4687-8769-71b46f1834d1\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"OtherKeyboard\",\n                    \"type\": \"Button\",\n                    \"id\": \"c867be90-9d40-4c41-91fd-8bfb0b89da89\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"NumKeys\",\n                    \"type\": \"Button\",\n                    \"id\": \"3a0f4186-fa68-4a59-98bf-b461160fe220\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"RBKeyBoard\",\n                    \"type\": \"Button\",\n                    \"id\": \"8b01c01d-ee87-46ca-a875-11bb77173eb7\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"LBKeyBoard\",\n                    \"type\": \"Button\",\n                    \"id\": \"0f64c989-7360-4459-b1fc-c5584fbd4565\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"978bfe49-cc26-4a3d-ab7b-7d7a29327403\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"StickDeadzone\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"WASD\",\n                    \"id\": \"00ca640b-d935-4593-8157-c05846ea39b3\",\n                    \"path\": \"Dpad\",\n                    \"interactions\": \"\",\n                    \"processors\": \"StickDeadzone(min=0.05)\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"e2062cb9-1b15-46a2-838c-2f8d72a0bdd9\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"320bffee-a40b-4347-ac70-c210eb8bc73a\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"d2581a9b-1d11-4566-b27d-b92aff5fabbc\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"fcfe95b8-67b9-4526-84b5-5d0bc98d6400\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c1f7a91b-d0fd-4a62-997e-7fb9b69bf235\",\n                    \"path\": \"<Gamepad>/rightStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"StickDeadzone\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"2D Vector\",\n                    \"id\": \"bf1b2102-fb0e-4484-8d70-38e0b2a21587\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Look\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"458f94e4-4fdc-4bd3-ace9-e779d87df441\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"da295510-4ec2-42c1-8c16-745cb36f77f4\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"347c7c82-c1d6-443d-8b68-87d328c5316b\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"d174cb6c-3c49-422b-9892-9803fb7d7385\",\n                    \"path\": \"<Keyboard>/rightArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"143bb1cd-cc10-4eca-a2f0-a3664166fe91\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Use\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"05f6913d-c316-48b2-a6bb-e225f14c7960\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Use\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"28da0391-e571-473b-8a72-259788843025\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Normalize(max=1)\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Use\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"62cebeef-9ded-4c20-832e-eda12f1b53cb\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a0f983dd-1548-4ff7-b226-be7cdced8d98\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c1183183-69ad-44bf-9ef5-bde18da78895\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Interact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"507f52fd-d93d-4c63-8ba9-bc72ebc31d0f\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Interact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c5c18188-5e75-4c50-ae82-c80503e9042c\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Normalize(max=1)\",\n                    \"groups\": \"\",\n                    \"action\": \"Interact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"becfd62e-f3f5-4688-b413-fbe710c48f2a\",\n                    \"path\": \"<Gamepad>/select\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Journal\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"34a90e62-258f-4575-ac12-7997117e53f8\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Journal\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"585a3692-815e-4554-ae74-13e1e9bb8073\",\n                    \"path\": \"<Keyboard>/j\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Journal\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"2a658c21-9194-4439-8134-23683a16059b\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Inventory\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"49f24a09-7063-40d9-9597-f099b3694328\",\n                    \"path\": \"<Keyboard>/tab\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Inventory\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"40e5a884-07a9-4ef8-bb8e-ac324ffde8ea\",\n                    \"path\": \"<Keyboard>/b\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Inventory\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"3c5e98d0-9731-4e87-9b5c-1b7957b1adbb\",\n                    \"path\": \"<Keyboard>/i\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Inventory\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"322db67c-dc20-4127-b987-ee0e5b37de7d\",\n                    \"path\": \"<Gamepad>/leftStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"DropItem\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5ac6252f-e5af-40cc-93dd-a6b0e82c7f93\",\n                    \"path\": \"<Keyboard>/q\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"DropItem\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cfb746a0-13c3-4742-b3b0-76693054b759\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Other\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ffb3f6f1-dad0-4d63-9901-f0e01e8d54c5\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Other\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a5bb8567-918c-473b-b838-88c3a55db605\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"RB\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"95aacb90-4f99-4bab-9cfd-f7632b43affb\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"LB\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cbbbe74c-bd95-439f-a4af-6fcd353ce70f\",\n                    \"path\": \"<Mouse>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"MousePosition\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8635e74f-63da-4dd8-bd5c-132d4db13e58\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"SwapToController\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f6e59457-6d0a-4bb7-b4f6-d05410db65d2\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"SwapToController\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a61f17e5-a9fe-40b8-99eb-3e865b228141\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"SwapToController\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"863c4d24-e0d5-46fa-ac2c-f737db6c4ff6\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"SwapToController\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f5ed32b4-1ffb-44c8-87b4-e86c1c03a3af\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"TriggerLook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"82fb259d-1987-4843-a0ce-4ed65c4a08e1\",\n                    \"path\": \"<Mouse>/forwardButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"TriggerLook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ddfb5a68-e1ed-4c21-a84f-aa505ca254a4\",\n                    \"path\": \"<Mouse>/backButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"TriggerLook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"68225db9-65db-4553-a5df-784ae2d6d7ff\",\n                    \"path\": \"<Keyboard>/leftShift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"TriggerLook\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d4469e66-b1bf-468a-86e9-59ff6397dd22\",\n                    \"path\": \"\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SwapCamera\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9eefa23e-13ed-4bfe-b440-95c670c40a45\",\n                    \"path\": \"<Keyboard>/z\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SwapCamera\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4a256c2b-4d6e-46bf-9d36-322f2c48de89\",\n                    \"path\": \"<Gamepad>/rightStickPress\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"SwapCamera\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a8b76da1-cd02-4ffb-af92-65235ab29afb\",\n                    \"path\": \"<Keyboard>/anyKey\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SwapToKeyboard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1f49460c-3fb8-4ad2-937d-be119f76935f\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SwapToKeyboard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"32d0fc17-f5ab-46f2-86cf-22a70f3f2a8b\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SwapToKeyboard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"46930a0b-c2ec-4624-854d-d6683f213da3\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SwapToKeyboard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Triggers\",\n                    \"id\": \"30f9aa1a-c251-4923-8b2a-bc2ca145005f\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"VehicleAccelerate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"47e66401-3c53-4c61-8b36-71226f41e6b0\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"VehicleAccelerate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"046a8e37-c8ac-46cd-b79d-85034da83337\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"VehicleAccelerate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"WASDForwardBack\",\n                    \"id\": \"04aa62e1-712e-442e-b82b-483b79af39a4\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"VehicleAccelerate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"9eb56747-ba7a-492b-88e3-44aa2ae0f8f6\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"VehicleAccelerate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"6c9006de-3a8f-4147-9a87-da8daf7dab98\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"VehicleAccelerate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7cd896ca-b7d4-4cd8-9781-a269e9d7044c\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"AxisDeadzone\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"VehicleUse\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"01b2c07f-8762-42e0-9945-b1d86d69e700\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"VehicleUse\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0987e4cc-3099-4293-b59b-2cd1429f1ce7\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"VehicleInteract\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e90992bb-78ba-4d1f-bde2-59215f6164f5\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"VehicleInteract\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5919f7f7-d186-4ec6-a87d-327d01ce3675\",\n                    \"path\": \"<Keyboard>/leftShift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"OtherKeyboard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5a20f40d-eda7-46f4-9253-95e2d4ef1e52\",\n                    \"path\": \"<Keyboard>/1\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"042a47dc-a4e0-44dd-9fb7-960662a47040\",\n                    \"path\": \"<Keyboard>/2\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=2)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f5ce2e92-b065-44f6-b154-41f6050a2e7d\",\n                    \"path\": \"<Keyboard>/3\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=3)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8d00e48a-8b36-4562-94b4-7b394bd98d6c\",\n                    \"path\": \"<Keyboard>/4\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=4)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"721fda2f-c5ca-4885-8665-db32a77b0e2e\",\n                    \"path\": \"<Keyboard>/5\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=5)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5cb18fee-32a6-4c83-945a-4285c40143c9\",\n                    \"path\": \"<Keyboard>/6\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=6)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cc75614b-1a23-43fc-902d-c7c81da72f31\",\n                    \"path\": \"<Keyboard>/7\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=7)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"82bea5b2-6292-43e4-9b71-762dedde913f\",\n                    \"path\": \"<Keyboard>/8\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=8)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7ffa266e-58bf-44a8-bfdc-66203fc3debb\",\n                    \"path\": \"<Keyboard>/9\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=9)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"12fa6f11-72c3-41d8-acaf-d1dd946ef049\",\n                    \"path\": \"<Keyboard>/0\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=10)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"0edd1be6-73f6-4bb2-bcb8-429d0f19358a\",\n                    \"path\": \"<Keyboard>/minus\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=11)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"36e7235d-a609-469e-b3db-d796ba432bae\",\n                    \"path\": \"<Keyboard>/equals\",\n                    \"interactions\": \"\",\n                    \"processors\": \"Scale(factor=12)\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"NumKeys\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f6fa4fd6-4f36-45ea-b95b-5e5203f98951\",\n                    \"path\": \"<Keyboard>/t\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"RBKeyBoard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"b604f9fc-81d6-41a3-b2c4-b5e6c8952f83\",\n                    \"path\": \"<Keyboard>/r\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"LBKeyBoard\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"UI\",\n            \"id\": \"66dc2691-a956-4bb3-9289-968c7df9dc8c\",\n            \"actions\": [\n                {\n                    \"name\": \"Navigate\",\n                    \"type\": \"Value\",\n                    \"id\": \"63f52baa-5f9e-4251-8d83-e297de762ba4\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Select\",\n                    \"type\": \"Button\",\n                    \"id\": \"848dfc3b-fc68-461d-8596-e73f0c42ac51\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"UIAlt\",\n                    \"type\": \"Button\",\n                    \"id\": \"2b17871b-7e50-495b-b060-d3e81328d54e\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"SelectActiveConfirmButton\",\n                    \"type\": \"Button\",\n                    \"id\": \"a54d8f92-bac0-46f9-9536-be6fab2d981a\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Cancel\",\n                    \"type\": \"Button\",\n                    \"id\": \"37e96650-5c5b-4966-be23-368ea05f64b2\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Point\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"cfa4c8ed-b126-4caa-80c9-227e11ebb783\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"ScrollWheel\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"46566833-5f71-4d70-a9e9-7f2dacbd7359\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"MiddleClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"5622da6f-015f-434b-8ab8-fec52504e6ed\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"RightClick\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"d83dbe3a-7267-43ec-bd31-95ecdb2a9cdc\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"TrackedDevicePosition\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"73779674-eaa9-4b10-addb-ae91a108bd87\",\n                    \"expectedControlType\": \"Vector3\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"TrackedDeviceOrientation\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"b856f2de-9c55-4a00-bff2-fa86de145de7\",\n                    \"expectedControlType\": \"Quaternion\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"OpenMap\",\n                    \"type\": \"Button\",\n                    \"id\": \"6d6f0133-c75f-4c0f-890a-507c7ba3fd97\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"OpenChat\",\n                    \"type\": \"Button\",\n                    \"id\": \"7486ba3a-bd50-4063-b963-201cb7c31792\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"KeyboardUpperCase\",\n                    \"type\": \"Button\",\n                    \"id\": \"4a016ec9-901e-4aff-b044-5a86497d3f06\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"Gamepad\",\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\n                    \"path\": \"<Gamepad>/dpad\",\n                    \"interactions\": \"\",\n                    \"processors\": \"StickDeadzone\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"3bc731f6-4ff1-4e1c-8f1c-2d8dd1bbd23c\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"StickDeadzone(min=0.25),NormalizeVector2\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Navigate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Select\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"31100a40-e5e7-4fa4-b1fb-b269e1be08cc\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Select\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d81c7cb5-cd81-4170-bcd0-d825bd2d3a9e\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Cancel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"82627dcc-3b13-4ba9-841d-e4b746d6553e\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"Cancel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4658b8f0-9869-4b9c-8bf7-91f6c7807a46\",\n                    \"path\": \"<VirtualMouse>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c52c8e0b-8179-41d3-b8a1-d149033bbe86\",\n                    \"path\": \"<Mouse>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"2448b72e-fb75-48df-8489-274ea930ea3d\",\n                    \"path\": \"<Pointer>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"Point\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"38c99815-14ea-4617-8627-164d27641299\",\n                    \"path\": \"<Mouse>/scroll\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"ScrollWheel\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"24066f69-da47-44f3-a07e-0015fb02eb2e\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"MiddleClick\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"4c191405-5738-4d4b-a523-c6a301dbf754\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"RightClick\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fb2e0e67-f92e-4da3-9dce-2781156649da\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"SelectActiveConfirmButton\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6173fecb-2af0-480c-a912-9cacb29bfde6\",\n                    \"path\": \"<Keyboard>/enter\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"SelectActiveConfirmButton\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8ef2f1cc-f114-473f-b410-67bcc0538e7f\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"UIAlt\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d42ac151-ec01-4207-95da-13ddca63628f\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"UIAlt\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"173b6c0e-c663-4db3-a796-15bdebab8a21\",\n                    \"path\": \"<Keyboard>/m\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"OpenMap\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c3fbaa5d-7a07-4e18-8aa3-e038abea2be2\",\n                    \"path\": \"<Gamepad>/dpad/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"OpenMap\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"2c92921a-9e47-4b58-9a15-70f14e94cbd6\",\n                    \"path\": \"<Keyboard>/enter\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Keyboard&Mouse\",\n                    \"action\": \"OpenChat\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a690eb43-2531-4238-86d0-337ee8f3e949\",\n                    \"path\": \"<Gamepad>/dpad/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"OpenChat\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f1692268-4841-409a-9590-ca4bbd1643c6\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"KeyboardUpperCase\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"a5a81860-448d-4ed5-a12a-90f106b7c9fa\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"Gamepad\",\n                    \"action\": \"KeyboardUpperCase\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        }\n    ],\n    \"controlSchemes\": [\n        {\n            \"name\": \"Keyboard&Mouse\",\n            \"bindingGroup\": \"Keyboard&Mouse\",\n            \"devices\": [\n                {\n                    \"devicePath\": \"<Mouse>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                },\n                {\n                    \"devicePath\": \"<Keyboard>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Gamepad\",\n            \"bindingGroup\": \"Gamepad\",\n            \"devices\": [\n                {\n                    \"devicePath\": \"<Gamepad>\",\n                    \"isOptional\": false,\n                    \"isOR\": false\n                }\n            ]\n        }\n    ]\n}");
		m_Controls = asset.FindActionMap("Controls", true);
		m_Controls_Move = m_Controls.FindAction("Move", true);
		m_Controls_Look = m_Controls.FindAction("Look", true);
		m_Controls_TriggerLook = m_Controls.FindAction("TriggerLook", true);
		m_Controls_SwapCamera = m_Controls.FindAction("SwapCamera", true);
		m_Controls_Use = m_Controls.FindAction("Use", true);
		m_Controls_Interact = m_Controls.FindAction("Interact", true);
		m_Controls_Jump = m_Controls.FindAction("Jump", true);
		m_Controls_Other = m_Controls.FindAction("Other", true);
		m_Controls_RB = m_Controls.FindAction("RB", true);
		m_Controls_LB = m_Controls.FindAction("LB", true);
		m_Controls_Inventory = m_Controls.FindAction("Inventory", true);
		m_Controls_Journal = m_Controls.FindAction("Journal", true);
		m_Controls_DropItem = m_Controls.FindAction("DropItem", true);
		m_Controls_MousePosition = m_Controls.FindAction("MousePosition", true);
		m_Controls_SwapToController = m_Controls.FindAction("SwapToController", true);
		m_Controls_SwapToKeyboard = m_Controls.FindAction("SwapToKeyboard", true);
		m_Controls_VehicleAccelerate = m_Controls.FindAction("VehicleAccelerate", true);
		m_Controls_VehicleUse = m_Controls.FindAction("VehicleUse", true);
		m_Controls_VehicleInteract = m_Controls.FindAction("VehicleInteract", true);
		m_Controls_OtherKeyboard = m_Controls.FindAction("OtherKeyboard", true);
		m_Controls_NumKeys = m_Controls.FindAction("NumKeys", true);
		m_Controls_RBKeyBoard = m_Controls.FindAction("RBKeyBoard", true);
		m_Controls_LBKeyBoard = m_Controls.FindAction("LBKeyBoard", true);
		m_UI = asset.FindActionMap("UI", true);
		m_UI_Navigate = m_UI.FindAction("Navigate", true);
		m_UI_Select = m_UI.FindAction("Select", true);
		m_UI_UIAlt = m_UI.FindAction("UIAlt", true);
		m_UI_SelectActiveConfirmButton = m_UI.FindAction("SelectActiveConfirmButton", true);
		m_UI_Cancel = m_UI.FindAction("Cancel", true);
		m_UI_Point = m_UI.FindAction("Point", true);
		m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", true);
		m_UI_MiddleClick = m_UI.FindAction("MiddleClick", true);
		m_UI_RightClick = m_UI.FindAction("RightClick", true);
		m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", true);
		m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", true);
		m_UI_OpenMap = m_UI.FindAction("OpenMap", true);
		m_UI_OpenChat = m_UI.FindAction("OpenChat", true);
		m_UI_KeyboardUpperCase = m_UI.FindAction("KeyboardUpperCase", true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}
}
