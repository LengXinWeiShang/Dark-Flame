using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    // Unity 单例
    public static UIManager Instance { get; private set; }

    private PlayerCharacter player;

    public Slider healthSlider;
    public Slider energySlider;
    public Text deathText;
    public Text reviveText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();

        deathText.gameObject.SetActive(false);
        reviveText.gameObject.SetActive(false);
    }

    private void Update()
    {
        healthSlider.value = player.hp * 1.0f / player.maxHp;
        energySlider.value = player.energy / player.maxEnergy;
    }

    public void SetPlayerDeathText(string text)
    {
        deathText.text = text;
        deathText.gameObject.SetActive(true);
    }

    public void ShowRevived()
    {
        reviveText.gameObject.SetActive(true);
        // 先把文本变透明
        Color c = reviveText.color;
        reviveText.color = new Color(c.r, c.g, c.b, 0);

        Sequence seq = DOTween.Sequence();
        seq.Append(reviveText.DOFade(1, 1.0f));
        seq.AppendInterval(0.8f);
        seq.Append(reviveText.DOFade(0, 1.5f));
        seq.AppendCallback(() =>
        {
            reviveText.gameObject.SetActive(false);
        });
    }
}