using UnityEngine;
using System.Collections;

public class SingleTargetSkill : MonoBehaviour
{
    [Header("技能设置")]
    public KeyCode skillKey = KeyCode.Q;            // 按键
    public float cooldown = 3f;                     // 冷却时间
    public float damage = 20f;                      // 伤害
    public float attackRange = 20f;                 // 攻击距离
    public float attackWidth = 1f;                  // 攻击宽度
    public float attckMagic = 5f;                   // 攻击消耗法力值

    [Header("视觉效果")]
    public GameObject attackEffect;                 // 攻击特效
    public Color attackAreaColor = Color.red;       // 攻击区域颜色
    public float showTime = 0.5f;                   // 显示攻击区域的时间

    // 内部变量
    private bool isReady = true;                    // 技能是否就绪
    private float cooldownTimer = 0f;               // 冷却计时器

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
                Debug.Log("单体技能冷却完毕！");
            }
        }

        // 检测按键输入
        if (Input.GetKeyDown(skillKey) && isReady)
        {
            UseSkill();
        }
    }

    void UseSkill()
    {
        // 开始冷却
        isReady = false;
        cooldownTimer = cooldown;

        Debug.Log("释放单体技能！");

        // 显示攻击区域
        StartCoroutine(ShowAttackArea());

        // 检测攻击区域内的敌人
        CheckForEnemies();

        // 播放特效
        if (attackEffect != null)
        {
            Instantiate(attackEffect, transform.position + transform.forward * attackRange / 2, transform.rotation);
        }

    }

    // 显示攻击区域（协程）
    IEnumerator ShowAttackArea()
    {
        // 计算攻击区域的四个角
        Vector3 center = transform.position + transform.forward * attackRange / 2;
        float halfWidth = attackWidth / 2;

        Vector3 frontLeft = transform.position + transform.forward * attackRange + transform.right * -halfWidth;
        Vector3 frontRight = transform.position + transform.forward * attackRange + transform.right * halfWidth;
        Vector3 backLeft = transform.position + transform.right * -halfWidth;
        Vector3 backRight = transform.position + transform.right * halfWidth;

        // 显示0.5秒
        float timer = 0;
        while (timer < showTime)
        {
            // 绘制攻击区域（在Scene视图中可见）
            Debug.DrawLine(backLeft, frontLeft, attackAreaColor);
            Debug.DrawLine(backRight, frontRight, attackAreaColor);
            Debug.DrawLine(backLeft, backRight, attackAreaColor);
            Debug.DrawLine(frontLeft, frontRight, attackAreaColor);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    void CheckForEnemies()
    {
        // 计算攻击区域中心点
        Vector3 attackCenter = transform.position + transform.forward * (attackRange / 2);

        // 使用BoxCast检测攻击区域内的敌人
        RaycastHit[] hits = Physics.BoxCastAll(
            attackCenter,
            new Vector3(attackWidth / 2, 1f, attackRange / 2), // 检测区域大小
            transform.forward,
            transform.rotation,
            attackRange
        );

        bool hitEnemy = false;
        int enemiesHit = 0; // 连击数

        // 遍历所有被击中的物体
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                hitEnemy = true;
                enemiesHit++;

                // 敌人的受伤方法
                TestEnemy enemy = hit.collider.GetComponent<TestEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"单体攻击击中了 {hit.collider.name}，造成{damage}点伤害");
                }
                else
                {
                    Debug.LogWarning($"击中了敌人 {hit.collider.name}，但没有找到TestEnemy脚本");
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

        if (hitEnemy)
        {
            Debug.Log("单体攻击命中敌人");
        }
        else
        {
            Debug.Log("单体攻击没有命中敌人");
        }
    }
    

    // 在编辑器中显示攻击范围（方便调整参数）
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !isReady)
        {
            Gizmos.color = new Color(attackAreaColor.r, attackAreaColor.g, attackAreaColor.b, 0.3f);

            // 绘制攻击区域（立方体）
            Vector3 center = transform.position + transform.forward * attackRange / 2;
            Vector3 size = new Vector3(attackWidth, 2f, attackRange);
            Gizmos.DrawCube(center, size);

            // 绘制攻击方向
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * attackRange);
        }
    }
}