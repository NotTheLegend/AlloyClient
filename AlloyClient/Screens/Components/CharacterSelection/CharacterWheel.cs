using System;
using System.Collections.Generic;
using System.Linq;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Assets.Libraries;
using AlloyClient.Ui.Components.Buttons;
using OpenTK.Mathematics;

namespace AlloyClient.Screens.Components.CharacterSelection;

public class CharacterWheel : Container {
    private const int CenterX = Settings.DefaultScreenWidth / 2;
    private const int CenterY = Settings.DefaultScreenHeight / 2 + 200;

    private static int ClassesLength => ObjectLibrary.TypeToClassProps.Count;

    private readonly List<ClassRect> _classRects = [];

    private float _rotationAngle = 4.81f;

    // animate rotation
    private const float YOffsetMultiplier = 20f;

    private readonly List<float> _snapAngles = [];
    private float _targetRotationAngle;
    private float _startRotationAngle;
    private float _animationTimer;
    private float _animationDuration = 0.4f;
    private bool _isAnimating;

    // parse from player xml later
    public int CurrentCharacterIndex;
    public ClassRect SelectedClass;

    public CharacterWheel() {
        var i = 0;
        foreach (var kvp in ObjectLibrary.TypeToClassProps) {
            var classRect = new ClassRect(i, kvp.Key);
            _classRects.Add(classRect);
            AddChild(classRect);
            i++;
        }

        UpdateCharacterWheel(0);

        var spincfg = new TextButtonConfig
            { Text = "Forward", FontSize = 50, OnClicked = RotateToNextCharacter, FontType = FontType.Normal, X = 75, Y = 485 };

        var spinButton = new TextButton(spincfg);
        AddChild(spinButton);

        var backcfg = new TextButtonConfig
            { Text = "Back", FontSize = 50, OnClicked = RotateToPreviousCharacter, FontType = FontType.Normal, X = 75, Y = 560 };

        var spinBackButton = new TextButton(backcfg);
        AddChild(spinBackButton);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void UpdateCharacterWheel(float angle) {
        var baseY = CenterY + 50f;
        var angleStep = MathF.Tau / ClassesLength;
        for (var i = 0; i < ClassesLength; i++) {
            _snapAngles.Add(_rotationAngle - i * angleStep);
        }

        _rotationAngle += angle;

        for (var i = 0; i < _classRects.Count; i++) {
            var character = _classRects[i];

            var angleInRadians = _rotationAngle + (i * angleStep);

            var x = (CenterX + 50f) + 300f * MathF.Cos(angleInRadians);
            character.X = (int)x - (character.Width / 2);

            var yOffset = MathF.Sin(angleInRadians) * -YOffsetMultiplier;
            character.Y = (int)(baseY + yOffset - (character.Height / 2));

            var size = 160;
            character.Width = size;
            character.Height = size;

            var distanceFromCenter = Math.Abs(character.Y - baseY + 50f);
            var maxDistance = 40f;
            var alpha = Math.Clamp(1f - (distanceFromCenter / maxDistance), 0f, 1f);

            character.Alpha = alpha;

            if (i == CurrentCharacterIndex) {
                character.Alpha = 1f;
            }

            SelectedClass = _classRects[CurrentCharacterIndex];
        }
    }

    private void RotateToNextCharacter() {
        CurrentCharacterIndex = (CurrentCharacterIndex + 1) % ClassesLength;
        _startRotationAngle = _rotationAngle;
        _targetRotationAngle = _snapAngles[CurrentCharacterIndex];
        _animationTimer = 0f;
        _isAnimating = true;
    }

    private void RotateToPreviousCharacter() {
        CurrentCharacterIndex = (CurrentCharacterIndex - 1 + ClassesLength) % ClassesLength;
        _startRotationAngle = _rotationAngle;
        _targetRotationAngle = _snapAngles[CurrentCharacterIndex];
        _animationTimer = 0f;
        _isAnimating = true;
    }

    private void OnFrameEnter() {
        var gameTime = Stage.GameTime;
        UpdateCharacterWheel(0);

        var sortedCharacters = _classRects.OrderBy(c => c.Y).ToList();

        foreach (var character in sortedCharacters) {
            AddChild(character);
        }

        if (!_isAnimating)
            return;

        _animationTimer += (float)gameTime.TotalMs;

        float t = Math.Clamp(_animationTimer / _animationDuration, 0f, 1f);
        _rotationAngle = MathHelper.Lerp(_startRotationAngle, _targetRotationAngle, t);

        UpdateCharacterWheel(0f);

        if (t >= 1f) {
            _isAnimating = false;
        }
    }
}