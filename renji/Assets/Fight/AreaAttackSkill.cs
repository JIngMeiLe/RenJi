using UnityEngine;
using System.Collections;

public class AreaAttackSkill : MonoBehaviour
{
    [Header("技能设置")]
    public KeyCode skillKey = KeyCode.E;            // 按键
    public float cooldown = 5f;                     // 冷却时间
    public float damage = 30f;                      // 基础伤害
    public float attackRadius = 15f;                // 攻击半径
    public float castRange = 3f;                    // 施法距离
    public float attckMagic = 10f;                  // 攻击消耗法力值

    [Header("视觉效果")]
    public GameObject areaEffect;                   // 范围特效
    public Color areaColor = Color.blue;            // 范围区域颜色
    public float showTime = 0.8f;                   // 显示时间

    // 内部变量
    private bool isReady = true;
    private float cooldownTimer = 0f;
    private Vector3 attackPosition;                 // 攻击位置

    public TestEnemy testEnemy;


    void Update()
    {
        // 更新冷却时间
        if (!isReady)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isReady = true;
                Debug.Log("范围技能冷却完毕！");
            }
        }

        // 检测按键输入
        if (Input.GetKeyDown(skillKey) && isReady)
        {
            // 先确定攻击位置
            DetermineAttackPosition();
            // 再使用技能
            UseSkill();
        }
    }

    void DetermineAttackPosition()
    {
        //需要手动控制方向
        attackPosition = transform.position + transform.forward * castRange;
    }

    void UseSkill()
    {
        // 开始冷却
        isReady = false;
        cooldownTimer = cooldown;

        Debug.Log($"释放范围攻击！位置: {attackPosition}");

        // 显示攻击区域
        StartCoroutine(ShowAreaEffect());

        // 检测范围内的敌人
        CheckEnemiesInArea();

        // 播放特效
        if (areaEffect != null)
        {
            Instantiate(areaEffect, attackPosition, Quaternion.identity);
        }

    }

    IEnumerator ShowAreaEffect()
    {
        // 显示一段时间
        float timer = 0;
        while (timer < showTime)
        {
            // 绘制圆形区域
            DrawCircle(attackPosition, attackRadius, areaColor);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    // 绘制圆形（使用多个线段模拟）
    void DrawCircle(Vector3 center, float radius, Color color)
    {
        int segments = 30; // 线段数量，越多越圆
        float angleStep = 360f / segments;

        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * radius, 0, Mathf.Sin(0) * radius);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            Debug.DrawLine(prevPoint, nextPoint, color);
            prevPoint = nextPoint;
        }

        // 绘制从中心到圆周的线
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45 * Mathf.Deg2Rad;
            Vector3 edgePoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(center, edgePoint, color);
        }
    }

    void CheckEnemiesInArea()
    {
        // 检测球形区域内的所有碰撞体
        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackRadius);

        int enemyCount = 0;
        int enemiesHit = 0;

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                enemyCount++;
                enemiesHit++;

                // 敌人的受伤方法
                TestEnemy enemy = collider.GetComponent<TestEnemy>();
                if (enemy != null)
                {
                    // 计算距离衰减伤害
                    float distance = Vector3.Distance(attackPosition, collider.transform.position);
                    float distanceFactor = 1f - (distance / attackRadius); // 距离越近伤害越高
                    float finalDamage = damage * distanceFactor;

                    // 调用受伤方法
                    enemy.TakeDamage(finalDamage);
                    Debug.Log($"范围攻击击中 {collider.name}，距离: {distance:F1}，伤害: {finalDamage:F1}");
                }

                // ========== 计算技能使用得分 ==========
                if (ScoreManager.Instance != null)
                {
                    int skillScore = ScoreManager.Instance.CalculateSkillScore(
                        true,
                        enemiesHit
                    );

                    ScoreManager.Instance.AddScore(skillScore);
                }
            }
        }

        if (enemyCount > 0)
        {
            Debug.Log($"范围攻击共击中 {enemyCount} 个敌人");
        }
        else
        {
            Debug.Log("范围攻击没有击中敌人");
        }
    }

    // 在编辑器中可视化攻击范围
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !isReady)
        {
            // 绘制施法范围
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(transform.position + transform.forward * castRange, 0.5f);

            // 绘制攻击范围预览
            Gizmos.color = new Color(areaColor.r, areaColor.g, areaColor.b, 0.2f);
            Gizmos.DrawSphere(transform.position + transform.forward * castRange, attackRadius);

            // 绘制施法方向
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * castRange);
        }
    }
}