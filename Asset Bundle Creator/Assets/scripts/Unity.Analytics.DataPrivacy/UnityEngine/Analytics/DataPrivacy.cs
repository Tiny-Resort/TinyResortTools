using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine.Networking;

namespace UnityEngine.Analytics
{
	public class DataPrivacy
	{
		[Serializable]
		internal struct UserPostData
		{
			public string appid;

			public string userid;

			public long sessionid;

			public string platform;

			public uint platformid;

			public string sdk_ver;

			public bool debug_device;

			public string deviceid;

			public string plugin_ver;
		}

		[Serializable]
		internal struct TokenData
		{
			public string url;

			public string token;
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass9_0
		{
			public UnityWebRequest www;

			public Action<string> failure;

			public Action<string> success;

			internal void _003CFetchPrivacyUrl_003Eb__0(AsyncOperation async2)
			{
				string text = www.downloadHandler.text;
				if (!string.IsNullOrEmpty(www.error) || string.IsNullOrEmpty(text))
				{
					string errorString = getErrorString(www);
					if (failure != null)
					{
						failure(errorString);
					}
					return;
				}
				TokenData tokenData = default(TokenData);
				tokenData.url = "";
				try
				{
					tokenData = JsonUtility.FromJson<TokenData>(text);
				}
				catch (Exception ex)
				{
					if (failure != null)
					{
						failure(ex.ToString());
					}
				}
				success(tokenData.url);
			}
		}

		private const string kVersion = "3.0.0";

		private const string kVersionString = "DataPrivacyPackage/3.0.0";

		internal const string kBaseUrl = "https://data-optout-service.uca.cloud.unity3d.com";

		private const string kTokenUrl = "https://data-optout-service.uca.cloud.unity3d.com/token";

		internal static UserPostData GetUserData()
		{
			UserPostData result = default(UserPostData);
			result.appid = Application.cloudProjectId;
			result.userid = AnalyticsSessionInfo.userId;
			result.sessionid = AnalyticsSessionInfo.sessionId;
			result.platform = Application.platform.ToString();
			result.platformid = (uint)Application.platform;
			result.sdk_ver = Application.unityVersion;
			result.debug_device = Debug.isDebugBuild;
			result.deviceid = SystemInfo.deviceUniqueIdentifier;
			result.plugin_ver = "DataPrivacyPackage/3.0.0";
			return result;
		}

		private static string GetUserAgent()
		{
			return string.Format("UnityPlayer/{0} ({1}/{2}{3} {4})", Application.unityVersion, Application.platform.ToString(), (uint)Application.platform, Debug.isDebugBuild ? "-dev" : "", "DataPrivacyPackage/3.0.0");
		}

		private static string getErrorString(UnityWebRequest www)
		{
			string text = www.downloadHandler.text;
			string text2 = www.error;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "Empty response";
			}
			if (!string.IsNullOrEmpty(text))
			{
				text2 = text2 + ": " + text;
			}
			return text2;
		}

		public static void FetchPrivacyUrl(Action<string> success, Action<string> failure = null)
		{
			_003C_003Ec__DisplayClass9_0 _003C_003Ec__DisplayClass9_ = new _003C_003Ec__DisplayClass9_0();
			_003C_003Ec__DisplayClass9_.failure = failure;
			_003C_003Ec__DisplayClass9_.success = success;
			string s = JsonUtility.ToJson(GetUserData());
			UploadHandlerRaw uploadHandlerRaw = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
			uploadHandlerRaw.contentType = "application/json";
			_003C_003Ec__DisplayClass9_.www = UnityWebRequest.Post("https://data-optout-service.uca.cloud.unity3d.com/token", "");
			_003C_003Ec__DisplayClass9_.www.uploadHandler = uploadHandlerRaw;
			_003C_003Ec__DisplayClass9_.www.SetRequestHeader("User-Agent", GetUserAgent());
			_003C_003Ec__DisplayClass9_.www.SendWebRequest().completed += _003C_003Ec__DisplayClass9_._003CFetchPrivacyUrl_003Eb__0;
		}
	}
}
