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
        // ���������������0��˵������Ҹ����ʾ��revived��
        if (deathCount > 0)
        {
            UIManager.Instance.ShowRevived();
        }
    }

    // �������
    public void PlayerDie()
    {
        UIManager.Instance.SetPlayerDeathText("You Died");
        deathCount++;
        StartCoroutine(CoRestart());
    }

    // ���������¼�����Ϸ����
    private IEnumerator CoRestart()
    {
        yield return new WaitForSeconds(6f);
        SceneManager.LoadScene("Game");
    }

    // �������й��ʤ��
    public void Win()
    {
        UIManager.Instance.SetPlayerDeathText("Enemy Defeat");
        StartCoroutine(WinAndBackToTitle());
    }

    // ʤ����ص��������
    private IEnumerator WinAndBackToTitle()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartPage");
    }
}