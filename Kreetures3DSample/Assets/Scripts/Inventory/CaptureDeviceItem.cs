using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Capture Device")]
public class CaptureDeviceItem : ItemBase
{
	public override bool Use(Kreeture kreeture)
	{
		return true;
	}
}
