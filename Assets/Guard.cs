using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour {

    public static event System.Action OnPlayerSeen;
    public Transform pathHolder;

    public int speed = 1;
    public float waitSeconds = .3f;
    public int turnSpeedDeg = 90;
    public float timeToSpotPlayer = .5f;

    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    float viewAngle;
    float playerVisibleTimer;

    Transform player;
    Color originalSpotlightColor;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotlight.spotAngle;
        originalSpotlightColor = spotlight.color;

        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++) {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    private void Update() {
        if (CanSeePlayer()) {
            playerVisibleTimer += Time.deltaTime;
        } else {
            playerVisibleTimer -= Time.deltaTime;
        }

        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotlight.color = Color.Lerp(originalSpotlightColor, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if( playerVisibleTimer >= timeToSpotPlayer) {
            if (OnPlayerSeen != null) {
                OnPlayerSeen();
                Player player = FindObjectOfType<Player>();
                speed = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider triggerCollider) { 
        if (triggerCollider.tag == "WinZone") {
            print("C'est la grosse win, GG!");
        }
    }

    bool CanSeePlayer() {
        // Check if the player is within our vision distance
        if (Vector3.Distance(transform.position, player.position) < viewDistance) {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            // Check if the player is within our vision angle
            if(angleBetweenGuardAndPlayer < viewAngle / 2f) {
                // Check for obstacles in the view
                if (!Physics.Linecast(transform.position, player.position, viewMask)) {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator FollowPath(Vector3[] waypoints) {
        transform.position = waypoints[0];        
        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while(true) {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if(transform.position == targetWaypoint) {
                targetWaypointIndex++;
                // Reset index if we're at the last waypoint
                if (targetWaypointIndex == waypoints.Length ) {
                    targetWaypointIndex = 0;
                }
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitSeconds);
                yield return StartCoroutine(TurntoFace(targetWaypoint));
            }

            yield return null;
        }
    }

    IEnumerator TurntoFace(Vector3 lookTarget) {
        Vector3 directionToLookTarget = (lookTarget - transform.position).normalized;

        // 90 - ... because the Unity "angle circle" is oriented towards the z axis
        // cf https://youtu.be/-dGi2Ffdiuk?list=PLFt_AvWsXl0fnA91TcmkRyhhixX9CO3Lw&t=498
        //
        // atan2(z,x) gives the angle value in radians of the direction we want (here the target)
        float targetAngle = 90 - Mathf.Atan2(directionToLookTarget.z, directionToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f) {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeedDeg * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;

            yield return null;
        }
        
    }

    private void OnDrawGizmos() {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        Gizmos.color = Color.white;
        foreach (Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position, .2f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);
    }
}
