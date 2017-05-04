using System;

namespace ApplicationLayer
{
    public interface ISettings
    {
        event EventHandler OnDataLoad;
        void SaveSettings();
    }
}
