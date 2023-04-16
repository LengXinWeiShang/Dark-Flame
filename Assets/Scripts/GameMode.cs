using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{
    public static GameMode Instance { get; private set; }
    public int enemyCount;
    private static int deathCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("NormalEnemy").Length;
        // 如果死亡次数大于0，说明是玩家复活，显示“revived”
        if (deathCount > 0)
        {
            UIManager.Instance.ShowRevived();
        }
    }

    // 玩家死亡
    public void PlayerDie()
    {
        UIManager.Instance.SetPlayerDeathText("You Died");
        deathCount++;
        StartCoroutine(CoRestart());
    }

    // 死亡后重新加载游戏界面
    private IEnumerator CoRestart()
    {
        yield return new WaitForSeconds(6f);
        SceneManager.LoadScene("Game");
    }

    // 击败所有怪物，胜利
    public void Win()
    {
        UIManager.Instance.SetPlayerDeathText("Enemy Defeat");
        StartCoroutine(WinAndBackToTitle());
    }

    // 胜利后回到标题界面
    private IEnumerator WinAndBackToTitle()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartPage");
    }
}