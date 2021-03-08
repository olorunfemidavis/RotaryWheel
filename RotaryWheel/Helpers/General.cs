using Newtonsoft.Json;
using RotaryWheel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Essentials;

namespace RotaryWheel.Helpers
{
    public class General
    {
        internal static void DoHaptic(HapticFeedbackType type)
        {
            try
            {
                // Perform click feedback
                // Use default vibration length
                Vibration.Vibrate(TimeSpan.FromMilliseconds(50));
               // Xamarin.Essentials.HapticFeedback.Perform(type);
            }
            catch (FeatureNotSupportedException ex)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }
    }
}
