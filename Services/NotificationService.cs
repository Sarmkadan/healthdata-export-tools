using System;
using System.Collections.Generic;

namespace HealthDataExport.Tools
{
    public class NotificationService
    {
        // Existing code...

        public override string ToString()
        {
            return $"NotificationService {{ Subject = {Subject}, Body = {Body}, Type = {Type}, Timestamp = {Timestamp}, Metadata = {Metadata} }}";
        }
    }
}