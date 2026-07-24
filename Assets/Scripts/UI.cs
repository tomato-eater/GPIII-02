using Benjathemaker;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    int maxCoin;
    Text nowText;
    Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SimpleGemsAnim[] c = FindObjectsByType<SimpleGemsAnim>(FindObjectsSortMode.None);
        maxCoin = c.Length;
        transform.Find("CountBack/MaxCoin").GetComponent<Text>().text = maxCoin.ToString();
        nowText = transform.Find("CountBack/NowCoin").GetComponent<Text>();
        player = FindAnyObjectByType<Player>();
        player.getCoin.Subscribe(get => Count(get)).AddTo(this);
    }

    void Count(int get)
    {
        nowText.text = get.ToString();

        if(get == maxCoin)
        {
            Debug.Log("GameClear");
        }
    }
}
