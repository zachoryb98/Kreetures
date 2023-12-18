using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class DialogManager : MonoBehaviour
{
	private PlayerInput UIControls;
	[SerializeField] GameObject dialogBox;
	[SerializeField] TextMeshProUGUI dialogText;
	[SerializeField] int lettersPerSecond;

	public event Action OnShowDialog;
	public event Action OnCloseDialog;

	public static DialogManager Instance { get; private set; }
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject); // Keep the GameManager object when changing scenes
		}
		else
		{
			Destroy(gameObject); // Destroy any duplicate GameManager instances
		}
		
		UIControls = new PlayerInput();
	}

	public bool IsShowing { get; private set; }

	public IEnumerator ShowDialogText(string text, bool waitForInput = true, bool autoClose = true)
	{
		IsShowing = true;
		dialogBox.SetActive(true);

		yield return TypeDialog(text);
		if (waitForInput)
		{
			yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
		}

		if (autoClose)
		{
			CloseDialog();
		}
	}

	public void CloseDialog()
	{
		dialogBox.SetActive(false);
		IsShowing = false;
	}

	public IEnumerator ShowDialog(Dialog dialog)
	{
		yield return new WaitForEndOfFrame();
		
		OnShowDialog?.Invoke();

		IsShowing = true;

		dialogBox.SetActive(true);

		foreach (var line in dialog.Lines)
		{
			yield return TypeDialog(line);
			yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
		}

        dialogBox.SetActive(false);
		IsShowing = false;
				
		UIControls.OverWorldUI.Disable();
		UIControls.BattleUI.Disable();
		UIControls.PlayerControls.Enable();
		OnCloseDialog?.Invoke();		
	}

	public IEnumerator TypeDialog(string line)
	{
		dialogText.text = "";
		foreach (var letter in line.ToCharArray())
		{
			dialogText.text += letter;
			yield return new WaitForSeconds(1f / lettersPerSecond);
		}		
		GameManager.Instance.playerController.SetContinueDialog(false);		
	}
}