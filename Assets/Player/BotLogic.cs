using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BotLogic
{
    public enum botDifficulty
    {
        easy,
        medium,
        hard,
        max
    }

    public enum attackState
    {
        rush,
        holdPosition,
        hitAndRun,
        nimble,
        circler,
        none
    }

    public enum aiState
    {
        wander, //wander the map randomly, avoiding resources
        getAmmo,//head to nearest ammo restore object
        getShield,//head to nearest shield recharge object
        attack,//damage an enemy while taking steps to defend itself
        investigate,//head to spot where target was last seen
        guard,//hold position and look for threats
        flee,//run away to random spot
        none,//placeholder
    }

    class BotHandler
    {
        Transform transform;
        GameInput.Controller myController;
        List<GameObject> navList;
        float reactionTime;
        float reactionClock = float.MinValue;
        float maxReactionTime;
        float reactionDecay = 0.8f;
        int navListIter;
        int maxMisses = 4;
        int misses = 0;
        /*
         * the bot handler is a wrapper for ai functionality. the state machine is still handled within the bot object itself but common functions are stored here
         * the bot handler is not a state machine. the only state it stores is related to reaction time
         * reaction time is also used by the state machine but the state machine and the bot handler have seperate clocks
         * the bot handler mostly uses reaction time to make turning the ship less accurate. any fuctions that involve reaction time take an argument whether it should be used
         * the controller object itself stores a lot of state data too, such as the direction it is turning
         * the bot handler contains a minimum of data itself. data is passed to it by the state machine
         * the state machine is responsible for populating the navList, the pathfinding system plots a course given various inputs. the bot handler is responsible for steering along that course
         * the bot handler has authority over the course. it can take shortcuts for example
         * the bot handler is not responsible for firing weapons, that is the responsibility of the state machine. its methods may be used for fire control in the state machine however
         * where possible the bot handler should not alter the parent transform or rigidbody directly. it should send instructions to the controller. this is not strictly enforced however
         * arguments are passed in the following order: references, required simple datatypes, optional datatypes
         * the parent controller should always be passed by reference and should come first in the arguments
         * when passing items by reference the item to be modified should be the first argument. where possible avoid modifiying multiple referenced objects
         */


        public BotHandler(Transform _transform, ref GameInput.Controller _myController, float _reactionTime = 0)
        {
            transform = _transform;
            myController = _myController;
            reactionTime = _reactionTime;
            maxReactionTime = _reactionTime;
            misses = maxMisses;
        }

        public void ResetReactionTime() { reactionTime = maxReactionTime; misses = maxMisses; }

        public void PlotCourse(ref List<GameObject> _navList)
        {
            navList = _navList;
            navListIter = 0;
        }

        public bool isLookingAt(Vector3 targetPosition, float minAngle, float closeEnough, bool useDelay)
        {
            //
            bool output = false;
            bool isAiming = true;
            bool clearAxes = false;
            if (closeEnough < minAngle) { closeEnough = minAngle; }
            float angDiff = 180 - AngDiff(targetPosition, Vector3.forward);
            float outerAng = 3;
            if (angDiff < closeEnough) { output = true; }
            misses = 0;
            if(angDiff<closeEnough && misses > 0 && reactionClock == float.MinValue && useDelay)
            {
                misses--;
                reactionClock = reactionTime;
                Debug.Log(misses);
            }
            //if (reactionClock > 0) { isAiming = false; reactionClock -= Time.deltaTime; }
            else { reactionClock = float.MinValue; }
            if (isAiming)
            {
                //figure out how to move controller
                Vector2 tempVec = Vector2.zero;
                tempVec.x = targetPosition.x;
                tempVec.y = targetPosition.y;
                tempVec.Normalize();
                float multipilier = angDiff / outerAng;
                if (multipilier < 0.1f) { multipilier = 0.1f; }
                if (angDiff < outerAng)
                {
                    tempVec *= multipilier;
                }
                myController.Filter(ref tempVec, myController.useLookCurve);
                myController.angle.x = -tempVec.y;
                myController.angle.y = tempVec.x;
            }
            if (clearAxes) { myController.angle = Vector3.zero; }
            return output;
        }

        public bool AtPosition(Vector3 targetPosition, bool shouldAccelerate, bool shouldBoost, bool lookAt, float radius, bool useReaction = false)
        {
            float distanceToTarget = Vector3.Magnitude(transform.position - targetPosition);
            if (distanceToTarget < radius) { return true; }
            //avoidance
            Vector3 avoidance = CollisionAvoidance();
            if (avoidance != Vector3.zero)
            {
                lookAt = false;
                shouldBoost = false;
                myController.angle = Vector3.zero;
                //myController.movement = Vector3.zero;
                myController.angle.x = avoidance.y;
                myController.angle.y = -avoidance.x;
                //myController.movement.x = avoidance.y;
                //myController.movement.y = avoidance.x;
                //Vector3.SmoothDamp()
                myController.movement.z = 1;
                shouldAccelerate = false;
            }
            Vector3 targetPos = transform.worldToLocalMatrix.MultiplyPoint(targetPosition);
            bool lookingAt = false;
            if (lookAt)
            {
                lookingAt = isLookingAt(targetPos, 0, 2, useReaction);
            }
            if (shouldBoost && lookingAt)
            {
                myController.isBoosting = true;
            }
            else { myController.isBoosting = false; }
            if (shouldAccelerate)
            {
                myController.movement = targetPos.normalized;
            }
            return false;
        }

        public Vector3 CollisionAvoidance()
        {
            Vector3 outVec = Vector3.zero;
            float rayLength = 10;
            Ray myRay = new Ray();
            RaycastHit hit;
            myRay.origin = Vector3.zero;
            int iter = 0;
            float minDistance = rayLength;
            for (int i = 0; i < 8; i++)
            {
                myRay.origin = Vector3.zero;
                myRay.direction = Vector3.zero;
                switch (i)
                {
                    case 0:
                        myRay.direction += Vector3.right;
                        break;
                    case 1:
                        myRay.direction += Vector3.right;
                        myRay.direction += Vector3.up;
                        break;
                    case 2:
                        myRay.direction += Vector3.up;
                        break;
                    case 3:
                        myRay.direction += Vector3.up;
                        myRay.direction -= Vector3.right;
                        break;
                    case 4:
                        myRay.direction -= Vector3.right;
                        break;
                    case 5:
                        myRay.direction -= Vector3.right;
                        myRay.direction -= Vector3.up;
                        break;
                    case 6:
                        myRay.direction -= Vector3.up;
                        break;
                    case 7:
                        myRay.direction += Vector3.right;
                        myRay.direction -= Vector3.up;
                        break;
                    default:
                        break;
                }
                myRay.direction += Vector3.forward * 2;
                myRay.direction.Normalize();
                
                myRay.origin= transform.TransformPoint(myRay.origin);
                myRay.direction = transform.TransformVector(myRay.direction);
                if (Physics.Raycast(myRay, out hit, rayLength))
                {
                    if (hit.collider.gameObject.tag == "Wall")
                    {
                        if (hit.distance < rayLength) { iter++; }
                        if (hit.distance < minDistance)
                        {
                            minDistance = hit.distance;
                            outVec = myRay.direction;
                        }
                    }
                }
                Debug.DrawLine(myRay.origin, myRay.origin + myRay.direction * rayLength);
            }
            outVec = transform.InverseTransformVector(outVec);
            outVec.z = 0;
            outVec.Normalize();
            return outVec;
        }

        public bool AtNavTarget(float botFov = 180, bool shouldBoost = true, float radius = 10, float endRadius = 0.5f)
        {
            GameObject navTarget;
            if (navList == null) { Debug.LogWarning("got a bad path"); return true; }//catch if no nav list has been generated
            if (navListIter < navList.Count)
            {
                if (isVisible(transform.position, navList[navList.Count - 1], botFov))
                {
                    navListIter = navList.Count - 1;
                }
                navTarget = navList[navListIter];
                if (navListIter == navList.Count - 1) { radius = endRadius; }
                if (AtPosition(navTarget.transform.position, true, shouldBoost, true, radius)) { navListIter++; }
                return false;
            }
            return true;
        }
        
        public bool GetNavItem(ref GameObject navTarget, ref List<GameObject> list)
        {
            while (list.Contains(null)) { list.Remove(null); }
            if (list.Count == 0)
            {
                return false;
            }
            float minDis = float.MaxValue;
            float tempDis;
            foreach (GameObject item in list)
            {
                tempDis = Vector3.Magnitude(transform.position - item.transform.position);
                if (tempDis < minDis)
                {
                    navTarget = item;
                    minDis = tempDis;
                }
            }
            return true;
        }

        public float AngDiff(Vector3 heading, Vector3 targetAng)
        {
            float dot = -Vector3.Dot(heading.normalized, targetAng.normalized);
            float angDiff = Mathf.Acos(dot) * Mathf.Rad2Deg;
            return angDiff;
        }

        public float isVisibleAngle(Vector3 facing, Vector3 startPosition, Vector3 endPosition)
        //returns a the angle between 
        {
            RaycastHit hit;
            float closeEnough = 3;
            bool isVisible = false;
            Vector3 heading = startPosition - endPosition;
            if (Physics.Linecast(startPosition, endPosition, out hit, -1, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(hit.distance - Vector3.Magnitude(startPosition - endPosition)) < closeEnough) { isVisible = true; }
            }
            float angDiff = float.NaN;
            if (isVisible)
            {
                angDiff = AngDiff(facing, heading);
            }
            return angDiff;
        }

        public bool isVisible(Vector3 startPosition, GameObject target, float fov = 180)
        {
            RaycastHit hit;
            float closeEnough = 3;
            if (target == null) { return false; }
            Vector3 endPosition = target.transform.position;
            Vector3 heading = startPosition - endPosition;
            float angDiff = AngDiff(transform.forward, heading);
            if (angDiff > fov && !float.IsNaN(angDiff)) { return false; }
            if (Physics.Linecast(startPosition, endPosition, out hit, -1, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(hit.distance - Vector3.Magnitude(startPosition - endPosition)) < closeEnough) { return true; }
            }
            return false;
        }

        public bool BeginAttack(ref GameObject attackTarget, ref List<GameObject> enemies, float botFov)
        {
            bool output = false;
            float minDis = float.MaxValue;
            float tempDis;
            if (enemies.Count == 0) { return false; }
            foreach (GameObject enemy in enemies)
            {
                if (isVisible(transform.position, enemy, botFov))
                {
                    tempDis = Vector3.Magnitude(transform.position - enemy.transform.position);
                    if (tempDis < minDis) { attackTarget = enemy; minDis = tempDis; output = true; }
                }
            }
            return output;
        }
    }
}
