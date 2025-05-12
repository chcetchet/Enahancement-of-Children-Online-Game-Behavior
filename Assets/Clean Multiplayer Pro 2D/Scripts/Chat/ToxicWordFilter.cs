using System.Collections.Generic;

public static class ToxicWordFilter
{
    private static Dictionary<string, string> _wordReplacements = new Dictionary<string, string>()
    {
        { "Noob", "New player?" },
        { "noob", "New player?" },
        { "Bot", "New player?" },
        { "bot", "New player?" },
        { "Trash", "Rough" },
        { "trash", "Rough" },
        { "Garbage", "Rough" },
        { "garbage", "Rough" },
        { "Carry this loser", "I’ll help!" },
        { "carry this loser", "I’ll help!" },
        { "Loser", "Winner" },
        { "loser", "Winner" },
        { "GG EZ", "GG" },
        { "Gg ez", "GG" },
        { "EZ", "GG" },
        { "ez", "GG" },
        { "Fuck", "Joy" },
        { "fuck", "Joy" },
        { "f***", "Joy" },
        { "wtf", "What's wrong?" },
        { "tf", "What's wrong?" },
        { "fu", "Joy" },
        { "faq", "Joy" },
        { "LOL bad", "Nice try" },
        { "lol bad", "Nice try" },
        { "Stupid", "not cool" },
        { "stupid", "not cool" },
        { "idiot", "Clever" },
        { "Idiot", "Clever" },
        { "Shut up", "Let's talk" },
        { "Kill", "Defeat" },
        { "kill", "defeat" },
        { "Shit", "Ugh" },
        { "shit", "Ugh" },
        { "shut up", "Let's talk" },
        { "Damn", "Wow" },
        { "damn", "Wow" },
        { "Bitch", "Rascal" },
        { "bitch", "Rascal" },
        { "Ass", "Wow, helpful" },
        { "ass", "Wow, helpful" },
        { "Asshole", "Wow, helpful" },
        { "asshole", "Wow, helpful" },
        // Add more words as needed
    };

    public static string FilterMessage(string message)
    {
        foreach (var word in _wordReplacements)
        {
            message = message.Replace(word.Key, word.Value, System.StringComparison.OrdinalIgnoreCase);
        }
        return message;
    }
}