using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class stringOrder : MonoBehaviour {

	public GameObject[] Characters;
	public TextMesh[] CharTexts;

	private KMNeedyModule NeedyModule;
	private KMAudio BombAudio;

	static int IDCounter = 1;
	int ModuleID;
	MonoRandom rng;

	private GameObject[] SelectedChars = new GameObject[2];

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

	public void Setup()
	{
		ModuleID = IDCounter++;
		rng = GetComponent<KMRuleSeedable> ().GetRNG ();
		SelectString ();
		Scramble ();
	}

	public void OnActivate() {
		Scramble ();
	}

	protected void OnTimerExpired()
	{
		DebugMsg ("Defuser failed to sort the string in time.");
		GetComponent<KMNeedyModule>().OnStrike();
	}

	void SelectString() {
		int i = (int)Random.Range (0,Strings.Length);
		char[] Chars = Strings[i].ToCharArray();
		for (int c = 0; c < CharTexts.Length; c++) {
			CharTexts [c].text = Chars [c].ToString();
		}
		DebugMsg ("String " + Strings[i] + " has been selected");
	}

	void Scramble() {
		MonoRandom rng = GetComponent<KMRuleSeedable> ().GetRNG ();
		Vector3[] loc = new Vector3[Characters.Length];
		for (int i = 0; i < loc.Length; i++) {
			loc [i] = Characters [i].transform.position;
		}
		loc = Shuffle (loc);
		for (int i = 0; i < loc.Length; i++) {
			setX (Characters [i], loc [i]);
		}
	}

	Vector3[] Shuffle(Vector3[] x) {
		Vector3[] o = x;
		for (int i = 0; i < x.Length; i++) {
			int j = Random.Range (0, i + 1);
			if (i != j) {
				Vector3 temp = o [i];
				o [i] = o [j];
				o [j] = temp;
			} else {
				j = (i + 1) % x.Length;
			}
		}
		return o;
	}

	void Awake() {
		Setup ();
		foreach (GameObject Char in Characters) {
			KMSelectable CharSelectable = Char.GetComponent<KMSelectable> ();
			CharSelectable.OnInteract += () =>
			{
				if(Char != SelectedChars[0]) {
					ButtonPress(CharSelectable);
					SelectedChars[1] = SelectedChars[0];
					SelectedChars[0] = Char;
					if(SelectedChars[1] != null) {
						Swap();
					}
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
		BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
	}

	bool Swap() {
		Vector3 loc0 = SelectedChars [0].transform.position;
		Vector3 loc1 = SelectedChars [1].transform.position;
		setX (SelectedChars [0], loc1);
		setX (SelectedChars [1], loc0);
		if (IsOrderCorrect ()) {
			NeedyModule.OnPass();
		}
		SelectedChars = new GameObject[2];
		return false;
	}

	void DebugMsg(string msg) {
		Debug.LogFormat ("[String Order #{0}] {1}", ModuleID, msg);
	}

	bool IsOrderCorrect() {
		bool IsIncorrect = false;
		for (int i = 0; i < Characters.Length - 1; i++) {
			IsIncorrect |= (Characters [i].transform.position.x > Characters [i + 1].transform.position.x);
		}
		return !IsIncorrect;
	}

	void setX(GameObject obj, Vector3 loc) {
		Transform t = obj.transform;
		Quaternion rot = t.rotation;
		obj.transform.SetPositionAndRotation (loc, rot);
	}
}
