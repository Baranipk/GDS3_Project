using UnityEngine;

public class BatPatrolState : IEnemyState
{
    private BatController bat;
    private int currentWaypointIndex = 0;

    public BatPatrolState(BatController batController)
    {
        this.bat = batController;
    }

    public void Enter()
    {
        // Gerekirse uçma animasyonunu yavaşa alabilirsin
        // bat.anim.Play("BatFlySlow");
    }

    public void Update()
    {
        // 1. OYUNCU KONTROLÜ: Oyuncu menzile girdi mi?
        float distanceToPlayer = Vector2.Distance(bat.transform.position, bat.player.position);
        if (distanceToPlayer <= bat.detectionRadius)
        {
            bat.StateMachine.ChangeState(bat.chaseState);
            return;
        }

        // 2. DEVRİYE HAREKETİ: Eğer waypoint yoksa olduğu yerde kalsın
        if (bat.waypoints.Length == 0) return;

        Transform targetWaypoint = bat.waypoints[currentWaypointIndex];

        // Hedefe doğru ilerle
        bat.transform.position = Vector2.MoveTowards(bat.transform.position, targetWaypoint.position, bat.patrolSpeed * Time.deltaTime);

        // Yönünü hedefe çevir
        bat.CheckFlip(targetWaypoint.position.x - bat.transform.position.x);

        // Hedefe çok yaklaştıysa bir sonraki noktaya geç
        if (Vector2.Distance(bat.transform.position, targetWaypoint.position) < 0.2f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= bat.waypoints.Length)
            {
                currentWaypointIndex = 0; // Başa dön
            }
        }
    }

    public void FixedUpdate() { }
    public void Exit() { }
}