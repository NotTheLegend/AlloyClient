using System;

namespace AlloyClient.UiLib.Utils;

internal sealed class InvalidCallbackException : Exception {
    
    internal InvalidCallbackException() : base() { }
    
    internal InvalidCallbackException(string msg) : base(msg) { }
    
    internal InvalidCallbackException(string msg, Exception inner) : base(msg, inner) { }
}