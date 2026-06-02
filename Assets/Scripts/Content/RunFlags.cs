using System.Collections.Generic;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// Per-run flag store + short-term event cooldown queue.
    /// Cleared on a new run by StartScript.click().
    ///
    /// Two purposes:
    ///  1. Get/Set/RunOncePerRun flags (e.g. "once_pregnancy_news") so big
    ///     events can fire at most once per run.
    ///  2. RecordEvent/RecentlySeen — FIFO queue of recently spawned event
    ///     ids so the EventDatabase pool skips them for a few draws and runs
    ///     stop feeling repetitive.
    /// </summary>
    public static class RunFlags
    {
        const int RecentCooldownSize = 4;

        static readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        static readonly Queue<string> _recentEvents = new Queue<string>();

        // ---- Boolean flags (long-lived, run-scoped) ----

        public static bool Get(string key)
        {
            return !string.IsNullOrEmpty(key)
                && _flags.TryGetValue(key, out bool value)
                && value;
        }

        public static void Set(string key, bool value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _flags[key] = value;
        }

        // ---- Recent-event cooldown (short-lived) ----

        public static void RecordEvent(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_recentEvents.Contains(id)) return;

            _recentEvents.Enqueue(id);
            while (_recentEvents.Count > RecentCooldownSize)
                _recentEvents.Dequeue();
        }

        public static bool RecentlySeen(string id)
        {
            return !string.IsNullOrEmpty(id) && _recentEvents.Contains(id);
        }

        // ---- Reset ----

        public static void Reset()
        {
            _flags.Clear();
            _recentEvents.Clear();
        }

        /// <summary>Convenience key for OncePerRun events.</summary>
        public static string OnceKey(string eventId) => "once_" + eventId;
    }
}
