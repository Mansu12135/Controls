using System;
using System.Collections.Generic;

namespace ApplicationLayer
{
    public interface IBasicPanel<T> : IFileSystemControl where T : Item
    {
        SaveItemManager<T> SaveItemManager { get; }

        Dictionary<string, T> Notes { get; }

        object Save(string project, Action action);
    }
}
