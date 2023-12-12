using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
	[Header("HP")]
	[SerializeField] int hpAmount;
	[SerializeField] bool restoreMaxHP;

	[Header("PP")]
	[SerializeField] int ppAmount;
	[SerializeField] bool restoreMaxPP;

	[Header("Status Conditions")]
	[SerializeField] ConditionID status;
	[SerializeField] bool recoverAllStatus;

	[Header("Revive")]
	[SerializeField] bool revive;
	[SerializeField] bool maxRevive;

	public override bool Use(Kreeture kreeture)
	{
		// Revive
		if (revive || maxRevive)
		{
			if (kreeture.HP > 0)
				return false;

			if (revive)
				kreeture.IncreaseHP(kreeture.MaxHp / 2);
			else if (maxRevive)
				kreeture.IncreaseHP(kreeture.MaxHp);

			kreeture.CureStatus();

			return true;
		}

		// No other items can be used on fainted pokemon
		if (kreeture.HP == 0)
			return false;

		// Restore HP
		if (restoreMaxHP || hpAmount > 0)
		{
			if (kreeture.HP == kreeture.MaxHp)
				return false;

			if (restoreMaxHP)
				kreeture.IncreaseHP(kreeture.MaxHp);
			else
				kreeture.IncreaseHP(hpAmount);
		}

		// Recover Status
		if (recoverAllStatus || status != ConditionID.none)
		{
			if (kreeture.Status == null && kreeture.VolatileStatus != null)
				return false;

			if (recoverAllStatus)
			{
				kreeture.CureStatus();
				kreeture.CureVolatileStatus();
			}
			else
			{
				if (kreeture.Status.Id == status)
					kreeture.CureStatus();
				else if (kreeture.VolatileStatus.Id == status)
					kreeture.CureVolatileStatus();
				else
					return false;
			}
		}

		// Restore PP
		if (restoreMaxPP)
		{
			kreeture.Attacks.ForEach(m => m.IncreasePP(m.Base.PP));
		}
		else if (ppAmount > 0)
		{
			kreeture.Attacks.ForEach(m => m.IncreasePP(ppAmount));
		}

		return true;
	}
}
