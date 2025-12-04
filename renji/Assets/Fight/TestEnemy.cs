using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class TestEnemy : MonoBehaviour
{
    [Header("积分设置")]
    public EnemyType enemyType = EnemyType.Normal;
    public int baseScoreValue = 50;

    public float health = 50f;

    void Start()
    {
        // 给敌人添加标签，方便技能检测
        gameObject.tag = "Enemy";

        // 添加一个红色材质以便区分
        GetComponent<Renderer>().material.color = Color.red;
    }

    // 受伤方法
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余生命: {health}");

        if (health <= 0)
        {
            Die();
        }

        //伤害检测增加连击数
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCombo(1);
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 死亡！");

        // 计算并添加积分
        if (ScoreManager.Instance != null)
        {
            // 获取击杀信息
            float distance = Vector3.Distance(transform.position,
                GameObject.FindGameObjectWithTag("Player").transform.position);

            // 计算得分
            int score = ScoreManager.Instance.CalculateKillScore(
                enemyType,
                distance
            );

            // 添加得分
            ScoreManager.Instance.AddScore(score, $"{enemyType}击杀");

        }
        Destroy(gameObject);
    }
}