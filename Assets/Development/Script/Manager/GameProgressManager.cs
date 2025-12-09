/* 
게임 진행 관리 매니저
 - 로그라이크 게임의 진행 상황(스테이지 및 라운드)을 관리하는 싱글톤 매니저 클래스
 - 게임 오버 시 현재 진행 중인 스테이지는 초기화되고, 최고 기록만 저장
 - 각 플레이는 독립적이며, 게임 오버 시 처음부터 다시 시작
 */

using System;
using UnityEngine;

public enum GameProgressState
{
    Idle,           // 대기 상태 (메뉴, 로비)
    Playing,        // 플레이 중
    Paused,         // 일시정지
    GameOver,       // 게임 오버
    GameClear       // 게임 클리어
}

public class GameProgressManager : Singleton<GameProgressManager>
{
    // ========== 현재 진행 상태 ==========
    private GameProgressState currentState = GameProgressState.Idle;
    [SerializeField] private int currentStage = 0;
    [SerializeField] private int currentRound = 0;

    // ========== 속성 ==========
    public GameProgressState CurrentState => currentState;
    public int CurrentStage => currentStage;
    public int CurrentRound => currentRound;

    // ========== 플레이어 정보 ==========
    [SerializeField] private int playerMaxHealth;
    [SerializeField] private int playerCurrentHealth;
    [SerializeField] private int playerLevel;
    [SerializeField] private int playerExperience;
    [SerializeField] private int playerCoins;
    [SerializeField] private int playerGems;
    [SerializeField] private int playerTotalKillCount;
    [SerializeField] private float playTime;

    // ========= 플레이어 정보 접근자 ==========
    public int PlayerMaxHealth => playerMaxHealth;
    public int PlayerCurrentHealth => playerCurrentHealth;
    public int PlayerLevel => playerLevel;
    public int PlayerExperience => playerExperience;
    public int PlayerCoins => playerCoins;
    public int PlayerGems => playerGems;
    public int PlayerTotalKillCount => playerTotalKillCount;
    public float PlayTime => playTime;

    // ========== Unity Lifecycle ==========
    protected override void Awake()
    {
        base.Awake();

        ChangeState(GameProgressState.Idle);
    }

    void Start()
    {
        // StartStage(1);
    }

    void Update()
    {
        if (currentState == GameProgressState.Playing)
        {
            playTime += Time.deltaTime;
            UIManager.Instance.UpdatePlayTime();
        }
    }

    // ========== 플레이어 정보 관리 ==========

    public void InitPlayerStats(int health)
    {
        SetPlayerStats(health, 0, 0, 0, 0, 0, 0f);
    }

    public void SetPlayerStats(int maxHealth, int level, int experience, int coins, int gems, int totalKillCount, float playTime)
    {
        playerMaxHealth = maxHealth;
        playerCurrentHealth = maxHealth;
        playerLevel = level;
        playerExperience = experience;
        playerCoins = coins;
        playerGems = gems;
        playerTotalKillCount = totalKillCount;
        this.playTime = playTime;
        UIManager.Instance.UpdateInGameResources();
    }

    public void ResetPlayerStats()
    {
        SetPlayerStats(playerMaxHealth, 0, 0, 0, 0, 0, 0f);
        UIManager.Instance.UpdateInGameUI();
    }

    public void AddHealth(int amount)
    {
        playerCurrentHealth = Mathf.Min(playerCurrentHealth + amount, playerMaxHealth);
        UIManager.Instance.UpdatePlayerHP();
    }

    public void TakeDamage(int amount)
    {
        playerCurrentHealth = Mathf.Max(playerCurrentHealth - amount, 0);
        UIManager.Instance.UpdatePlayerHP();
        if (playerCurrentHealth <= 0)
        {
            GameOver();
        }
    }

    public void AddExperience(int amount)
    {
        playerExperience += amount;
        // UIManager.Instance.UpdatePlayerExperience();
    }

    public void AddCoins(int amount)
    {
        playerCoins += amount;
        UIManager.Instance.UpdateInGameResources();
    }

    public void AddGems(int amount)
    {
        playerGems += amount;
        UIManager.Instance.UpdateInGameResources();
    }

    public void AddKillCount(int amount)
    {
        playerTotalKillCount += amount;
        UIManager.Instance.UpdateKillCount();
        UIManager.Instance.UpdateRoundLeftKillCount();
    }

    public void AddKillCount()
    {
        AddKillCount(1);
    }

    // ========== 스테이지 관리 ==========
    public void StartStage(int stage)
    {
        ChangeState(GameProgressState.Playing);
        currentStage = stage;
        SpawnManager.Instance.SetStage(currentStage);
        currentRound = 0;
        NextRound();

        UIManager.Instance.UpdateStageRound();
    }

    public void CompleteStage()
    {
        SaveProgress();
        if (currentStage < StageSettings.MaxStageNumber)
            NextStage();
        else
            GameClear();
    }

    public void NextStage()
    {
        StartStage(currentStage + 1);
    }

    // ========== 라운드 관리 ==========
    public void StartRound(int round)
    {
        ChangeState(GameProgressState.Playing);
        currentRound = round;
        SpawnManager.Instance.SetRound(currentRound);

        UIManager.Instance.UpdateStageRound();
        UIManager.Instance.UpdateRoundLeftKillCount();
    }

    public void CompleteRound()
    {
        SaveProgress();
        NextRound();
    }

    public void NextRound()
    {
        StartRound(currentRound + 1);
    }

    // ========== 게임 상태 관리 ==========
    public void ChangeState(GameProgressState newState)
    {
        if (currentState == newState) return;

        GameProgressState oldState = currentState;
        currentState = newState;

        HandleStateTransition(oldState, newState);
    }

    private void HandleStateTransition(GameProgressState oldState, GameProgressState newState)
    {
        if (oldState == newState) return;
        // 이전 상태 종료 처리
        switch (oldState)
        {
            case GameProgressState.Idle:
                UIManager.Instance.HideMainMenuUI();
                UIManager.Instance.ShowInGameUI();
                StartStage(1);
                break;
            case GameProgressState.Playing:
                break;
            case GameProgressState.Paused:
                Time.timeScale = 1f;
                HidePauseUI(); // 일시정지 UI 숨기기
                break;
            case GameProgressState.GameOver:
                HideGameOverUI(); // 게임 오버 UI 숨기기
                break;
            case GameProgressState.GameClear:
                HideGameClearUI(); // 게임 클리어 UI 숨기기
                break;
        }

        // 새 상태 시작 처리
        switch (newState)
        {
            case GameProgressState.Idle:
                Time.timeScale = 1f;
                MetaManager.Instance.AddResources(playerCoins, playerGems); // 메타 매니저에 자원 추가
                ClearAllMonsters();
                break;
            case GameProgressState.Playing:
                Time.timeScale = 1f;
                ResetPlayerStats();
                // BackgroundManager.Instance.StartSpawningBackgrounds();
                break;
            case GameProgressState.Paused:
                Time.timeScale = 0f;
                ShowPauseUI();
                break;
            case GameProgressState.GameOver:
                Time.timeScale = 0f;
                SaveMaxProgressToProfile();  // 프로필에 최고 기록 저장
                ShowGameOverUI(); // 게임 오버 UI 표시
                ResetProgress(); // 진행 상황 초기화
                // BackgroundManager.Instance.StopSpawningBackgrounds(); // 배경 소환 중지

                break;
            case GameProgressState.GameClear:
                Time.timeScale = 0f;
                SaveMaxProgressToProfile();  // 프로필에 최고 기록 저장
                ShowGameClearUI(); // 게임 클리어 UI 표시
                ResetProgress(); // 진행 상황 초기화
                // BackgroundManager.Instance.StopSpawningBackgrounds(); // 배경 소환 중지

                break;
        }
    }

    public void PauseGame()
    {
        ChangeState(GameProgressState.Paused);
    }

    public void ResumeGame()
    {
        ChangeState(GameProgressState.Playing);
    }

    public void GameOver()
    {
        ChangeState(GameProgressState.GameOver);
    }

    public void GameClear()
    {
        ChangeState(GameProgressState.GameClear);
    }

    public void ExitGame()
    {
        ChangeState(GameProgressState.Idle);
    }

    // ========== 진행 상황 저장/로드 ==========
    private void SaveProgress()
    {
        if (ProfileManager.Instance.CurrentState == ProfileState.Active)
        {
            ProfileManager.Instance.GameData.UpdateMaxProgress(currentStage, currentRound);
            ProfileManager.Instance.SaveActiveProfile();
        }
    }

    // 프로필에 최고 진행 사항 저장
    private void SaveMaxProgressToProfile()
    {
        if (ProfileManager.Instance.CurrentState == ProfileState.Active)
        {
            ProfileManager.Instance.GameData.UpdateMaxProgress(currentStage, currentRound);
            ProfileManager.Instance.SaveActiveProfile();
        }
    }

    // 프로필 최고 진행 사항 불러오기
    public void LoadProgress()
    {
        if (ProfileManager.Instance.CurrentState == ProfileState.Active)
        {
            currentStage = ProfileManager.Instance.GameData.MaxStage;
            currentRound = ProfileManager.Instance.GameData.MaxRound;
        }
    }

    // 진행 상황 초기화
    public void ResetProgress()
    {
        currentStage = 0;
        currentRound = 0;
    }

    // ========== UI 관리 ==========
    protected void ShowPauseUI()
    {
        UIManager.Instance.ShowPauseUI();
    }

    protected void HidePauseUI()
    {
        UIManager.Instance.HidePauseUI();
    }

    protected void ShowGameOverUI()
    {
        UIManager.Instance.ShowGameOverUI();
    }

    protected void HideGameOverUI()
    {
        UIManager.Instance.HideGameOverUI();
    }

    protected void ShowGameClearUI()
    {
        UIManager.Instance.ShowGameClearUI();
    }

    protected void HideGameClearUI()
    {
        UIManager.Instance.HideGameClearUI();
    }

    // ========= 기타 메서드 ==========
    private void ClearAllMonsters()
    {
        SpawnManager.Instance.ClearAllMonsters();
    }
}