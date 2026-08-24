using Alloy.UiLib.Core;
using AlloyClient.AppEngine;
using AlloyClient.Display;

namespace AlloyClient.Screens.Components.Containers.Account;

public class LoginContainer : AccountFrame {
    public static readonly EventType<Event> LoginEvent = "loginSuccess";

    private readonly AccountFormField _usernameInput;
    private readonly AccountFormField _passwordInput;

    private bool _requestPending;

    public LoginContainer() : base("Sign in", 350) {
        _usernameInput = AddField("Username", 68, maxCharacters: 32);
        _passwordInput = AddField("Password", 150, password: true, maxCharacters: 64);

        AddNavigation("New user? Click here to Register!", 217,
            () => OverlayManager.Set(new RegisterContainer()));

        AddActions("Cancel", CloseOverlay, "Sign in", OnLogin);
        AddEventListener(Event.AddedToStage, _usernameInput.Focus);
    }

    private void OnLogin() {
        if (_requestPending || !ValidateInputs()) {
            return;
        }

        ClearStatus();
        _requestPending = true;
        SetActionsEnabled(false);
        AddEventListener(AppRequests.VerifyAsync(_usernameInput.Text, _passwordInput.Text, true), OnLoginResponse);
    }

    private bool ValidateInputs() {
        var valid = true;
        if (!_usernameInput.HasText()) {
            _usernameInput.SetError("Username is required");
            valid = false;
        }

        if (!_passwordInput.HasText()) {
            _passwordInput.SetError("Password is required");
            valid = false;
        }

        return valid;
    }

    private void OnLoginResponse(AppResponse response) {
        _requestPending = false;
        SetActionsEnabled(true);

        if (!response.Success) {
            SetStatus(response.Message);
            return;
        }

        CloseOverlay();
        DispatchEvent(new Event(LoginEvent));
    }
}