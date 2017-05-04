using System;

namespace Controls
{
    public interface ISettings
    {
        event EventHandler OnDataLoad;
        void SaveSettings();
    }
}
