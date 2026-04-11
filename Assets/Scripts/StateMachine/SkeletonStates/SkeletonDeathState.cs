using UnityEngine;

public class SkeletonDeathState : IEnemyState
{
    private SkeletonController skeleton;
    public SkeletonDeathState(SkeletonController skeleton) => this.skeleton = skeleton;

    public void Enter()
    {
        skeleton.enemyAnim.PlayDeath();
        skeleton.rb.linearVelocity = Vector2.zero;
        skeleton.rb.bodyType = RigidbodyType2D.Static;
        skeleton.GetComponent<Collider2D>().enabled = false;

        Object.Destroy(skeleton.gameObject, 3f);
    }
    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}