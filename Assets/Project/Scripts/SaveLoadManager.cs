using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveLoadManager
{

	public static void SavePlayerData(PlayerData player)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath + "/Player.dat";
		FileStream stream = new FileStream(path, FileMode.Create);

		formatter.Serialize(stream, player);
		stream.Close();

		Debug.Log("Player data saved!");
	}

	public static PlayerData LoadPlayerData()
	{
		string path = Application.persistentDataPath + "/Player.dat";

		if (File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Open);

			PlayerData data = formatter.Deserialize(stream) as PlayerData;
			stream.Close();

			return data;

		}
		else
		{
			Debug.Log("Player data not found in " + path);
			return null;
		}
	}
}
