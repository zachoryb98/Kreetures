using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryItem : MonoBehaviour
{
    [SerializeField] Dialog dialog;

    private void OnTriggerEnter(Collider other)
    {        
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }

    public bool TriggerRepeatedly => false;
}
