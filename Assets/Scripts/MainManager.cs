using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour {
	public Text m_WelcomeText;
	public Text m_BalanceText;
	public Text m_BetAmountText;
	public Slider m_BetAmountSlider;
	public Dropdown m_BetSideDropdown;
	public Button m_BetBtn;
	public Text m_DiceNumber;
	public Text m_DiceResult;

	private string username;
	private LoomWrapper loomWrapper;

	private int lastBet = -1;

	// Use this for initialization
	void Start () {
		username = PlayerPrefs.GetString ("username");
		m_WelcomeText.text = "欢迎，" + username;

		loomWrapper = LoomWrapper.Instance;
		loomWrapper.GetChipCount (OnGetChipCount);

		m_BetBtn.onClick.AddListener (Bet);
		m_BetBtn.enabled = false;
	}

	void OnGetChipCount(LDChipQueryResult result) {
		m_BalanceText.text = "" + result.Amount;

		m_BetAmountSlider.minValue = Mathf.Min (1, result.Amount);
		m_BetAmountSlider.maxValue = result.Amount;

		UpdateSlider (result.Amount);
		m_BetAmountSlider.onValueChanged.AddListener (BetAmountChanged);

		m_BetBtn.enabled = true;
	}

	void BetAmountChanged(float value) {
		m_BetAmountText.text = "" + m_BetAmountSlider.value;
	}

	void UpdateSlider(int amount) {
		// Keep bet amount
		int bet = lastBet;
		if (lastBet < 0) {
			bet = amount;
		} else {
			bet = Mathf.Min (lastBet, amount);
		}
		m_BetAmountSlider.value = bet;
		m_BetAmountText.text = "" + bet;
	}

	async void Bet() {
		int amount = (int)m_BetAmountSlider.value;
		bool betBig = m_BetSideDropdown.value == 0;

		await loomWrapper.RollDice (amount, betBig, DiceRolled);

		lastBet = amount;
	}

	void DiceRolled(LDRollQueryResult result) {
		m_BalanceText.text = "" + result.Amount;
		m_DiceNumber.text = "" + result.Point;
		m_DiceResult.text = (result.Win ? "You Win!" : "You Lose!");

		UpdateSlider (result.Amount);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
