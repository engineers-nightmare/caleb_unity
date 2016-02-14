using UnityEngine;
using System.Collections.Generic;

public class ClimbMovement : MonoBehaviour {

    public Handhold CurrentHold;
    public Handhold NextHold;
    float moveT = 0.0f;

    public float MaxGrabDistance = 0.85f;
    public float IdealGrabDistance = 0.55f;
    public float MaxClimbDistance = 0.9f;
    public float MinGrabCosAngle = 0.2f;
    public float MoveDeadzone = 0.1f;
    public float MinContinueMoveCosAngle = 0.0f;
    public float MinSelectHoldCosAngle = 0.2f;
    public float MoveSpeed = 0.05f;
    public float PosBlendFactor = 0.1f;
    public float NearbyHoldsRadius = 4.0f;

	// Use this for initialization
	void Start () {
        CurrentHold = null;
        NextHold = null;
	}
	
	// Update is called once per frame
	void Update () {
	    if (CurrentHold == null)
        {
            /* Trying to grab on */
            CurrentHold = GetNearestHold(GetNearbyHoldComponents());
        }
        else
        {
            /* Climbing */

            /* Desired move direction in world space */
            var move = this.transform.TransformDirection(
                new Vector3(Input.GetAxis("Horizontal"),
                            Input.GetAxis("Vertical")));

            if (move.sqrMagnitude >= MoveDeadzone)
            {
                if (NextHold == null)
                {
                    /* Try to find a new hold in that direction */
                    NextHold = GetBestHoldInDirection(move, GetNearbyHoldComponents());
                }
                else
                {
                    /* Evaluate whether this is still a good direction to go, otherwise back out */
                    var moveDir = move.normalized;
                    var holdDir = (NextHold.p - CurrentHold.p).normalized;

                    var speedT = move.magnitude * MoveSpeed / (NextHold.p - CurrentHold.p).magnitude;
                    if (Vector3.Dot(moveDir, holdDir) > MinContinueMoveCosAngle)
                    {
                        moveT += speedT;
                        if (moveT > 1.0f)
                        {
                            CurrentHold = NextHold;
                            NextHold = null;
                            moveT = 0.0f;
                        }
                    }
                    else
                    {
                        moveT -= speedT;
                        if (moveT < 0.0f)
                        {
                            NextHold = null;
                            moveT = 0.0f;
                        }
                    }
                }
            }

            /* Fixup transform */
            Vector3 idealPos, idealDir;
            if (NextHold != null)
            {
                idealPos = Vector3.Lerp(
                    CurrentHold.p + CurrentHold.n * IdealGrabDistance,
                    NextHold.p + NextHold.n * IdealGrabDistance,
                    moveT);
                idealDir = Vector3.Lerp(
                    -CurrentHold.n,
                    -NextHold.n,
                    moveT);
            }
            else
            {
                idealPos = CurrentHold.p + CurrentHold.n * IdealGrabDistance;
                idealDir = -CurrentHold.n;
            }

            /* Blend pos toward ideal */
            gameObject.transform.position = Vector3.Lerp(idealPos, gameObject.transform.position, PosBlendFactor);

            /* Find a suitable rotation */
            var q = Quaternion.FromToRotation(gameObject.transform.forward, idealDir);
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, q * gameObject.transform.rotation, PosBlendFactor);
        }
	}

    Handholds[] GetNearbyHoldComponents()
    {
        var hits = Physics.OverlapSphere(gameObject.transform.position, NearbyHoldsRadius);
        var list = new List<Handholds>();
        foreach( var h in hits )
        {
            var holds = h.GetComponent<Handholds>();
            if (holds != null)
                list.Add(holds);
        }

        return list.ToArray();
    }

    Handhold GetBestHoldInDirection(Vector3 dir, Handholds[] components)
    {
        var bestDot = MinSelectHoldCosAngle;
        var pos = gameObject.transform.position;
        Handhold hold = null;

        foreach (var c in components)
        {
            foreach (var h in c.handholds)
            {
                if (h.isBlocked)
                    continue;       /* dont try going anywhere we cant reach */

                if (h == CurrentHold)
                    continue;       /* ignore current */

                var disp = h.p - pos;
                if (disp.sqrMagnitude > MaxClimbDistance * MaxClimbDistance)
                    continue;       /* don't go anywhere out of reach */

                var thisDot = Vector3.Dot(disp.normalized, dir);
                if (thisDot < bestDot)
                    continue;

                bestDot = thisDot;
                hold = h;
            }
        }

        return hold;
    }

    Handhold GetNearestHold(Handholds[] components)
    {
        var bestDistanceSqr = MaxGrabDistance * (MaxGrabDistance + 1);
        var pos = gameObject.transform.position;
        Handhold hold = null;

        foreach (var c in components)
        {
            foreach (var h in c.handholds)
            {
                if (h.isBlocked)
                    continue;           /* blocked holds are never a candidate */

                if (Vector3.Dot(h.n, (pos - h.p).normalized) < MinGrabCosAngle)
                    continue;           /* we're behind this hold's cone of acceptability */

                var thisDistSqr = (pos - h.p).sqrMagnitude;
                if (thisDistSqr >= bestDistanceSqr)
                    continue;           /* not best candidate by distance */

                bestDistanceSqr = thisDistSqr;
                hold = h;
            }
        }

        return hold;
    }
}
