using UnityEngine;
using System.Collections;

public class NormalAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public float damage = 5f;          // 攻击伤害
    public float attackRange = 30f;      // 攻击范围
    public float attackCooldown = 0.5f; // 攻击冷却时间

    [Header("按键设置")]
    public KeyCode attackKey = KeyCode.Mouse0; // 攻击按键（默认鼠标左键）

    [Header("锁敌设置")]
    public string enemyTag = "Enemy";   // 敌人标签

    [Header("视觉效果")]
    public Color attackRangeColor = Color.yellow;   // 攻击范围显示颜色

    // 内部变量
    private float currentCooldown = 0f; // 当前冷却时间
    private bool canAttack = true;      // 是否可以攻击

    void Update()
    {
        // 更新冷却时间
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                canAttack = true;
            }
        }

        // 按下攻击键时尝试攻击
        if (Input.GetKeyDown(attackKey) && canAttack)
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        // 寻找最近的敌人
        GameObject closestEnemy = FindClosestEnemy();

        // 如果有敌人并且在攻击范围内，就攻击
        if (closestEnemy != null)
        {
            float distance = Vector3.Distance(transform.position, closestEnemy.transform.position);
            if (distance <= attackRange)
            {
                AttackEnemy(closestEnemy);
            }
            else
            {
                Debug.Log("敌人在攻击范围外！");
            }
        }
        else
        {
            Debug.Log("没有找到敌人！");
        }
    }

    GameObject FindClosestEnemy()
    {
        // 查找所有敌人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            // 寻找最近的敌人
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    void AttackEnemy(GameObject enemy)
    {
        // 设置冷却时间
        canAttack = false;
        currentCooldown = attackCooldown;

        // 对敌人造成伤害
        if (enemy != null)
        {
            TestEnemy enemyScript = enemy.GetComponent<TestEnemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
                Debug.Log($"普攻击中 {enemy.name}，造成 {damage} 伤害");
            }
            else
            {
                Debug.Log($"击中 {enemy.name}，但没有找到受伤脚本");
            }
        }
    }

    // 可视化攻击范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // 在UI上显示攻击信息
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 16;

        // 显示攻击信息
        GUI.Label(new Rect(10, 160, 200, 30), $"攻击键: {attackKey}", style);
        GUI.Label(new Rect(10, 180, 200, 30), $"普攻伤害: {damage}", style);
        GUI.Label(new Rect(10, 200, 200, 30), $"攻击范围: {attackRange}", style);

        // 显示冷却时间
        if (!canAttack)
        {
            GUI.Label(new Rect(10, 220, 200, 30), $"冷却中: {currentCooldown:F2}s", style);
        }
        else
        {
            GUI.Label(new Rect(10, 220, 200, 30), "可以攻击", style);
        }
    }
}