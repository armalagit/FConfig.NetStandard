using System;
using System.Collections.Generic;

namespace EventConstructor
{

    // Declare a delegate for the ConfigurationLoaded event
    public delegate void ConfigurationLoadedEventHandler(ConfigurationLoadedEventArgs args);

    // Define a class that contains the event
    public class EventRegistry {
        // Declare the ConfigurationLoaded event
        public event ConfigurationLoadedEventHandler ConfigurationLoaded;

        // Add a method to raise the ConfigurationLoaded event
        public void OnConfigurationLoaded(Dictionary<string, object> configObj) {
            ConfigurationLoaded?.Invoke(new ConfigurationLoadedEventArgs(configObj));
        }
    }

    // Define a class that holds the event arguments for the ConfigurationLoaded event
    public class ConfigurationLoadedEventArgs : EventArgs {
        // Define a property for the handled configuration object
        public Dictionary<string, object> HandledConfiguration { get; set; }

        // Define a constructor that initializes the handled configuration object
        public ConfigurationLoadedEventArgs(Dictionary<string, object> configObj) {
            HandledConfiguration = configObj;
        }
    }
    
}
