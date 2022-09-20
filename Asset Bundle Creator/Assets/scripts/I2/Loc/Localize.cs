using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/I2 Localize")]
	public class Localize : MonoBehaviour
	{
		public enum TermModification
		{
			DontModify = 0,
			ToUpper = 1,
			ToLower = 2,
			ToUpperFirst = 3,
			ToTitle = 4
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Predicate<UnityEngine.Object> _003C_003E9__49_0;

			public static Func<UnityEngine.Object, string> _003C_003E9__49_1;

			public static Func<IGrouping<string, UnityEngine.Object>, string> _003C_003E9__49_2;

			public static Func<IGrouping<string, UnityEngine.Object>, UnityEngine.Object> _003C_003E9__49_3;

			internal bool _003CUpdateAssetDictionary_003Eb__49_0(UnityEngine.Object x)
			{
				return x == null;
			}

			internal string _003CUpdateAssetDictionary_003Eb__49_1(UnityEngine.Object o)
			{
				return o.name;
			}

			internal string _003CUpdateAssetDictionary_003Eb__49_2(IGrouping<string, UnityEngine.Object> g)
			{
				return g.Key;
			}

			internal UnityEngine.Object _003CUpdateAssetDictionary_003Eb__49_3(IGrouping<string, UnityEngine.Object> g)
			{
				return g.First();
			}
		}

		public string mTerm = string.Empty;

		public string mTermSecondary = string.Empty;

		[NonSerialized]
		public string FinalTerm;

		[NonSerialized]
		public string FinalSecondaryTerm;

		public TermModification PrimaryTermModifier;

		public TermModification SecondaryTermModifier;

		public string TermPrefix;

		public string TermSuffix;

		public bool LocalizeOnAwake = true;

		private string LastLocalizedLanguage;

		public bool IgnoreRTL;

		public int MaxCharactersInRTL;

		public bool IgnoreNumbersInRTL = true;

		public bool CorrectAlignmentForRTL = true;

		public bool AddSpacesToJoinedLanguages;

		public bool AllowLocalizedParameters = true;

		public List<UnityEngine.Object> TranslatedObjects = new List<UnityEngine.Object>();

		[NonSerialized]
		public Dictionary<string, UnityEngine.Object> mAssetDictionary = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);

		public UnityEvent LocalizeEvent = new UnityEvent();

		public static string MainTranslation;

		public static string SecondaryTranslation;

		public static string CallBackTerm;

		public static string CallBackSecondaryTerm;

		public static Localize CurrentLocalizeComponent;

		public bool AlwaysForceLocalize;

		[SerializeField]
		public EventCallback LocalizeCallBack = new EventCallback();

		public bool mGUI_ShowReferences;

		public bool mGUI_ShowTems = true;

		public bool mGUI_ShowCallback;

		public ILocalizeTarget mLocalizeTarget;

		public string mLocalizeTargetName;

		public string Term
		{
			get
			{
				return mTerm;
			}
			set
			{
				SetTerm(value);
			}
		}

		public string SecondaryTerm
		{
			get
			{
				return mTermSecondary;
			}
			set
			{
				SetTerm(null, value);
			}
		}

		private void Awake()
		{
			UpdateAssetDictionary();
			FindTarget();
			if (LocalizeOnAwake)
			{
				OnLocalize();
			}
		}

		private void OnEnable()
		{
			OnLocalize();
		}

		public bool HasCallback()
		{
			if (LocalizeCallBack.HasCallback())
			{
				return true;
			}
			return LocalizeEvent.GetPersistentEventCount() > 0;
		}

		public void OnLocalize(bool Force = false)
		{
			if ((!Force && (!base.enabled || base.gameObject == null || !base.gameObject.activeInHierarchy)) || string.IsNullOrEmpty(LocalizationManager.CurrentLanguage) || (!AlwaysForceLocalize && !Force && !HasCallback() && LastLocalizedLanguage == LocalizationManager.CurrentLanguage))
			{
				return;
			}
			LastLocalizedLanguage = LocalizationManager.CurrentLanguage;
			if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
			{
				GetFinalTerms(out FinalTerm, out FinalSecondaryTerm);
			}
			bool flag = I2Utils.IsPlaying() && HasCallback();
			if (!flag && string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty(FinalSecondaryTerm))
			{
				return;
			}
			CallBackTerm = FinalTerm;
			CallBackSecondaryTerm = FinalSecondaryTerm;
			MainTranslation = ((string.IsNullOrEmpty(FinalTerm) || FinalTerm == "-") ? null : LocalizationManager.GetTranslation(FinalTerm, false));
			SecondaryTranslation = ((string.IsNullOrEmpty(FinalSecondaryTerm) || FinalSecondaryTerm == "-") ? null : LocalizationManager.GetTranslation(FinalSecondaryTerm, false));
			if (!flag && string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty(SecondaryTranslation))
			{
				return;
			}
			CurrentLocalizeComponent = this;
			LocalizeCallBack.Execute(this);
			LocalizeEvent.Invoke();
			LocalizationManager.ApplyLocalizationParams(ref MainTranslation, base.gameObject, AllowLocalizedParameters);
			if (!FindTarget())
			{
				return;
			}
			bool flag2 = LocalizationManager.IsRight2Left && !IgnoreRTL;
			if (MainTranslation != null)
			{
				switch (PrimaryTermModifier)
				{
				case TermModification.ToUpper:
					MainTranslation = MainTranslation.ToUpper();
					break;
				case TermModification.ToLower:
					MainTranslation = MainTranslation.ToLower();
					break;
				case TermModification.ToUpperFirst:
					MainTranslation = GoogleTranslation.UppercaseFirst(MainTranslation);
					break;
				case TermModification.ToTitle:
					MainTranslation = GoogleTranslation.TitleCase(MainTranslation);
					break;
				}
				if (!string.IsNullOrEmpty(TermPrefix))
				{
					MainTranslation = (flag2 ? (MainTranslation + TermPrefix) : (TermPrefix + MainTranslation));
				}
				if (!string.IsNullOrEmpty(TermSuffix))
				{
					MainTranslation = (flag2 ? (TermSuffix + MainTranslation) : (MainTranslation + TermSuffix));
				}
				if (AddSpacesToJoinedLanguages && LocalizationManager.HasJoinedWords && !string.IsNullOrEmpty(MainTranslation))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(MainTranslation[0]);
					int i = 1;
					for (int length = MainTranslation.Length; i < length; i++)
					{
						stringBuilder.Append(' ');
						stringBuilder.Append(MainTranslation[i]);
					}
					MainTranslation = stringBuilder.ToString();
				}
				if (flag2 && mLocalizeTarget.AllowMainTermToBeRTL() && !string.IsNullOrEmpty(MainTranslation))
				{
					MainTranslation = LocalizationManager.ApplyRTLfix(MainTranslation, MaxCharactersInRTL, IgnoreNumbersInRTL);
				}
			}
			if (SecondaryTranslation != null)
			{
				switch (SecondaryTermModifier)
				{
				case TermModification.ToUpper:
					SecondaryTranslation = SecondaryTranslation.ToUpper();
					break;
				case TermModification.ToLower:
					SecondaryTranslation = SecondaryTranslation.ToLower();
					break;
				case TermModification.ToUpperFirst:
					SecondaryTranslation = GoogleTranslation.UppercaseFirst(SecondaryTranslation);
					break;
				case TermModification.ToTitle:
					SecondaryTranslation = GoogleTranslation.TitleCase(SecondaryTranslation);
					break;
				}
				if (flag2 && mLocalizeTarget.AllowSecondTermToBeRTL() && !string.IsNullOrEmpty(SecondaryTranslation))
				{
					SecondaryTranslation = LocalizationManager.ApplyRTLfix(SecondaryTranslation);
				}
			}
			if (LocalizationManager.HighlightLocalizedTargets)
			{
				MainTranslation = "LOC:" + FinalTerm;
			}
			mLocalizeTarget.DoLocalize(this, MainTranslation, SecondaryTranslation);
			CurrentLocalizeComponent = null;
		}

		public bool FindTarget()
		{
			if (mLocalizeTarget != null && mLocalizeTarget.IsValid(this))
			{
				return true;
			}
			if (mLocalizeTarget != null)
			{
				UnityEngine.Object.DestroyImmediate(mLocalizeTarget);
				mLocalizeTarget = null;
				mLocalizeTargetName = null;
			}
			if (!string.IsNullOrEmpty(mLocalizeTargetName))
			{
				foreach (ILocalizeTargetDescriptor mLocalizeTarget in LocalizationManager.mLocalizeTargets)
				{
					if (mLocalizeTargetName == mLocalizeTarget.GetTargetType().ToString())
					{
						if (mLocalizeTarget.CanLocalize(this))
						{
							this.mLocalizeTarget = mLocalizeTarget.CreateTarget(this);
						}
						if (this.mLocalizeTarget != null)
						{
							return true;
						}
					}
				}
			}
			foreach (ILocalizeTargetDescriptor mLocalizeTarget2 in LocalizationManager.mLocalizeTargets)
			{
				if (mLocalizeTarget2.CanLocalize(this))
				{
					this.mLocalizeTarget = mLocalizeTarget2.CreateTarget(this);
					mLocalizeTargetName = mLocalizeTarget2.GetTargetType().ToString();
					if (this.mLocalizeTarget != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void GetFinalTerms(out string primaryTerm, out string secondaryTerm)
		{
			primaryTerm = string.Empty;
			secondaryTerm = string.Empty;
			if (FindTarget())
			{
				if (mLocalizeTarget != null)
				{
					mLocalizeTarget.GetFinalTerms(this, mTerm, mTermSecondary, out primaryTerm, out secondaryTerm);
					primaryTerm = I2Utils.GetValidTermName(primaryTerm);
				}
				if (!string.IsNullOrEmpty(mTerm))
				{
					primaryTerm = mTerm;
				}
				if (!string.IsNullOrEmpty(mTermSecondary))
				{
					secondaryTerm = mTermSecondary;
				}
				if (primaryTerm != null)
				{
					primaryTerm = primaryTerm.Trim();
				}
				if (secondaryTerm != null)
				{
					secondaryTerm = secondaryTerm.Trim();
				}
			}
		}

		public string GetMainTargetsText()
		{
			string primaryTerm = null;
			string secondaryTerm = null;
			if (mLocalizeTarget != null)
			{
				mLocalizeTarget.GetFinalTerms(this, null, null, out primaryTerm, out secondaryTerm);
			}
			if (!string.IsNullOrEmpty(primaryTerm))
			{
				return primaryTerm;
			}
			return mTerm;
		}

		public void SetFinalTerms(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			primaryTerm = (RemoveNonASCII ? I2Utils.GetValidTermName(Main) : Main);
			secondaryTerm = Secondary;
		}

		public void SetTerm(string primary)
		{
			if (!string.IsNullOrEmpty(primary))
			{
				FinalTerm = (mTerm = primary);
			}
			OnLocalize(true);
		}

		public void SetTerm(string primary, string secondary)
		{
			if (!string.IsNullOrEmpty(primary))
			{
				FinalTerm = (mTerm = primary);
			}
			FinalSecondaryTerm = (mTermSecondary = secondary);
			OnLocalize(true);
		}

		internal T GetSecondaryTranslatedObj<T>(ref string mainTranslation, ref string secondaryTranslation) where T : UnityEngine.Object
		{
			string value;
			string secondary;
			DeserializeTranslation(mainTranslation, out value, out secondary);
			T val = null;
			if (!string.IsNullOrEmpty(secondary))
			{
				val = GetObject<T>(secondary);
				if ((UnityEngine.Object)val != (UnityEngine.Object)null)
				{
					mainTranslation = value;
					secondaryTranslation = secondary;
				}
			}
			if ((UnityEngine.Object)val == (UnityEngine.Object)null)
			{
				val = GetObject<T>(secondaryTranslation);
			}
			return val;
		}

		public void UpdateAssetDictionary()
		{
			TranslatedObjects.RemoveAll(_003C_003Ec._003C_003E9__49_0 ?? (_003C_003Ec._003C_003E9__49_0 = _003C_003Ec._003C_003E9._003CUpdateAssetDictionary_003Eb__49_0));
			mAssetDictionary = TranslatedObjects.Distinct().GroupBy(_003C_003Ec._003C_003E9__49_1 ?? (_003C_003Ec._003C_003E9__49_1 = _003C_003Ec._003C_003E9._003CUpdateAssetDictionary_003Eb__49_1)).ToDictionary(_003C_003Ec._003C_003E9__49_2 ?? (_003C_003Ec._003C_003E9__49_2 = _003C_003Ec._003C_003E9._003CUpdateAssetDictionary_003Eb__49_2), _003C_003Ec._003C_003E9__49_3 ?? (_003C_003Ec._003C_003E9__49_3 = _003C_003Ec._003C_003E9._003CUpdateAssetDictionary_003Eb__49_3));
		}

		internal T GetObject<T>(string Translation) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(Translation))
			{
				return null;
			}
			return GetTranslatedObject<T>(Translation);
		}

		private T GetTranslatedObject<T>(string Translation) where T : UnityEngine.Object
		{
			return FindTranslatedObject<T>(Translation);
		}

		private void DeserializeTranslation(string translation, out string value, out string secondary)
		{
			if (!string.IsNullOrEmpty(translation) && translation.Length > 1 && translation[0] == '[')
			{
				int num = translation.IndexOf(']');
				if (num > 0)
				{
					secondary = translation.Substring(1, num - 1);
					value = translation.Substring(num + 1);
					return;
				}
			}
			value = translation;
			secondary = string.Empty;
		}

		public T FindTranslatedObject<T>(string value) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			if (mAssetDictionary == null || mAssetDictionary.Count != TranslatedObjects.Count)
			{
				UpdateAssetDictionary();
			}
			foreach (KeyValuePair<string, UnityEngine.Object> item in mAssetDictionary)
			{
				if (item.Value is T && value.EndsWith(item.Key, StringComparison.OrdinalIgnoreCase) && string.Compare(value, item.Key, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return (T)item.Value;
				}
			}
			T val = LocalizationManager.FindAsset(value) as T;
			if ((bool)(UnityEngine.Object)val)
			{
				return val;
			}
			return ResourceManager.pInstance.GetAsset<T>(value);
		}

		public bool HasTranslatedObject(UnityEngine.Object Obj)
		{
			if (TranslatedObjects.Contains(Obj))
			{
				return true;
			}
			return ResourceManager.pInstance.HasAsset(Obj);
		}

		public void AddTranslatedObject(UnityEngine.Object Obj)
		{
			if (!TranslatedObjects.Contains(Obj))
			{
				TranslatedObjects.Add(Obj);
				UpdateAssetDictionary();
			}
		}

		public void SetGlobalLanguage(string Language)
		{
			LocalizationManager.CurrentLanguage = Language;
		}
	}
}
