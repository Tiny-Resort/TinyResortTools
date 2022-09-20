using System;
using System.ComponentModel;

[Serializable]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ES3SerializableSettings : ES3Settings
{
	public ES3SerializableSettings()
		: base(false)
	{
	}

	public ES3SerializableSettings(bool applyDefaults)
		: base(applyDefaults)
	{
	}

	public ES3SerializableSettings(string path)
		: base(false)
	{
		base.path = path;
	}

	public ES3SerializableSettings(string path, ES3.Location location)
		: base(false)
	{
		base.location = location;
	}
}
