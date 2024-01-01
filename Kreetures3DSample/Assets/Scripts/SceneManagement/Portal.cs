using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] public string sceneToLoad;
    [SerializeField] public Transform spawnPoint;

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(SwitchScene());
        }
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);        

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this);

        GameManager.Instance.playerController.gameObject.transform.position = destPortal.SpawnPoint.position;

        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;
}
