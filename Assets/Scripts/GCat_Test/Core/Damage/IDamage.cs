namespace GCat_Test.Core.Damage
{
    public interface IDamage
    {
        public bool IsDead { get; }
        public void TakeDamage();
        public void DestroySelf();
    }
}