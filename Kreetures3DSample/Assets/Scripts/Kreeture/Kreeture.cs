using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Kreeture
{

	[SerializeField] KreetureBase _base;
	[SerializeField] int level;

	public Kreeture(KreetureBase kBase, int kLevel)
	{
		_base = kBase;
		level = kLevel;

		Init();
	}

	public KreetureBase Base
	{
		get
		{
			return _base;
		}
	}
	public int Level
	{
		get
		{
			return level;
		}
	}

	public int Exp { get; set; }
	public int HP { get; set; }
	public List<Attack> Attacks { get; set; }
	public Attack CurrentAttack { get; set; }
	public Dictionary<Stat, int> Stats { get; private set; }
	public Dictionary<Stat, int> StatBoosts { get; private set; }
	public Condition Status { get; private set; }
	public int StatusTime { get; set; }
	public Condition VolatileStatus { get; private set; }
	public int VolatileStatusTime { get; set; }

	public Queue<string> StatusChanges { get; private set; } 
	public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public void Init()
	{
		// Generate Moves
		Attacks = new List<Attack>();
		foreach (var attack in Base.LearnableAttacks)
		{
			if (attack.Level <= Level)
				Attacks.Add(new Attack(attack.Base));

			if (Attacks.Count >= 4)
				break;
		}

		Exp = Base.GetExpForLevel(level);

		CalculateStats();
		HP = MaxHp;

		StatusChanges = new Queue<string>();
		ResetStatBoost();
		Status = null;
		VolatileStatus = null;
	}

	public Kreeture(KreetureSaveData saveData)
	{
		_base = KreetureDB.GetObjectByName(saveData.name);
		HP = saveData.hp;
		level = saveData.level;
		Exp = saveData.exp;

		if (saveData.statusId != null)
			Status = ConditionsDB.Conditions[saveData.statusId.Value];
		else
			Status = null;

		Attacks = saveData.attacks.Select(s => new Attack(s)).ToList();

		CalculateStats();
		StatusChanges = new Queue<string>();
		ResetStatBoost();		
		VolatileStatus = null;
	}

	public KreetureSaveData GetSaveData()
	{
		var saveData = new KreetureSaveData()
		{
			name = Base.name,
			hp = HP,
			level = Level,
			exp = Exp,
			statusId = Status?.Id,
			attacks = Attacks.Select(a => a.GetSaveData()).ToList()
		};

		return saveData;
	}

	void CalculateStats()
	{
		Stats = new Dictionary<Stat, int>();
		Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
		Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
		Stats.Add(Stat.ElementalStrike, Mathf.FloorToInt((Base.ElmtStrike * Level) / 100f) + 5);
		Stats.Add(Stat.ElementalWard, Mathf.FloorToInt((Base.ElmtWard * Level) / 100f) + 5);
		Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

		int oldMaxHP = MaxHp;
		MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;

		HP += MaxHp - oldMaxHP;
	}

	void ResetStatBoost()
	{
		StatBoosts = new Dictionary<Stat, int>()
		{
			{Stat.Attack, 0},
			{Stat.Defense, 0},
			{Stat.ElementalStrike, 0},
			{Stat.ElementalWard, 0},
			{Stat.Speed, 0},
			{Stat.Accuracy, 0},
			{Stat.Evasion, 0},
		};
	}

	int GetStat(Stat stat)
	{
		int statVal = Stats[stat];

		// Apply stat boost
		int boost = StatBoosts[stat];
		var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

		if (boost >= 0)
			statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
		else
			statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

		return statVal;
	}

	public void ApplyBoosts(List<StatBoost> statBoosts)
	{
		foreach (var statBoost in statBoosts)
		{
			var stat = statBoost.stat;
			var boost = statBoost.boost;

			StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
			
			if (boost > 0)
				StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
			else
				StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");

			Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
		}
	}

	public bool CheckForLevelUp()
	{
		if (Exp > Base.GetExpForLevel(level + 1))
		{
			++level;
			CalculateStats();
			return true;
		}

		return false;
	}

	public LearnableMove GetLearnableMoveAtCurrLevel()
	{
		return Base.LearnableAttacks.Where(x => x.Level == level).FirstOrDefault();
	}


	public void LearnMove(AttackBase moveToLearn)
	{
		if (Attacks.Count > KreetureBase.MaxNumOfMoves)
			return;

		Attacks.Add(new Attack(moveToLearn));
	}

	public bool HasMove(AttackBase moveToCheck)
	{
		return Attacks.Count(m => m.Base == moveToCheck) > 0;
	}

	public Evolution CheckForEvolution()
	{
		return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level);
	}

    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    public void Evolve(Evolution evolution)
	{
		_base = evolution.EvolvesInto;
		CalculateStats();
	}

	public int Attack
	{
		get { return GetStat(Stat.Attack); }
	}

	public int Defense
	{
		get { return GetStat(Stat.Defense); }
	}

	public int ElementalStrike
	{
		get { return Mathf.FloorToInt((Base.ElmtStrike * Level) / 100f) + 5; }
	}

	public int ElementalWard
	{
		get { return GetStat(Stat.ElementalWard); }
	}

	public int Speed
	{
		get
		{
			return GetStat(Stat.Speed);
		}
	}

	public int MaxHp { get; private set; }

	public DamageDetails TakeDamage(Attack move, Kreeture attacker)
	{
		float critical = 1f;
		if (Random.value * 100f <= 6.25f)
			critical = 2f;

		float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

		var damageDetails = new DamageDetails()
		{
			TypeEffectiveness = type,
			Critical = critical,
			Fainted = false
		};

		float attack = (move.Base.Category == MoveCategory.Special) ? attacker.ElementalStrike : attacker.Attack;
		float defense = (move.Base.Category == MoveCategory.Special) ? ElementalWard : Defense;

		float modifiers = Random.Range(0.85f, 1f) * type * critical;
		float a = (2 * attacker.Level + 10) / 250f;
		float d = a * move.Base.Power * ((float)attack / defense) + 2;
		int damage = Mathf.FloorToInt(d * modifiers);

		DecreaseHP(damage);

		return damageDetails;
	}

	public void DecreaseHP(int damage)
	{
		HP = Mathf.Clamp(HP - damage, 0, MaxHp);
		OnHPChanged?.Invoke();
	}

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionId)
	{
		if (Status != null) return;

		Status = ConditionsDB.Conditions[conditionId];
		
		Status?.OnStart?.Invoke(this);
		StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
		OnStatusChanged?.Invoke();
	}

	public void CureStatus()
	{
		Status = null;
		OnStatusChanged?.Invoke();
	}

	public void SetVolatileStatus(ConditionID conditionId)
	{
		if (VolatileStatus != null) return;

		VolatileStatus = ConditionsDB.Conditions[conditionId];
		VolatileStatus?.OnStart?.Invoke(this);
		StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
	}

	public void CureVolatileStatus()
	{
		VolatileStatus = null;
	}

	public Attack GetRandomMove()
	{
		var movesWithPP = Attacks.Where(x => x.PP > 0).ToList();

		int r = Random.Range(0, movesWithPP.Count);
		return Attacks[r];
	}

	public bool OnBeforeMove()
	{
		bool canPerformMove = true;
		if (Status?.OnBeforeMove != null)
		{
			if (!Status.OnBeforeMove(this))
				canPerformMove = false;
		}

		if (VolatileStatus?.OnBeforeMove != null)
		{
			if (!VolatileStatus.OnBeforeMove(this))
				canPerformMove = false;
		}

		return canPerformMove;
	}

	public void OnAfterTurn()
	{
		Status?.OnAfterTurn?.Invoke(this);
		VolatileStatus?.OnAfterTurn?.Invoke(this);
	}

	public void OnBattleOver()
	{
		VolatileStatus = null;
		ResetStatBoost();
	}
}

public class DamageDetails
{
	public bool Fainted { get; set; }
	public float Critical { get; set; }
	public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class KreetureSaveData
{
	public string name;
	public int hp;
	public int level;
	public int exp;
	public ConditionID? statusId;
	public List<AttackSaveData> attacks;
}