using System.Linq;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.AppEngine;
using AlloyClient.Display;

namespace AlloyClient.Screens.Components.Containers.Account;

public class RegisterContainer : AccountFrame {
    private readonly AccountFormField _usernameInput;
    private readonly AccountFormField _passwordInput;
    private readonly AccountFormField _confirmPasswordInput;

    private bool _requestPending;

    public RegisterContainer() : base("Register in order to play", 430) {
        _usernameInput = AddField("Username", 68, maxCharacters: 10);
        _passwordInput = AddField("Password", 150, password: true, maxCharacters: 64);
        _confirmPasswordInput = AddField("Retype Password", 232, password: true, maxCharacters: 64);

        AddNavigation("Already registered? Click here to sign in!", 299,
            () => OverlayManager.Set(new LoginContainer()));

        AddActions("Cancel", CloseOverlay, "Register", OnRegister);
        AddEventListener(Event.AddedToStage, _usernameInput.Focus);
    }

    private void OnRegister() {
        if (_requestPending || !ValidateInputs()) {
            return;
        }

        _requestPending = true;
        SetActionsEnabled(false);
        AddEventListener(AppRequests.Register(_usernameInput.Text, _passwordInput.Text), OnRegisterResponse);
    }

    private bool ValidateInputs() {
        _usernameInput.ClearError();
        _passwordInput.ClearError();
        _confirmPasswordInput.ClearError();

        var errors = 0;
        var username = _usernameInput.Text;
        if (string.IsNullOrWhiteSpace(username) || username.Length > 10 || !username.All(char.IsLetter)) {
            _usernameInput.SetError("Use 1-10 letters only");
            errors++;
        }

        if (string.IsNullOrWhiteSpace(_passwordInput.Text) || _passwordInput.Text.Length < 9) {
            _passwordInput.SetError("Password must be at least 9 characters");
            errors++;
        }

        if (_passwordInput.Text != _confirmPasswordInput.Text) {
            _confirmPasswordInput.SetError("Passwords do not match");
            errors++;
        }

        if (errors == 0) {
            ClearStatus();
            return true;
        }

        SetStatus(errors == 1 ? "Please fix the error below" : "Please fix the errors below");
        return false;
    }

    private void OnRegisterResponse(AppResponse response) {
        _requestPending = false;
        SetActionsEnabled(true);

        if (!response.Success) {
            SetStatus(response.Message);
            return;
        }

        CloseOverlay();
        ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 500, 0x0);
    }
}