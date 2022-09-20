using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ES3Internal;
using UnityEngine;

public class ES3AutoSave : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Predicate<Component> _003C_003E9__13_0;

		internal bool _003COnAfterDeserialize_003Eb__13_0(Component c)
		{
			if (!(c == null))
			{
				return c.GetType() == typeof(Component);
			}
			return true;
		}
	}

	public bool saveLayer = true;

	public bool saveTag = true;

	public bool saveName = true;

	public bool saveHideFlags = true;

	public bool saveActive = true;

	public bool saveChildren;

	private bool isQuitting;

	public List<Component> componentsToSave = new List<Component>();

	public void Reset()
	{
		saveLayer = false;
		saveTag = false;
		saveName = false;
		saveHideFlags = false;
		saveActive = false;
		saveChildren = false;
	}

	public void Awake()
	{
		if (ES3AutoSaveMgr.Current == null)
		{
			ES3Debug.LogWarning("<b>No GameObjects in this scene will be autosaved</b> because there is no Easy Save 3 Manager. To add a manager to this scene, exit playmode and go to Assets > Easy Save 3 > Add Manager to Scene.", this);
		}
		else
		{
			ES3AutoSaveMgr.AddAutoSave(this);
		}
	}

	public void OnApplicationQuit()
	{
		isQuitting = true;
	}

	public void OnDestroy()
	{
		if (!isQuitting)
		{
			ES3AutoSaveMgr.RemoveAutoSave(this);
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		componentsToSave.RemoveAll(_003C_003Ec._003C_003E9__13_0 ?? (_003C_003Ec._003C_003E9__13_0 = _003C_003Ec._003C_003E9._003COnAfterDeserialize_003Eb__13_0));
	}
}
