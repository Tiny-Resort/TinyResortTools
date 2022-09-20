using System;
using UnityEngine;

[Serializable]
internal class VehicleSavable
{
	public int vehicleId;

	public int colourVaration;

	public float[] position = new float[3];

	public float[] rotation = new float[3];

	public VehicleSavable()
	{
	}

	public VehicleSavable(Vehicle toSave)
	{
		vehicleId = toSave.saveId;
		colourVaration = toSave.getVariation();
		position[0] = toSave.transform.position.x;
		position[1] = toSave.transform.position.y;
		position[2] = toSave.transform.position.z;
		rotation[0] = toSave.transform.eulerAngles.x;
		rotation[1] = toSave.transform.eulerAngles.y;
		rotation[2] = toSave.transform.eulerAngles.z;
	}

	public Vector3 getPosition()
	{
		return new Vector3(position[0], position[1], position[2]);
	}

	public Quaternion getRotation()
	{
		return Quaternion.Euler(rotation[0], rotation[1], rotation[2]);
	}
}
