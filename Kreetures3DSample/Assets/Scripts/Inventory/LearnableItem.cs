using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Learnable Move")]
public class LearnableItem : ItemBase
{
	[SerializeField] AttackBase attack;
	[SerializeField] bool isHM;

	public override string ItemName => base.ItemName + $": {attack.AttackName}";

	public override bool Use(Kreeture kreeture)
	{
		// Learning move is handled from Inventory UI, If it was learned then return true
		return kreeture.HasMove(attack);
	}

	public bool CanBeTaught(Kreeture kreeture)
	{
		return kreeture.Base.LearnableByItems.Contains(attack);
	}

	public override bool IsReusable => isHM;

	public override bool CanUseInBattle => false;

	public AttackBase Attack => attack;
	public bool IsHM => isHM;
}
