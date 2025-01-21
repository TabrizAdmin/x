using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassController : MonoBehaviour
{
    public AudioClip attackSound;
    public AudioSource sfx;
    public int level, hp, maxHP, attack, defence, damage;
    public string className;
    public Boolean attackable;
    protected Animator animator;
    protected int targetDefence;
    protected GameObject soundManager, target, tempTarget, root, HealthBar;
    protected LevelManagerController levelManager;
    protected WalkingController walking;
    protected string enemyGroupName;
    protected float dist, tempDist;
    protected Boolean arrowFired;

    protected void GetDataFromAPI()
    {
        // level = UnityEngine.Random.Range(1, 14);
    }

    protected void StartingFunction()
    {
        soundManager = GameObject.Find("SoundManager");
        sfx = soundManager.GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerController>();
        walking = transform.parent.GetComponent<WalkingController>();

        if (gameObject.tag == "Ally")
            enemyGroupName = "Enemy";
        else
            enemyGroupName = "Ally";

        hp = 10 * level;
        maxHP = hp;
        damage = 0;

        HealthBar = transform.Find("HealthBarBG").gameObject;
    }

    protected void SecondFunction()
    {
        FindTarget();

        if (!target)
            return;

        if (damage == 0)
        {
            targetDefence = target.GetComponent<ClassController>().defence;
            damage = attack - targetDefence;
            if (damage < 1)
                damage = 1;
        }
    }

    public void StartRunning()
    {
        animator.SetBool("Moving", true);
    }

    public void StartIdle()
    {
        animator.SetBool("Moving", false);
        transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    }

    public Boolean IsTargetDead(int dmg)
    {
        if (dmg > hp)
            dmg = hp;

        hp -= dmg;

        int direction = 1;
        if (gameObject.tag == "Ally")
            direction = -1;

        HealthBar.transform.localScale = new Vector3((hp * 0.7f) / maxHP, 2.0f, 1.0f);
        HealthBar.transform.localPosition = new Vector3(((maxHP - hp) * 0.7f * direction) / (maxHP * 2), 2.19f, 0f);

        if (hp < 1)
        {
            attackable = false;
            animator.SetBool("Dead", true);
            StartCoroutine(RemoveUnit(2.0f));

            if (gameObject.tag == "Ally")
                levelManager.allyCount--;
            else
                levelManager.enemyCount--;

            return true;
        }

        return false;
    }

    protected void StopAttackWhenBattleFinished()
    {
        if (levelManager)
            if (animator.GetBool("BattleStarted"))
                if (levelManager.IsBattleEnded())
                    animator.SetBool("BattleStarted", false);
    }

    protected void FixInFightingPoint()
    {
        if (walking)
        {
            float positionX = walking.fightDest.transform.position.x;
            float positionZ = walking.fightDest.transform.position.z;

            if (levelManager)
                if (levelManager.battleStarted)
                    if (gameObject.tag == "Ally")
                    {
                        transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                        transform.position = new Vector3(positionX, transform.position.y, positionZ);
                    }
        }
    }

    protected void GoToTarget()
    {
        if (walking)
        {
            walking.target = target.transform;
            walking.GotoTarget();
        }
    }

    protected IEnumerator RemoveUnit(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        gameObject.SetActive(false);
    }

    protected void FindTarget()
    {
        dist = 1000.0f;

        for (int i = 2; i < 11; i++)
            FindNearestTarget(1, i);

        if (!target)
            for (int i = 2; i < 11; i++)
                FindNearestTarget(2, i);

        if (!target)
            FindNearestTarget(1, 1);

        if (!target)
            FindNearestTarget(2, 1);

        if (!target)
            return;

        if (!target.GetComponent<ClassController>().attackable)
            for (int i = 1; i < 11; i++)
                FindNearestTarget(2, i);

        if (!target.GetComponent<ClassController>().attackable)
            for (int i = 1; i < 11; i++)
                FindNearestTarget(3, i);
    }

    private void FindNearestTarget(int LineNumer, int Rotator)
    {
        tempTarget = GameObject.Find(enemyGroupName).transform.Find("Line" + LineNumer.ToString() + "_Pos" + Rotator.ToString()).gameObject;
        if (tempTarget.transform.childCount > 0)
        {
            tempTarget = tempTarget.transform.GetChild(0).gameObject;
            tempDist = Vector3.Distance(transform.position, tempTarget.transform.position);

            if (tempTarget)
                if (tempTarget.GetComponent<ClassController>().attackable)
                {
                    if (target)
                        if (!target.GetComponent<ClassController>().attackable)
                        {
                            target = tempTarget;
                            dist = tempDist;

                            return;
                        }

                    if (tempDist < dist)
                    {
                        target = tempTarget;
                        dist = tempDist;
                    }
                }
        }
    }

    protected void Attack()
    {
        if (target)
            if (!target.GetComponent<ClassController>().attackable)
                FindTarget();

        animator.SetBool("BattleStarted", true);
    }

    public IEnumerator RepeatAttack(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        StartCoroutine(RepeatAttack(UnityEngine.Random.Range(1.5f, 2.5f)));

        if (levelManager.battleStarted && !levelManager.IsBattleEnded())
        {
            Attack();
            bool findNewTarget = true;
            if (target)
                if (attackable)
                    if (target.GetComponent<ClassController>().attackable)
                    {
                        AttackSound();
                        findNewTarget = false;
                    }


            if (findNewTarget)
                FindTarget();
        }
    }

    protected void AttackSound()
    {
        arrowFired = true;
        sfx.PlayOneShot(attackSound);
        StartCoroutine(SetAnimatorFalse());
    }

    private IEnumerator SetAnimatorFalse()
    {
        yield return new WaitForSeconds(0.5f);

        if (animator.GetBool("BattleStarted"))
            animator.SetBool("BattleStarted", false);
    }
}
