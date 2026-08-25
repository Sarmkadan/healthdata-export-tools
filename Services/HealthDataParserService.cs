using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthDataExportTools.Services
{
    public class HealthDataParserService
    {
        public HealthDataParserService()
        {
        }

        public override string ToString()
        {
            return $"HealthDataParserService {{ SleepRecords = SleepRecords, HeartRateRecords = HeartRateRecords, SpO2Records = SpO2Records, StepsRecords = StepsRecords, ActivityRecords = ActivityRecords, Metrics = Metrics }}";
        }
    }
}