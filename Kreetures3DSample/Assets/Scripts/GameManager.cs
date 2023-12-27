using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog, Menu, Cutscene, Bag, Paused, PartyScreen }

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }// Singleton instance            

	[SerializeField] public PartyScreen partyScreen;
	[SerializeField] public InventoryUI inventoryUI;

	//Wild creature
	private Kreeture wildKreeture { get; set; }

	[SerializeField] public PlayerController playerController;
	[SerializeField] public TrainerController trainerController;

	// Player-related data
	public KreetureParty playerTeam = new KreetureParty();
	public KreetureParty enemyTrainerParty = new KreetureParty();
	private bool enterTrainerBattle = false;

	private string previousSceneName;

	MenuController menuController;

	public bool playerDefeated = false;

	private Vector3 playerPosition = new Vector3();
	private Quaternion playerRotation = new Quaternion();

	public GameState state;
    public GameState prevState;

    List<SavableEntity> savableEntities;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;			
			var spawner = FindObjectOfType<PlayerSpawnChecker>();
			spawner.SpawnOnCommand();
			KreetureDB.Init();
			ConditionsDB.Init();
			AttackDB.Init();
			partyScreen.Init();
			ItemDB.Init();
			QuestDB.Init();

			menuController = FindObjectOfType<MenuController>();

			//Cursor.lockState = CursorLockMode.Locked;
			//Cursor.visible = false;

			DontDestroyOnLoad(gameObject); // Keep the GameManager object when changing scenes
		}
		else
		{
			Destroy(gameObject); // Destroy any duplicate GameManager instances
		}
	}

	private void Update()
	{
		if (state == GameState.FreeRoam)
		{
			if (Input.GetKeyDown(KeyCode.L))
			{
				SavingSystem.i.Load("saveSlot1");
			}

			//Maybe move this to player controller? 
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				menuController.OpenMenu();
				state = GameState.Menu;
			}
		}

		if (state == GameState.Menu)
		{
			menuController.HandleUpdate();
		}

		if (state == GameState.PartyScreen)
		{
			Action onSelected = () =>
			{
				//TODO: Go to Summary Screen
			};

			Action onBack = () =>
			{
				partyScreen.gameObject.SetActive(false);
				state = GameState.FreeRoam;
			};

			partyScreen.HandleUpdate(onSelected, onBack);
		}
		if(state == GameState.Bag)
		{
			Action onBack = () =>
			{
				inventoryUI.gameObject.SetActive(false);
				state = GameState.FreeRoam;
			};
			inventoryUI.HandleUpdate(onBack);
		}
	}

	public void SetTrainerLoss()
	{
		trainerController.BattleLost();
		SetIsTrainerBattle(false);
		playerController.SetContinueDialog(false);
	}

	public Kreeture GetWildKreeture()
	{
		return wildKreeture;
	}

	private void Start()
	{
		DialogManager.Instance.OnShowDialog += () =>
		{
			prevState = state;
			state = GameState.Dialog;
			playerController.DisablePlayerControls();
			playerController.EnableUIControls();
		};

		DialogManager.Instance.OnDialogFinished += () =>
		{
			if (state == GameState.Dialog)
				state = prevState;
			playerController.EnablePlayerControls();
			playerController.DisableUIControls();
			if (enterTrainerBattle)
			{
				if (trainerController != null)
				{
					PersistentObjectManager.Instance.RegisterObject(trainerController.gameObject);
					TransitionToTrainerBattle(trainerController.getSceneToLoad());
				}
			}
		};

		menuController.onBack += () =>
		{
			state = GameState.FreeRoam;
		};

		menuController.onMenuSelected += OnMenuSelected;
	}

	public void TransitionToBattle(string sceneToLoad)
	{
		playerController.gameObject.SetActive(false);
		enterTrainerBattle = false;
		OpenBattleScene(sceneToLoad);
		state = GameState.Battle;
	}

	public void TransitionToTrainerBattle(string _sceneToLoad)
	{
		playerController.gameObject.SetActive(false);
		OpenBattleScene(_sceneToLoad);
		state = GameState.Battle;
	}

	public TrainerController GetTrainer()
	{
		return trainerController;
	}

	public bool GetIsTrainerBattle()
	{
		return enterTrainerBattle;
	}

	public void SetIsTrainerBattle(bool result)
	{
		enterTrainerBattle = result;
	}

	internal void SetWildKreeture(Kreeture _wildKreeture)
	{
		wildKreeture = _wildKreeture;
	}

	public KreetureParty GetPlayerTeam()
	{
		return playerTeam;
	}

	public KreetureParty GetEnemyTeam()
	{
		return enemyTrainerParty;
	}

	public void SetEnemyTeam(KreetureParty party)
	{
		enemyTrainerParty = party;
	}

	public void SetPlayerTeam(KreetureParty party)
	{
		playerTeam = party;
	}

	public void SetPlayer(PlayerController _playerController)
	{
		playerController = _playerController;
	}

	public void SetPreviousScene(string sceneName)
	{
		previousSceneName = sceneName;
	}

	public string GetPreviousScene()
	{
		return previousSceneName;
	}

	public void SetPlayerPosition(Vector3 position, Quaternion rotation)
	{
		playerPosition = position;
		playerRotation = rotation;
	}

	public Vector3 GetPlayerPosition()
	{
		return playerPosition;
	}

	public Quaternion GetPlayerRotation()
	{
		return playerRotation;
	}

	public string GetLastHealScene()
	{
		return PlayerPrefs.GetString("playerSpawnScene");
	}

	public void SetLastHealScene(string healScene)
	{
		PlayerPrefs.SetString("playerSpawnScene", healScene);
	}

	public Vector3 GetPlayerLastHealPosition()
	{
		Vector3 lastHealLocation = new Vector3();
		lastHealLocation.x = PlayerPrefs.GetFloat("playerHealPositionX");
		lastHealLocation.y = PlayerPrefs.GetFloat("playerHealPositionY");
		lastHealLocation.z = PlayerPrefs.GetFloat("playerHealPositionZ");

		return lastHealLocation;
	}

	public void SetPlayerLastHealLocation(Vector3 currentPosition)
	{
		PlayerPrefs.SetFloat("playerHealPositionX", currentPosition.x);
		PlayerPrefs.SetFloat("playerHealPositionY", currentPosition.y);
		PlayerPrefs.SetFloat("playerHealPositionZ", currentPosition.z);
	}

	public GameObject GetPlayerController()
	{
		GameObject playerController = GameObject.Find("Player");

		return playerController;
	}

	public void OpenBattleScene(string sceneToLoad)
	{
		Instance.SetPreviousScene(SceneManager.GetActiveScene().name);

		SaveOnDemand();

		SceneManager.LoadScene(sceneToLoad);
	}

	public void SaveOnDemand()
	{
		savableEntities = GetSavableEntitiesInScene();
		SavingSystem.i.CaptureEntityStates(savableEntities);
	}

	List<SavableEntity> GetSavableEntitiesInScene()
	{
		//CURRENTLY ONLY WORKS FOR TRAINERS
		var trainerControllers = FindObjectsOfType<TrainerController>();

		List<SavableEntity> savableEntities = trainerControllers
			.Select(trainer => trainer.GetComponent<SavableEntity>())
			.Where(savableEntity => savableEntity != null)
			.ToList();

		return savableEntities;
	}

	public void RestoreSavableEntities()
	{
		savableEntities = GetSavableEntitiesInScene();
		SavingSystem.i.RestoreEntityStates(savableEntities);
	}


	void OnMenuSelected(int selectedItem)
	{
		if (selectedItem == 0)
		{
			// Database
		}
		else if (selectedItem == 1)
		{
			// kreetures			
			//Only should be one party screen in overworld 						
			state = GameState.PartyScreen;
			partyScreen.gameObject.SetActive(true);
		}
		else if (selectedItem == 2)
		{
			// inventory
			inventoryUI.gameObject.SetActive(true);
			state = GameState.Bag;
		}
		else if (selectedItem == 3)
		{
			// Quests
			//SavingSystem.i.Load("saveSlot1");
		}
		else if (selectedItem == 4)
		{
			// Save
			SavingSystem.i.Save("saveSlot1");
			state = GameState.FreeRoam;
		}
		else if (selectedItem == 5)
		{
			// Quit
			//SavingSystem.i.Save("saveSlot1");
		}
	}
}

[System.Serializable]
public class PlayerData
{
	public Vector3 position;
	// Add more data you want to carry over
}