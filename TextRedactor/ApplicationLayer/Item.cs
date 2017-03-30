using System;

namespace ApplicationLayer
{
    [Serializable]
    public abstract class Item
    {
      public virtual string Name { get; set; }
    }
}
