using System;
using OpenTK.Platform;

namespace Alloy.UiLib.Core;

public interface ITextInputTarget {
    void OnTextInput(ReadOnlySpan<char> text);
}

public interface IManualTextInputTarget {
    void OnManualTextInput(Key key);
}