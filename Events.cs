using System;
using System.Collections.Generic;

namespace EventConstructor
{

    public delegate void ConfigurationLoadedEventHandler(ConfigurationLoadedEventArgs args);

    // Define a class that contains the event
    public class EventRegistry
    {

        // Declare the events
        public event ConfigurationLoadedEventHandler ConfigurationLoaded;

        // Add methods to raise the event
        public void OnConfigurationLoaded(Dictionary<string, object> configObj)
        {
            ConfigurationLoaded?.Invoke(new ConfigurationLoadedEventArgs(configObj));
        }

    }

    public class ConfigurationLoadedEventArgs : EventArgs
    {

        public Dictionary<string, object> HandledConfiguration { get; set; }

        public ConfigurationLoadedEventArgs(Dictionary<string, object> configObj)
        {
            HandledConfiguration = configObj;
        }

    }

}
