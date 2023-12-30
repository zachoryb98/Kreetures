using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }
    private void Awake()
    {
        if (i == null)
        {
            i = this;
            DontDestroyOnLoad(gameObject); // Keep the EvolutionManager object when changing scenes
        }
        else
        {
            if(SceneManager.GetActiveScene().name == "EvolutionScene")
                EvolutionManager.i.StartEvolution();
            Destroy(gameObject); // Destroy any duplicate GameManager instances
        }
    }

    public void StartEvolution()
    {
        GameManager.Instance.inventoryUI.gameObject.SetActive(false);
        FetchEvolutionInfo();
    }

    private void FetchEvolutionInfo()
    {
        var evolutions = GameManager.Instance.GetEvolutions();
        SetUpEvolution(evolutions);
    }

    private void SetUpEvolution(Dictionary<Kreeture, Evolution> evolutions)
    {
        //Key = Kreeture
        //Value = Evolution
        foreach (var kreeture in evolutions)
        {
            StartCoroutine(Evolve(kreeture.Key, kreeture.Value));
        }
    }

    public IEnumerator Evolve(Kreeture kreeture, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        //evolutionUI.SetActive(true);

        var kreetureModel = kreeture.Base.Model;
        var instantiatedModel = Instantiate(kreetureModel, new Vector3(0, 5, 0), Quaternion.Euler(0, -165, 0));

        yield return DialogManager.Instance.ShowDialogText($"{kreeture.Base.Name} is evolving", true);

        var oldKreeture = kreeture.Base;
        kreeture.Evolve(evolution);

        var newKreetureModel = kreeture.Base.Model;
        DestroyImmediate(instantiatedModel, true);
        Instantiate(newKreetureModel, new Vector3(0, 5, 0), Quaternion.Euler(0, -165, 0));
        yield return DialogManager.Instance.ShowDialogText($"{oldKreeture.Name} evolved into {kreeture.Base.Name}", true);        

        yield return new WaitForSeconds(5.0f);

        //evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
