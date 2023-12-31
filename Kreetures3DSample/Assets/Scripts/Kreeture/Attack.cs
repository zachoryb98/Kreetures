using System;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{
	public AttackBase Base { get; set; }
	public int PP { get; set; }

	public Attack(AttackBase kBase)
	{
		Base = kBase;
		PP = kBase.PP;
	}

	public Attack(AttackSaveData saveData)
	{		
		Base = AttackDB.GetObjectByName(saveData.name);
		PP = saveData.pp;
	}

	public AttackSaveData GetSaveData()
	{
		var saveData = new AttackSaveData()
		{
			name = Base.name,
			pp = PP
		};
		return saveData;
	}

	public void IncreasePP(int amount)
	{
		PP = Mathf.Clamp(PP + amount, 0, Base.PP);
	}
}

[Serializable]
public class AttackSaveData
{
	public string name;
	public int pp;
}