using UnityEngine;

public class SatyrDeathState : IEnemyState
{
    private SatyrController satyr;
    public SatyrDeathState(SatyrController satyr) => this.satyr = satyr;

    public void Enter()
    {
        satyr.enemyAnim.PlayDeath();
        satyr.rb.linearVelocity = Vector2.zero;
        satyr.rb.bodyType = RigidbodyType2D.Static;
        satyr.GetComponent<Collider2D>().enabled = false;

        // 3 saniye sonra sahneden sil
        Object.Destroy(satyr.gameObject, 2f);
    }
    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}