using System;

[Serializable]
public class PlayerData
{
	public string id;
	public float elapsedTime;

	public PlayerData(string id)
	{
		this.id = id;
	}
}
