public interface IMeleeEnemyBehaviour
{
    void InitReferences();
    void MakeAttack();

    void SubscribeReceiveDamage();
    void SubscribeDeath();

}