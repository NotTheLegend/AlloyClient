using System;

namespace MonoClient.UiLib;

internal sealed class InvalidCallbackException : Exception {
    
    internal InvalidCallbackException() : base() { }
    
    internal InvalidCallbackException(string msg) : base(msg) { }
    
    internal InvalidCallbackException(string msg, Exception inner) : base(msg, inner) { }
}