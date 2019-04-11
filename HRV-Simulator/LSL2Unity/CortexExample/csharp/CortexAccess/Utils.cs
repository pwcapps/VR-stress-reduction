using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    public enum StreamID : int
    {
        CORTEX_VERSION_STREAM = 10,
        AUTHORIZE_STREAM,
        SESSION_STREAM,
        HEADSETS_STREAM,
        MOTION_STREAM,
        EEG_STREAM,
        DEVICE_STREAM,
        PERF_METRICS_STREAM,
        SYS_STREAM,
        MENTAL_CMD_DATA_STREAM,
        FACIAL_EXP_DATA_STREAM,
        TRAINING_STREAM,
        MENTAL_CMD_TRAIN_STREAM,
        FACIAL_EXP_TRAIN_STREAM
    }

    public enum TrainingControl : int
    {
        START_TRAINING = 1,
        ACCEPT_TRAINING,
        REJECT_TRAINING,
        RESET_TRAINING
    }

    public class Utils
    {
        public static Int64 GetEpochTimeNow()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            Int64 secondsSinceEpoch = (Int64)t.TotalMilliseconds;
            return secondsSinceEpoch;

        }
    }
}
