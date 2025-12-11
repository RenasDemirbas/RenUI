using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RenUI.Core.Events;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;
using RenUI.Input;
using RenUI.Rendering;

namespace RenUI.Elements.Controls;

public class TextBox : UIElement
{
    private string _text = string.Empty;
    private SpriteFont? _font;
    private int _cursorPosition;
    private int _selectionStart;
    private int _selectionLength;
    private float _cursorBlinkTimer;
    private bool _cursorVisible = true;
    private int _scrollOffset;

    private const float CursorBlinkInterval = 0.5f;

    public string Text
    {
        get => _text;
        set
        {
            var newText = value ?? string.Empty;
            if (_text != newText)
            {
                _text = newText;
                _cursorPosition = Math.Min(_cursorPosition, _text.Length);
                ClearSelection();
                OnTextChanged.Dispatch(new UIEventArgs(this));
                MarkDirty();
            }
        }
    }

    public SpriteFont? Font
    {
        get => _font;
        set
        {
            _font = value;
            MarkDirty();
        }
    }

    public int CursorPosition
    {
        get => _cursorPosition;
        set => _cursorPosition = Math.Clamp(value, 0, _text.Length);
    }

    public string Placeholder { get; set; } = string.Empty;
    public Color TextColor { get; set; } = Color.White;
    public Color PlaceholderColor { get; set; } = Color.Gray;
    public Color BackgroundColor { get; set; } = new Color(30, 30, 30);
    public Color FocusedBorderColor { get; set; } = new Color(0, 120, 215);
    public Color BorderColor { get; set; } = new Color(60, 60, 60);
    public Color SelectionColor { get; set; } = new Color(0, 120, 215, 128);
    public Color CursorColor { get; set; } = Color.White;
    public int BorderThickness { get; set; } = 1;
    public int MaxLength { get; set; } = int.MaxValue;
    public bool IsReadOnly { get; set; }
    public bool IsPassword { get; set; }
    public char PasswordChar { get; set; } = '*';

    public EventDispatcher<UIEventArgs> OnTextChanged { get; } = new();
    public EventDispatcher<UIEventArgs> OnSubmit { get; } = new();

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (IsFocused)
        {
            _cursorBlinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cursorBlinkTimer >= CursorBlinkInterval)
            {
                _cursorBlinkTimer = 0;
                _cursorVisible = !_cursorVisible;
            }
        }
    }

    public override bool HandleMouseInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;

        var mousePos = inputState.MousePosition;
        IsHovered = Contains(mousePos);

        if (IsHovered && inputState.IsMouseButtonPressed(MouseButton.Left))
        {
            if (!IsFocused)
            {
                OnFocusGained();
            }

            _cursorPosition = GetCharacterIndexAtPosition(mousePos.X);
            ClearSelection();
            ResetCursorBlink();
            return true;
        }

        return false;
    }

    public override bool HandleKeyboardInput(IInputState inputState)
    {
        if (!IsEnabled || !IsFocused || IsReadOnly) return false;

        var keyboard = inputState.CurrentKeyboardState;
        bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        bool ctrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);

        foreach (Keys key in Enum.GetValues<Keys>())
        {
            if (!inputState.IsKeyPressed(key)) continue;

            if (HandleSpecialKey(key, shift, ctrl))
            {
                ResetCursorBlink();
                return true;
            }

            if (!KeyboardHelper.IsModifierKey(key) && !KeyboardHelper.IsNavigationKey(key))
            {
                bool capsLock = keyboard.CapsLock;
                char? c = KeyboardHelper.KeyToChar(key, shift, capsLock);
                if (c.HasValue && _text.Length < MaxLength)
                {
                    DeleteSelection();
                    _text = _text.Insert(_cursorPosition, c.Value.ToString());
                    _cursorPosition++;
                    OnTextChanged.Dispatch(new UIEventArgs(this));
                    ResetCursorBlink();
                    return true;
                }
            }
        }

        return false;
    }

    private bool HandleSpecialKey(Keys key, bool shift, bool ctrl)
    {
        switch (key)
        {
            case Keys.Back:
                if (HasSelection())
                {
                    DeleteSelection();
                }
                else if (_cursorPosition > 0)
                {
                    _text = _text.Remove(_cursorPosition - 1, 1);
                    _cursorPosition--;
                    OnTextChanged.Dispatch(new UIEventArgs(this));
                }
                return true;

            case Keys.Delete:
                if (HasSelection())
                {
                    DeleteSelection();
                }
                else if (_cursorPosition < _text.Length)
                {
                    _text = _text.Remove(_cursorPosition, 1);
                    OnTextChanged.Dispatch(new UIEventArgs(this));
                }
                return true;

            case Keys.Left:
                if (shift) ExtendSelection(-1);
                else
                {
                    ClearSelection();
                    _cursorPosition = Math.Max(0, _cursorPosition - 1);
                }
                return true;

            case Keys.Right:
                if (shift) ExtendSelection(1);
                else
                {
                    ClearSelection();
                    _cursorPosition = Math.Min(_text.Length, _cursorPosition + 1);
                }
                return true;

            case Keys.Home:
                if (shift) ExtendSelectionTo(0);
                else
                {
                    ClearSelection();
                    _cursorPosition = 0;
                }
                return true;

            case Keys.End:
                if (shift) ExtendSelectionTo(_text.Length);
                else
                {
                    ClearSelection();
                    _cursorPosition = _text.Length;
                }
                return true;

            case Keys.Enter:
                OnSubmit.Dispatch(new UIEventArgs(this));
                return true;

            case Keys.A when ctrl:
                SelectAll();
                return true;

            case Keys.C when ctrl:
                // Copy would require clipboard access
                return true;

            case Keys.V when ctrl:
                // Paste would require clipboard access
                return true;

            case Keys.X when ctrl:
                // Cut would require clipboard access
                return true;
        }

        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible) return;

        var bounds = GetAbsoluteBounds();

        PrimitiveRenderer.DrawRectangle(spriteBatch, bounds, BackgroundColor);
        PrimitiveRenderer.DrawRectangleBorder(spriteBatch, bounds, IsFocused ? FocusedBorderColor : BorderColor, BorderThickness);

        if (_font == null) return;

        var textBounds = new Rectangle(
            bounds.X + Padding.Left + 4,
            bounds.Y + Padding.Top,
            bounds.Width - Padding.Horizontal - 8,
            bounds.Height - Padding.Vertical);

        var displayText = GetDisplayText();

        if (HasSelection() && IsFocused)
        {
            DrawSelection(spriteBatch, textBounds);
        }

        if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(Placeholder) && !IsFocused)
        {
            var placeholderPos = new Vector2(textBounds.X, textBounds.Y + (textBounds.Height - _font.LineSpacing) / 2);
            spriteBatch.DrawString(_font, Placeholder, placeholderPos, PlaceholderColor);
        }
        else if (!string.IsNullOrEmpty(displayText))
        {
            var textPos = new Vector2(textBounds.X - _scrollOffset, textBounds.Y + (textBounds.Height - _font.LineSpacing) / 2);
            spriteBatch.DrawString(_font, displayText, textPos, TextColor);
        }

        if (IsFocused && _cursorVisible)
        {
            DrawCursor(spriteBatch, textBounds);
        }
    }

    private string GetDisplayText()
    {
        if (IsPassword)
        {
            return new string(PasswordChar, _text.Length);
        }
        return _text;
    }

    private void DrawCursor(SpriteBatch spriteBatch, Rectangle textBounds)
    {
        if (_font == null) return;

        var displayText = GetDisplayText();
        var textBeforeCursor = displayText[.._cursorPosition];
        var cursorX = textBounds.X + (int)_font.MeasureString(textBeforeCursor).X - _scrollOffset;

        var cursorRect = new Rectangle(cursorX, textBounds.Y + 2, 2, textBounds.Height - 4);
        PrimitiveRenderer.DrawRectangle(spriteBatch, cursorRect, CursorColor);
    }

    private void DrawSelection(SpriteBatch spriteBatch, Rectangle textBounds)
    {
        if (_font == null || _selectionLength == 0) return;

        var displayText = GetDisplayText();
        int start = Math.Min(_selectionStart, _selectionStart + _selectionLength);
        int end = Math.Max(_selectionStart, _selectionStart + _selectionLength);

        var textBeforeSelection = displayText[..start];
        var selectedText = displayText[start..end];

        var selectionX = textBounds.X + (int)_font.MeasureString(textBeforeSelection).X - _scrollOffset;
        var selectionWidth = (int)_font.MeasureString(selectedText).X;

        var selectionRect = new Rectangle(selectionX, textBounds.Y + 2, selectionWidth, textBounds.Height - 4);
        PrimitiveRenderer.DrawRectangle(spriteBatch, selectionRect, SelectionColor);
    }

    private int GetCharacterIndexAtPosition(int x)
    {
        if (_font == null) return 0;

        var bounds = GetAbsoluteBounds();
        var textX = x - bounds.X - Padding.Left - 4 + _scrollOffset;
        var displayText = GetDisplayText();

        for (int i = 0; i <= displayText.Length; i++)
        {
            var width = _font.MeasureString(displayText[..i]).X;
            if (width >= textX)
            {
                return i > 0 && textX < width - _font.MeasureString(displayText[(i - 1)..i]).X / 2 ? i - 1 : i;
            }
        }

        return displayText.Length;
    }

    private void ResetCursorBlink()
    {
        _cursorBlinkTimer = 0;
        _cursorVisible = true;
    }

    private bool HasSelection() => _selectionLength != 0;

    private void ClearSelection()
    {
        _selectionStart = _cursorPosition;
        _selectionLength = 0;
    }

    private void SelectAll()
    {
        _selectionStart = 0;
        _selectionLength = _text.Length;
        _cursorPosition = _text.Length;
    }

    private void ExtendSelection(int direction)
    {
        int newPos = Math.Clamp(_cursorPosition + direction, 0, _text.Length);
        _selectionLength += _cursorPosition - newPos;
        _cursorPosition = newPos;
    }

    private void ExtendSelectionTo(int position)
    {
        _selectionLength = _selectionStart - position;
        _cursorPosition = position;
    }

    private void DeleteSelection()
    {
        if (!HasSelection()) return;

        int start = Math.Min(_selectionStart, _selectionStart + _selectionLength);
        int length = Math.Abs(_selectionLength);

        _text = _text.Remove(start, length);
        _cursorPosition = start;
        ClearSelection();
        OnTextChanged.Dispatch(new UIEventArgs(this));
    }
}
