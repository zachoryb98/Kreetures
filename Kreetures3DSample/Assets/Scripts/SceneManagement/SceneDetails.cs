using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;

    public bool IsLoaded { get; private set; }

    List<SavableEntity> savableEntities;

	//private void ChangeScene(Collider2D collision)
	//{
	//	if (collision.tag == "Player")
	//	{
	//		Debug.Log($"Entered {gameObject.name}");

	//		LoadScene();
	//		GameController.Instance.SetCurrentScene(this);

	//		// Load all connected scenes
	//		foreach (var scene in connectedScenes)
	//		{
	//			scene.LoadScene();
	//		}

	//		// Unload the scenes that ar no longer connected
	//		var prevScene = GameController.Instance.PrevScene;
	//		if (prevScene != null)
	//		{
	//			var previoslyLoadedScenes = prevScene.connectedScenes;
	//			foreach (var scene in previoslyLoadedScenes)
	//			{
	//				if (!connectedScenes.Contains(scene) && scene != this)
	//					scene.UnloadScene();
	//			}

	//			if (!connectedScenes.Contains(prevScene))
	//				prevScene.UnloadScene();
	//		}
	//	}
	//}

	//public void LoadScene()
 //   {
 //       if (!IsLoaded)
 //       {
 //           var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
 //           IsLoaded = true;

 //           operation.completed += (AsyncOperation op) =>
 //           {
 //               savableEntities = GetSavableEntitiesInScene();
 //               SavingSystem.i.RestoreEntityStates(savableEntities);
 //           };
 //       }
 //   }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }
}
