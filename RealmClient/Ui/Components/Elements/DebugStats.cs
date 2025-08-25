using Common;
using RealmClient.Game;
using RealmClient.Rendering;
using RealmClient.UiLib;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.Utils;

namespace RealmClient.Ui.Components.Elements;

public class DebugStats : Sprite {
    private const int Outline = 3;

    private readonly SimpleText _fps = new (new TextConfig {Text = "FPS: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _tiles = new (new TextConfig {Text = "Tiles: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _shadows = new (new TextConfig {Text = "Shadows: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _entities = new (new TextConfig {Text = "Entities: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _particles = new (new TextConfig {Text = "Particles: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop });
    private readonly SimpleText _ui = new(new TextConfig {Text = "Ui: 0", X = 2, FontSize = 16, FontType = FontType.Bold, OutlineThickness = Outline, Anchor = UiAnchor.LeftTop});

    private double _lastTime;
    
    public DebugStats() {
        AddChild(_fps);
        AddChild(_tiles);
        AddChild(_shadows);
        AddChild(_entities);
        AddChild(_particles);
        AddChild(_ui);
        
        _fps.Y = 4;
        _tiles.Y = _fps.Y + _fps.Height + 4;
        _shadows.Y = _tiles.Y + _tiles.Height + 4;
        _entities.Y = _shadows.Y + _shadows.Height + 4;
        _particles.Y = _entities.Y + _entities.Height + 4;
        _ui.Y = _particles.Y + _particles.Height + 4;
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (gameTime.TotalMs - _lastTime < 1000) return;

        _lastTime = gameTime.TotalMs;
        
        Logger.Info($"\n[Frame Stats]\n" +
                    $"FPS: {GameScreen.Frames}\n" +
                    $"Tiles: {Render.LastDrawCountTiles}\n" +
                    $"Shadows: {Render.LastDrawCountShadows}\n" +
                    $"DrawnEntities: {Render.LastDrawCountEntities}\n" +
                    $"DrawnUiElements: {UiRender.LastRenderCount}\n" +
                    $"DrawnParticles: {Render.LastDrawParticleCount}");
        
        _fps.SetText($"FPS: {GameScreen.Frames}");
        _tiles.SetText($"Tiles: {Render.LastDrawCountTiles}");
        _shadows.SetText($"Shadows: {Render.LastDrawCountShadows}");
        _entities.SetText($"Entities: {Render.LastDrawCountEntities}");
        _particles.SetText($"Particles: {Render.LastDrawParticleCount}");
        _ui.SetText($"Ui: {UiRender.LastRenderCount}");

        GameScreen.Frames = 0;
    }
}