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

	private void Awake()
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

	public Dictionary<Kreeture, Evolution> CheckForEvolutions()
	{

		Dictionary<Kreeture, Evolution> evolutions =  new Dictionary<Kreeture, Evolution>();

		foreach(var kreeture in kreetures)
		{
			var evolution = kreeture.CheckForEvolution();
			if(evolution != null)
			{

				//Maybe instead here just change scenes, then in the start method kick off evolution things?
				//yield return EvolutionManager.i.Evolve(kreeture, evolution);				
				evolutions.Add(kreeture, evolution);
			}
		}

		return evolutions;
    }

	public void PartyUpdated()
	{
		OnUpdated?.Invoke();
	}

	public static KreetureParty GetPlayerParty()
	{
		var player = GameManager.Instance.playerController;

		return player.playerParty;
	}
}
