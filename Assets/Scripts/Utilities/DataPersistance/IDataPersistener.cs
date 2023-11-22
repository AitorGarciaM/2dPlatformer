using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistener
{
	DataSettings GetDataSettings();
	Data SaveData();
	void LoadData(Data data);
}
