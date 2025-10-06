using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using OpenTK.Mathematics;
using RealmClient.State;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.BuiltIn.Buttons;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;

namespace RealmClient.Screens.Components.CharacterSelection;

public class CharacterWheel : Container {
    private const int CenterX = Settings.DefaultScreenWidth / 2; 
    private const int CenterY = Settings.DefaultScreenHeight / 2 + 200;
    
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
    public readonly ushort[] Classes = [0x0300, 0x0307, 0x030e, 0x0310, 0x031d, 0x031e, 0x031f, 0x0320, 0x0321, 0x0322, 0x0323, 0x0324, 0x0325, 0x0326];
    public int CurrentCharacterIndex;
    public ClassRect SelectedClass;

    public CharacterWheel() {
        for (var i = 0; i < Classes.Length; i++) {
            var classRect = new ClassRect(i, Classes[i]); 
            _classRects.Add(classRect);
            AddChild(classRect);
        }

        UpdateCharacterWheel(0);
        
        var spincfg = new TextButtonConfig { Text = "Forward", FontSize = 50, OnClicked = RotateToNextCharacter, FontType = FontType.Normal, X = 75, Y = 485 };
        var spinButton = new TextButton(spincfg);
        AddChild(spinButton);
        
        var backcfg = new TextButtonConfig { Text = "Back", FontSize = 50, OnClicked = RotateToPreviousCharacter, FontType = FontType.Normal, X = 75, Y = 560 };
        var spinBackButton = new TextButton(backcfg);
        AddChild(spinBackButton);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void UpdateCharacterWheel(float angle) {
        var baseY = CenterY + 50f; 
        var angleStep = MathF.Tau / Classes.Length;
        for (var i = 0; i < Classes.Length; i++) {
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
        CurrentCharacterIndex = (CurrentCharacterIndex + 1) % Classes.Length;
        _startRotationAngle = _rotationAngle;
        _targetRotationAngle = _snapAngles[CurrentCharacterIndex];
        _animationTimer = 0f;
        _isAnimating = true;
    }

    private void RotateToPreviousCharacter() {
        CurrentCharacterIndex = (CurrentCharacterIndex - 1 + Classes.Length) % Classes.Length;
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