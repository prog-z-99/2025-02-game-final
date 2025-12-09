using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    // =========== UI References ==========
    [Header("Main UI Panels")]
    [SerializeField] private GameObject PauseUI;
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject GameClearUI;

    // ========= Main UI Elements ==========
    [Header("Main UI Elements")]
    [SerializeField] private Text MainGoldText;
    [SerializeField] private Text MainGemText;
    [SerializeField] private InputField DebugGoldInputField;
    [SerializeField] private InputField DebugGemInputField;

    // =========== In-Game UI Elements ==========
    [Header("In-Game UI Elements")]
    [SerializeField] private Slider InGamePlayerHPSlider;
    [SerializeField] private Text InGameKillCountText;
    [SerializeField] private Text InGamePlayTimeText;
    [SerializeField] private Text InGameStageRoundText;
    [SerializeField] private Text InGameRoundLeftKillCountText;
    [SerializeField] private Text InGameGoldText;
    [SerializeField] private Text InGameGemText;

    // ========== Unity Lifecycle ==========
    protected override void Awake()
    {
        base.Awake();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameProgressManager.Instance.CurrentState == GameProgressState.Playing)
                GameProgressManager.Instance.PauseGame();
            else if (GameProgressManager.Instance.CurrentState == GameProgressState.Paused)
                GameProgressManager.Instance.ResumeGame();
        }

    }

    // ========== UI Show/Hide Methods ==========
    public void ShowPauseUI()
    {
        PauseUI.SetActive(true);
    }

    public void HidePauseUI()
    {
        PauseUI.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        GameOverUI.SetActive(true);
    }

    public void HideGameOverUI()
    {
        GameOverUI.SetActive(false);
    }

    public void ShowGameClearUI()
    {
        GameClearUI.SetActive(true);
    }

    public void HideGameClearUI()
    {
        GameClearUI.SetActive(false);
    }

    // ========= Main UI Update Methods ==========
    public void UpdateMainMenuResources()
    {
        MainGoldText.text = $"Gold: {MetaManager.Instance.CurrentGold}";
        MainGemText.text = $"Gem: {MetaManager.Instance.CurrentGem}";

        // 재화 변경 시 모든 업그레이드 슬롯 새로고침
        RefreshAllUpgradeSlots();
    }

    public void RefreshAllUpgradeSlots()
    {
        UpgradeSlot[] allSlots = FindObjectsByType<UpgradeSlot>(FindObjectsSortMode.None);
        foreach (var slot in allSlots)
        {
            slot.Refresh();
        }
    }

    public void OnDebugAddGoldButtonClicked()
    {
        if (int.TryParse(DebugGoldInputField.text, out int amount))
        {
            MetaManager.Instance.AddResources(amount, 0);
        }
        DebugGoldInputField.text = null;
    }

    public void OnDebugAddGemButtonClicked()
    {
        if (int.TryParse(DebugGemInputField.text, out int amount))
        {
            MetaManager.Instance.AddResources(0, amount);
        }
        DebugGemInputField.text = null;
    }

    // ========= In-Game UI Update Methods ==========

    public void UpdatePlayerHP(float currentHP, float maxHP)
    {
        InGamePlayerHPSlider.maxValue = maxHP;
        InGamePlayerHPSlider.value = currentHP;
    }

    public void UpdatePlayerHP()
    {
        UpdatePlayerHP(GameProgressManager.Instance.PlayerCurrentHealth, GameProgressManager.Instance.PlayerMaxHealth);
    }

    public void UpdateKillCount(int killCount)
    {
        InGameKillCountText.text = $"Kills: {killCount}";
    }

    public void UpdateKillCount()
    {
        UpdateKillCount(GameProgressManager.Instance.PlayerTotalKillCount);
    }

    public void UpdatePlayTime(float playTime)
    {
        int minutes = (int)(playTime / 60);
        int seconds = (int)(playTime % 60);
        InGamePlayTimeText.text = $"Time: {minutes:D2}:{seconds:D2}";
    }

    public void UpdatePlayTime()
    {
        UpdatePlayTime(GameProgressManager.Instance.PlayTime);
    }

    public void UpdateStageRound(int stage, int round)
    {
        InGameStageRoundText.text = $"Stage: {stage} - Round: {round}";
    }

    public void UpdateStageRound()
    {
        UpdateStageRound(GameProgressManager.Instance.CurrentStage, GameProgressManager.Instance.CurrentRound);
    }

    public void UpdateRoundLeftKillCount(int leftKillCount)
    {
        InGameRoundLeftKillCountText.text = $"Kills remaining: {leftKillCount}";
    }

    public void UpdateRoundLeftKillCount()
    {
        UpdateRoundLeftKillCount(SpawnManager.Instance.RoundLeftKillCount);
    }

    public void UpdateInGameResources()
    {
        InGameGoldText.text = $"Golds: {GameProgressManager.Instance.PlayerCoins}";
        InGameGemText.text = $"Gems: {GameProgressManager.Instance.PlayerGems}";
    }
}