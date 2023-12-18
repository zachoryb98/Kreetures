using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Capture Device")]
public class CaptureDeviceItem : ItemBase
{
	[SerializeField] float catchRateModifier = 1;

	public override bool Use(Kreeture kreeture)
	{
		return true;
	}

	public override bool CanUseOutsideBattle => false;

	public float CatchRateModifier => catchRateModifier;
}
