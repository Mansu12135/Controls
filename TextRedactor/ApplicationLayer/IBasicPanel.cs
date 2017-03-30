namespace ApplicationLayer
{
    public interface IBasicPanel<out T> where T : Item
    {
        object Save(string Name);
    }
}
