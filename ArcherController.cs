using System.Collections;
using UnityEngine;

public class ArcherController : ClassController
{
    private GameObject arrow;
    private Vector3 firstArrowPosition;
    private Quaternion firstArrowRotation;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FirstWaitingTime(0.1f));
    }

    // Update is called once per frame
    void Update()
    {
        StopAttackWhenBattleFinished();
        FixInFightingPoint();
        if (arrowFired)
        {
            RotateArrow();
            MoveArrow();
        }
    }

    private IEnumerator FirstWaitingTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        StartingFunction();

        StartCoroutine(RepeatAttack(UnityEngine.Random.Range(0.5f, 1.5f)));
        StartCoroutine(SecondWaitingTime(5.5f));
        FindArrow();
        arrowFired = false;
        
        attack = 4 * level;
        defence = 1 * level;
    }

    private IEnumerator SecondWaitingTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        SetArrowsPosition();
        SecondFunction();
    }

    private void FindArrow()
    {
        arrow = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L/Rigged_Bow_Testing/SM_Arrow_01").gameObject;
    }

    private void SetArrowsPosition()
    {
        arrow.transform.SetParent(transform, true);

        firstArrowPosition = arrow.transform.position;
        firstArrowRotation = arrow.transform.rotation;
    }

    private void RotateArrow()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = target.transform.position - arrow.transform.position;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(arrow.transform.forward, targetDirection, 1.0f, 0.0f);

        // Draw a ray pointing at our target in
        UnityEngine.Debug.DrawRay(arrow.transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        arrow.transform.rotation = Quaternion.LookRotation(newDirection);
    }

    private void MoveArrow()
    {
        // Move our position a step closer to the target
        arrow.transform.position = Vector3.MoveTowards(arrow.transform.position, target.transform.position, 0.5f);

        // Check if the position of the cube and sphere are approximately equal.
        if (Vector3.Distance(arrow.transform.position, target.transform.position) < 0.001f)
        {
            if (target)
                if (target.GetComponent<ClassController>().attackable)
                    if (target.GetComponent<ClassController>().IsTargetDead(damage))
                        FindTarget();

            arrow.transform.position = firstArrowPosition;
            arrow.transform.rotation = firstArrowRotation;
            arrowFired = false;
        }
    }
}
