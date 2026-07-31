using Benjathemaker;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    int maxCoin;
    Text nowText;
    Player player;
    Animator animator;
    Text resultText;

    float maxHp;
    Slider hpSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SimpleGemsAnim[] c = FindObjectsByType<SimpleGemsAnim>(FindObjectsSortMode.None);
        maxCoin = c.Length;
        transform.Find("CountBack/MaxCoin").GetComponent<Text>().text = maxCoin.ToString();
        nowText = transform.Find("CountBack/NowCoin").GetComponent<Text>();

        player = FindAnyObjectByType<Player>();

        player.getCoin.Subscribe(get => Count(get)).AddTo(this);
        maxHp = player.HP.Value;

        hpSlider = transform.Find("Hp").GetComponent<Slider>();
        player.HP.Subscribe(hp => HpUpDate(hp)).AddTo(this);

        animator = GetComponent<Animator>();
        animator.SetBool("c", false);

        resultText = transform.Find("ResultBox/ResultsText").GetComponent<Text>();
    }

    void HpUpDate(float hp)
    {
        hpSlider.value = hp / maxHp;
    }

    void Count(int get)
    {
        nowText.text = get.ToString();

        if(get == maxCoin)
        {
            resultText.text = "Game Clear";
            animator.SetBool("c", true);
        }
    }

    public void Death()
    {
        resultText.text = "Game Over";
        animator.SetBool("c", true);
    }

    /// <summary>
    /// もう一度
    /// </summary>
    public void OnceMore()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 終わり
    /// </summary>
    public void End()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲーム開発中
#else
    Application.Quit();//ゲーム開発後
#endif
    }
}
