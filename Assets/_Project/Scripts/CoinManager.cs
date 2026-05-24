using TMPro;
using UnityEngine;

public sealed class CoinManager : MonoBehaviour
{
    private const string TotalCoinsKey = "TOTAL_COINS";

    [SerializeField] private TMP_Text coinsText;

    private int runCoins;
    private int totalCoins;

    public int RunCoins => runCoins;
    public int TotalCoins => totalCoins;

    private void Awake()
    {
        totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        HideCoins();
    }

    public void BeginCounting()
    {
        runCoins = 0;
        totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);

        if (coinsText != null)
        {
            coinsText.gameObject.SetActive(true);
        }

        UpdateCoinsText();
    }

    public void AddCoins(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);

        if (safeAmount == 0)
        {
            return;
        }

        runCoins += safeAmount;
        totalCoins += safeAmount;

        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.Save();

        UpdateCoinsText();
    }

    public void HideCoins()
    {
        if (coinsText != null)
        {
            coinsText.gameObject.SetActive(false);
        }
    }

    public void ClearTotalCoins()
    {
        runCoins = 0;
        totalCoins = 0;

        PlayerPrefs.DeleteKey(TotalCoinsKey);
        PlayerPrefs.Save();

        UpdateCoinsText();
    }

    private void UpdateCoinsText()
    {
        if (coinsText == null)
        {
            return;
        }

        coinsText.text = $"Coins: {runCoins}";
    }
}