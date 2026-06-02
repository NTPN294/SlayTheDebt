using Assets.Scripts.Content;

/// <summary>
/// Backwards-compatible text shim. All real scripted-event content (including
/// gameplay effects) now lives in <see cref="ScriptedEventDatabase"/>; this class
/// stays so any existing caller of <c>ScriptedEvents.Get(phase)</c> keeps working.
///
/// To add or change a scripted event, edit ScriptedEventDatabase, not this file.
/// </summary>
public static class ScriptedEvents
{
    public static (string title, string description) Get(int phase)
    {
        EventDefinition def = ScriptedEventDatabase.Get(phase);
        if (def == null)
            return ("Life Event", "Something important happens.");

        return (def.Title, def.Description);
    }
}
