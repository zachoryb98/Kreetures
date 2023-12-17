using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Learnable Move")]
public class LearnableItem : ItemBase
{
	[SerializeField] AttackBase attack;

	public override bool Use(Kreeture kreeture)
	{
		// Learning move is handled from Inventory UI, If it was learned then return true
		return kreeture.HasMove(attack);
	}

	public AttackBase Attack => attack;
}
