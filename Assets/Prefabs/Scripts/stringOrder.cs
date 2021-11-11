using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class stringOrder : MonoBehaviour {

	public GameObject[] Characters;
	public TextMesh[] CharTexts;

	private KMNeedyModule NeedyModule;
	private KMAudio BombAudio;

	private Color selected = new Color(0.988f, 1f, 0.278f, 1f);
	private Color unselected = new Color(1f, 1f, 1f, 1f);

	static int IDCounter = 1;
	int ModuleID;

	private int?[] SelectedChars = new int?[2];

	string[] Strings = {
		"ABCDEFGHIJ",
		"TSRQPONMLK",
		"UVWXYZABCD",
		"1234567890",
		"ABCDE12345",
		"24680ZYXWV",
		"13579ACEGI",
		"12358KMNQT",
		"AZBYCXDWEV",
		"2019PQRSTE",
		"ABCZYX1230"
	};

	private string currentString;
	private string selectedString;

	private bool ModActive;

	public void Setup()
	{
		ModuleID = IDCounter++;
	}

	public void OnActivate() {
		ModActive = true;
		SelectString ();
		Scramble ();
	}

	protected void OnTimerExpired()
	{
		ModActive = false;
		DebugMsg ("Defuser failed to sort the string in time.");
		NeedyModule.OnStrike();
	}

	void SelectString() {
		int i = Random.Range (0,Strings.Length);
		selectedString = Strings[i];
		currentString = Strings[i];
		DebugMsg ("String " + selectedString + " has been selected.");
	}

	void Scramble() {
		char[] scrambledChars = currentString.ToCharArray().Shuffle();
		while (scrambledChars.Join("") == selectedString)
			scrambledChars = currentString.ToCharArray().Shuffle();
		for (int c = 0; c < CharTexts.Length; c++) {
			CharTexts[c].text = scrambledChars[c].ToString();
		}
		currentString = scrambledChars.Join("");
	}

	void Awake() {
		Setup ();
		foreach (GameObject Char in Characters) {
			KMSelectable CharSelectable = Char.GetComponent<KMSelectable> ();
			CharSelectable.OnInteract += () =>
			{
				int index = Array.IndexOf(Characters, Char);
				ButtonPress(CharSelectable);
				CharTexts[index].color = selected;
				SelectedChars[1] = SelectedChars[0];
				SelectedChars[0] = index;
				if (SelectedChars[1] != null)
				{
					Swap();
				}
				return false;
			};
		}
		NeedyModule = GetComponent<KMNeedyModule> ();
		BombAudio = GetComponent<KMAudio> ();
		NeedyModule.OnNeedyActivation += OnActivate;
		NeedyModule.OnTimerExpired += OnTimerExpired;
	}

	void ButtonPress(KMSelectable Selectable)
	{
		Selectable.AddInteractionPunch(0.5f);
		BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Selectable.transform);
	}

	bool Swap() {
		string store = CharTexts[(int)SelectedChars[0]].text;
		CharTexts[(int)SelectedChars[0]].text = CharTexts[(int)SelectedChars[1]].text;
		CharTexts[(int)SelectedChars[1]].text = store;
		CharTexts[(int)SelectedChars[0]].color = unselected;
		CharTexts[(int)SelectedChars[1]].color = unselected;
		string newString = "";
		for (int c = 0; c < CharTexts.Length; c++)
		{
			newString += CharTexts[c].text;
		}
		currentString = newString;
		if (IsOrderCorrect () && ModActive) {
			DebugMsg("Defuser successfully sorted the string.");
			NeedyModule.OnPass();
			ModActive = false;
		}
		SelectedChars[0] = null;
		SelectedChars[1] = null;
		return false;
	}

	void DebugMsg(string msg) {
		Debug.LogFormat ("[String Order #{0}] {1}", ModuleID, msg);
	}

	bool IsOrderCorrect() {
		bool IsIncorrect = true;
		if (currentString != selectedString) IsIncorrect = false;
		return IsIncorrect;
	}

	//twitch plays
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} swap <0-9><0-9> [Swaps the two specified characters from left to right] | Command can be chained, for ex: !{0} swap 04;58;23";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*swap\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
            if (parameters.Length == 2)
            {
				string[] pairs = parameters[1].Split(';');
				for (int i = 0; i < pairs.Length; i++)
                {
					if (pairs[i].Length != 2)
						yield break;
					int temp = -1;
					if (!int.TryParse(pairs[i], out temp))
						yield break;
					if (temp < 0 || temp > 99)
						yield break;
				}
				yield return null;
				for (int i = 0; i < pairs.Length; i++)
				{
					Characters[pairs[i][0] - '0'].GetComponent<KMSelectable>().OnInteract();
					yield return new WaitForSeconds(.1f);
					Characters[pairs[i][1] - '0'].GetComponent<KMSelectable>().OnInteract();
					yield return new WaitForSeconds(.1f);
				}
			}
		}
	}

	void TwitchHandleForcedSolve()
	{
		StartCoroutine(DealWithNeedy());
	}

	private IEnumerator DealWithNeedy()
	{
		while (true)
		{
			while (!ModActive) { yield return null; }
			if (SelectedChars[0] != null)
            {
				int correctIndex = selectedString.IndexOf(currentString[(int)SelectedChars[0]]);
				Characters[correctIndex].GetComponent<KMSelectable>().OnInteract();
				yield return new WaitForSeconds(.1f);
			}
			for (int i = 0; i < 10; i++)
            {
				if (currentString[i] != selectedString[i])
                {
					Characters[i].GetComponent<KMSelectable>().OnInteract();
					yield return new WaitForSeconds(.1f);
					int correctIndex = selectedString.IndexOf(currentString[i]);
					Characters[correctIndex].GetComponent<KMSelectable>().OnInteract();
					yield return new WaitForSeconds(.1f);
				}
            }
		}
	}
}