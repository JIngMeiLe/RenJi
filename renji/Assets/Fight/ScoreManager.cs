using UnityEngine;
using System;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("积分设置")]
    [SerializeField] private int currentScore = 0;          // 当前积分
    [SerializeField] private int maxScore = 999999;         // 最大积分
    [SerializeField] private int minScore = 0;              // 最小积分

    [Header("连击系统")]
    [SerializeField] private int comboCount = 0;            // 当前连击数
    [SerializeField] private float comboTimeWindow = 3f;    // 连击时间窗口（秒）
    [SerializeField] private float lastKillTime = 0f;       // 上次击杀时间
    [SerializeField] private int maxCombo = 1000;             // 最大连击数

    [Header("计算公式系数")]
    [SerializeField] private float baseScoreMultiplier = 1f;    // 基础分数倍率
    [SerializeField] private float comboMultiplier = 0.1f;      // 连击倍率系数
    [SerializeField] private float distanceBonusFactor = 0.05f; // 距离加成系数

    [Header("特殊奖励")]
    [SerializeField] private int multiKillBonus = 30;           // 多杀奖励

    // 事件：当积分变化时触发
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action<int> OnHighScoreAchieved;

    // 属性
    public int CurrentScore => currentScore;
    public int ComboCount => comboCount;
    public int HighScore { get; private set; }

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 加载历史最高分
        LoadHighScore();
    }

    void Update()
    {
        // 更新连击系统：检查是否超出连击时间窗口
        if (comboCount > 0 && Time.time - lastKillTime > comboTimeWindow)
        {
            ResetCombo();
        }
    }

    // ========== 核心接口：加分/减分 ==========

    // 增加积分
    public void AddScore(int points, string reason = "", bool showPopup = true)
    {
        if (points <= 0) return;

        int oldScore = currentScore;
        currentScore = Mathf.Clamp(currentScore + points, minScore, maxScore);

        // 触发积分变化事件
        OnScoreChanged?.Invoke(currentScore);

        // 检查是否达到新高分
        if (currentScore > HighScore)
        {
            HighScore = currentScore;
            OnHighScoreAchieved?.Invoke(HighScore);
            SaveHighScore();
        }

        Debug.Log($"积分增加: +{points} ({reason}) 当前积分: {currentScore}");
    }

    // 减少积分
    public void SubtractScore(int points, string reason = "", bool showPopup = true)
    {
        if (points <= 0) return;

        int oldScore = currentScore;
        currentScore = Mathf.Clamp(currentScore - points, minScore, maxScore);

        OnScoreChanged?.Invoke(currentScore);

        Debug.Log($"积分减少: -{points} ({reason}) 当前积分: {currentScore}");
    }

    // 直接设置积分
    public void SetScore(int newScore)
    {
        int oldScore = currentScore;
        currentScore = Mathf.Clamp(newScore, minScore, maxScore);

        if (currentScore != oldScore)
        {
            OnScoreChanged?.Invoke(currentScore);

            if (currentScore > HighScore)
            {
                HighScore = currentScore;
                SaveHighScore();
            }
        }
    }

    // 重置积分（但保留最高分记录）
    public void ResetScore()
    {
        int oldScore = currentScore;
        currentScore = 0;
        ResetCombo();

        if (oldScore != 0)
        {
            OnScoreChanged?.Invoke(currentScore);
        }
    }

    // ========== 连击系统 ==========

    // 增加连击数
    public void AddCombo(int amount = 1)
    {
       
        // 检查是否在连击时间窗口内
        if (Time.time - lastKillTime <= comboTimeWindow)
        {
            comboCount = Mathf.Min(comboCount + amount, maxCombo);
        }
        else
        {
            comboCount = amount; // 重新开始连击
        }

        lastKillTime = Time.time;
        OnComboChanged?.Invoke(comboCount);

        Debug.Log($"连击数: {comboCount}");
    }

    // 重置连击
    public void ResetCombo()
    {
        if (comboCount > 0)
        {
            Debug.Log($"连击中断! 最高连击: {comboCount}");
            comboCount = 0;
            OnComboChanged?.Invoke(0);
        }
    }

    // ========== 积分计算公式 ==========

    // 计算击杀得分（主要计算公式）
    public int CalculateKillScore(EnemyType enemyType, float distance = 0f)
    {
        // 基础分
        int baseScore = GetEnemyBaseScore(enemyType);

        // 距离加成
        float distanceBonus = 1f + (distance * distanceBonusFactor);

        // 连击加成
        float comboBonus = 1f + (comboCount * comboMultiplier);

        // 总分数计算
        float totalMultiplier = baseScoreMultiplier * distanceBonus *
                                comboBonus;

        int finalScore = Mathf.RoundToInt(baseScore * totalMultiplier);

        // 增加连击数
        AddCombo();

        return finalScore;
    }

    // 计算技能使用得分
    public int CalculateSkillScore(bool hitEnemy = false, int enemiesHit = 0)
    {
        int baseScore = 0;

        // 命中敌人奖励
        if (hitEnemy)
        {
            baseScore += 5;
            baseScore += enemiesHit * 3; // 每多命中一个敌人加3分
        }

        return baseScore;
    }

    // 计算多杀奖励
    public int CalculateMultiKillBonus(int killCount)
    {
        if (killCount <= 1) return 0;

        // 多杀奖励公式：基础奖励 * (杀敌数-1)
        return multiKillBonus * (killCount - 1);
    }

    // ========== 辅助方法 ==========

    private int GetEnemyBaseScore(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Normal:
                return 50;
            case EnemyType.Elite:
                return 100;
            default:
                return 50;
        }
    }

    // ========== 数据持久化 ==========

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", HighScore);
        PlayerPrefs.Save();
    }

    private void LoadHighScore()
    {
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    // ========== 调试方法 ==========

    [ContextMenu("增加100分")]
    public void DebugAdd100Points()
    {
        AddScore(100, "调试奖励");
    }

    [ContextMenu("减少50分")]
    public void DebugSubtract50Points()
    {
        SubtractScore(50, "调试惩罚");
    }

    [ContextMenu("重置积分")]
    public void DebugResetScore()
    {
        ResetScore();
    }
}

// ========== 枚举定义 ==========

public enum EnemyType
{
    Normal,     // 普通敌人
    Elite,      // 精英敌人
}