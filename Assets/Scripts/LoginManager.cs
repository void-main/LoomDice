using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Loom.Unity3d;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour {
	public InputField m_UsernameInput;
	public Button m_LoginBtn;

	// Use this for initialization
	void Start () {
//		if (PlayerPrefs.GetString ("username") != "") {
//			SceneManager.LoadScene ("MainScene");
//			return;
//		}

		Button loginBtn = m_LoginBtn.GetComponent<Button> ();
		loginBtn.onClick.AddListener (LoginClicked);
	}

	async void LoginClicked() {
		Debug.Log ("Login clicked, username: " + m_UsernameInput.text);

		var privateKey = CryptoUtils.GeneratePrivateKey();
		var publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);

		LoomWrapper loom = LoomWrapper.Instance;
		var contract = await loom.GetContract(privateKey, publicKey);
		await loom.CreateAccount (contract, m_UsernameInput.text);
		await loom.GetChipCount (contract, m_UsernameInput.text);

		PlayerPrefs.SetString ("username", m_UsernameInput.text);
		PlayerPrefs.Save ();

		SceneManager.LoadScene ("MainScene");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
