using System.Collections;
using UnityEngine;

public interface IAttack
{
    void Attack();
    string GetAttackName();
    float GetAttackDistance();
    Vector2 GetDamageRange();
    float GetAttackCooldown();
    bool GetCanAttack();

    int GetDamage();

    Vector2 GetKnockbackDir();

    float GetKnockbackForce();

    IEnumerator AttackCooldown();

    

}
