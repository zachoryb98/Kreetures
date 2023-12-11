using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KreetureParty : MonoBehaviour
{
	[SerializeField] List<Kreeture> kreetures;

	public event Action OnUpdated;

	public List<Kreeture> Kreetures
	{
		get
		{
			return kreetures;
		}
		set
		{
			kreetures = value;
			OnUpdated?.Invoke();
		}
	}

	private void Start()
	{
		foreach (var kreeture in kreetures)
		{
			kreeture.Init();
		}
	}

	public Kreeture GetHealthyKreeture()
	{
		return kreetures.Where(x => x.HP > 0).FirstOrDefault();
	}

	public void AddKreeture(Kreeture newKreeture)
	{
		if (kreetures.Count < 6)
		{
			kreetures.Add(newKreeture);
			OnUpdated?.Invoke();
		}
		else
		{
			// TODO: Add to the PC once that's implemented
		}
	}

	public static KreetureParty GetPlayerParty()
	{
		var player = GameManager.Instance.playerController;

		return player.playerParty;
	}
}
