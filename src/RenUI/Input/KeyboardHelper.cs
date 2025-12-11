using Microsoft.Xna.Framework.Input;

namespace RenUI.Input;

public static class KeyboardHelper
{
    private static readonly Dictionary<Keys, char> ShiftKeyMap = new()
    {
        { Keys.D1, '!' }, { Keys.D2, '@' }, { Keys.D3, '#' }, { Keys.D4, '$' },
        { Keys.D5, '%' }, { Keys.D6, '^' }, { Keys.D7, '&' }, { Keys.D8, '*' },
        { Keys.D9, '(' }, { Keys.D0, ')' }, { Keys.OemMinus, '_' }, { Keys.OemPlus, '+' },
        { Keys.OemOpenBrackets, '{' }, { Keys.OemCloseBrackets, '}' }, { Keys.OemPipe, '|' },
        { Keys.OemSemicolon, ':' }, { Keys.OemQuotes, '"' }, { Keys.OemComma, '<' },
        { Keys.OemPeriod, '>' }, { Keys.OemQuestion, '?' }, { Keys.OemTilde, '~' }
    };

    private static readonly Dictionary<Keys, char> NormalKeyMap = new()
    {
        { Keys.D1, '1' }, { Keys.D2, '2' }, { Keys.D3, '3' }, { Keys.D4, '4' },
        { Keys.D5, '5' }, { Keys.D6, '6' }, { Keys.D7, '7' }, { Keys.D8, '8' },
        { Keys.D9, '9' }, { Keys.D0, '0' }, { Keys.OemMinus, '-' }, { Keys.OemPlus, '=' },
        { Keys.OemOpenBrackets, '[' }, { Keys.OemCloseBrackets, ']' }, { Keys.OemPipe, '\\' },
        { Keys.OemSemicolon, ';' }, { Keys.OemQuotes, '\'' }, { Keys.OemComma, ',' },
        { Keys.OemPeriod, '.' }, { Keys.OemQuestion, '/' }, { Keys.OemTilde, '`' },
        { Keys.Space, ' ' }
    };

    public static char? KeyToChar(Keys key, bool shift, bool capsLock)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            bool upper = shift ^ capsLock;
            char c = (char)('a' + (key - Keys.A));
            return upper ? char.ToUpper(c) : c;
        }

        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
        {
            return (char)('0' + (key - Keys.NumPad0));
        }

        if (shift && ShiftKeyMap.TryGetValue(key, out char shiftChar))
        {
            return shiftChar;
        }

        if (NormalKeyMap.TryGetValue(key, out char normalChar))
        {
            return normalChar;
        }

        return null;
    }

    public static bool IsModifierKey(Keys key) => key switch
    {
        Keys.LeftShift or Keys.RightShift or
        Keys.LeftControl or Keys.RightControl or
        Keys.LeftAlt or Keys.RightAlt or
        Keys.LeftWindows or Keys.RightWindows => true,
        _ => false
    };

    public static bool IsNavigationKey(Keys key) => key switch
    {
        Keys.Left or Keys.Right or Keys.Up or Keys.Down or
        Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown => true,
        _ => false
    };
}
