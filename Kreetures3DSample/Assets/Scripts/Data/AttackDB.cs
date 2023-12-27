using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDB
{
	static Dictionary<string, AttackBase> attacks;

	public static void Init()
	{
		attacks = new Dictionary<string, AttackBase>();

		var attackList = Resources.LoadAll<AttackBase>("Attacks");
		foreach (var attack in attackList)
		{
			if (attacks.ContainsKey(attack.AttackName))
			{
				Debug.LogError($"There are two attacks with the name {attack.AttackName}");
				continue;
			}

			attacks[attack.AttackName] = attack;
		}
	}

	public static AttackBase GetAttackByName(string name)
	{
		if (!attacks.ContainsKey(name))
		{
			Debug.LogError($"Attack with name {name} not found in the database");
			return null;
		}

		return attacks[name];
	}
}