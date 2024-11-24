using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoClient.Assets;
using MonoClient.Display;
using MonoClient.Objects;
using MonoClient.Rendering;
using MonoClient.Screens.MapEditor.Components;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Utils;

namespace MonoClient.Screens.MapEditor;

public partial class MapEditorScreen : Screen {
    public const ushort MapEditorTileType = 0x7F02;
    public class MapStructure {
        public string Name;
        public int Width;
        public int Height;
        public MapTile[,] Tiles;
        public Entity[,] Objects;
        
        public readonly RenderStorage EntityStorage = new();
        public VertexPositionColor[] MapBorderVertices;
    }

    private static readonly Logger Log = new(typeof(MapEditorScreen));
    private static readonly GraphicsDevice GraphicsDevice = Main.GameInstance.GraphicsDevice;
    
    #region Map Fields
    
    private static readonly VertexPositionColor[] TileOutlineVertices;
    
    public static MapEditorScreen Instance;
    
    private readonly BasicEffect _basicEffect;
    
    private bool _draggingCamera;
    
    public MapTile TileHovered;

    private Vector2 _cameraPosition;

    private StateContainer _prevInputState;
    private StateContainer _currInputState;
    
    private Vector2 _initialMousePosition;

    private int _zoomLevel = 24;

    private readonly List<MapStructure> _maps = [];
    private int _currentMapIndex = 0;
    
    #endregion
    
    #region Editor UI Fields
    
    private EditorTabBar _editorTabBar;
    private EditorButtonBar _editorButtonBar;
    private EditorToolBar _editorToolBar;
    private EditorObjectsPanel _editorObjectsPanel;
    
    private EditorObjectDetailsPanel _editorObjectDetailsPanel;
    
    #endregion

    private double _lastLogTime;
    private int _frames;

    static MapEditorScreen() {
        TileOutlineVertices = new VertexPositionColor[5];
        for (var i = 0; i < 5; i++) {
            TileOutlineVertices[i] = new VertexPositionColor(Vector3.Zero, Color.White);
        }
    }

    public MapEditorScreen() {
        Instance = this;
        
        SetBaseDimensions(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight);

        #region Setup Map
        
        LoadMap(@"C:\Users\kcmur\Desktop\Projects\Realm\Magica-Domain\7.0 Source\realm-src-master\common\resources\worlds\Vault.jm");
        //CreateNewMap($"New Map {_maps.Count + 1}", MapEditorUtils.MapSize.Gigantic);

        Settings.CameraAngle = 0;
        Camera.Reset(false);
        Camera.SetZoom(8);

        _basicEffect = new BasicEffect(GraphicsDevice) {
            VertexColorEnabled = true
        };
        
        #endregion
        
        #region Setup Editor UI
        
        _editorTabBar = new EditorTabBar();
        AddChild(_editorTabBar);
        
        _editorButtonBar = new EditorButtonBar();
        AddChild(_editorButtonBar);
        
        _editorToolBar = new EditorToolBar();
        AddChild(_editorToolBar);
        
        _editorObjectsPanel = new EditorObjectsPanel(this);
        AddChild(_editorObjectsPanel);

        _editorObjectDetailsPanel = new EditorObjectDetailsPanel();
        AddChild(_editorObjectDetailsPanel);

        #endregion
        
        MouseEnabled = true;
    }

    private void RefreshUi() {
        RemoveChild(_editorTabBar);
        RemoveChild(_editorButtonBar);
        RemoveChild(_editorToolBar);
        RemoveChild(_editorObjectsPanel);
        RemoveChild(_editorObjectDetailsPanel);
        
        _editorTabBar = new EditorTabBar();
        AddChild(_editorTabBar);
        
        _editorButtonBar = new EditorButtonBar();
        AddChild(_editorButtonBar);
        
        _editorToolBar = new EditorToolBar();
        AddChild(_editorToolBar);
        
        _editorObjectsPanel = new EditorObjectsPanel(this);
        AddChild(_editorObjectsPanel);

        _editorObjectDetailsPanel = new EditorObjectDetailsPanel();
        AddChild(_editorObjectDetailsPanel);
    }
    
    public override void Update(GameTime gameTime) {
        var time = gameTime.TotalGameTime.TotalMilliseconds;
        var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
        if (Main.GameInstance.IsActive) {
            InputHandler.Update(time, dt);
        }

        var state = new StateContainer {
            KeyboardState = Keyboard.GetState(),
            MouseState = Mouse.GetState()
        };

        _prevInputState = _currInputState;
        _currInputState = state;

        var currentMap = _maps[_currentMapIndex];
        var mousePosition = new Vector2(state.MouseState.X, state.MouseState.Y);
        if (state.MouseState.X >= 0 && state.MouseState.X < GraphicsDevice.Viewport.Width && state.MouseState.Y >= 0 && state.MouseState.Y < GraphicsDevice.Viewport.Height) {
            if (state.IsPressed(Keys.LeftControl)) {
                if (state.MouseState.LeftButton == ButtonState.Pressed && _prevInputState.MouseState.LeftButton == ButtonState.Pressed) {
                    var diff = _initialMousePosition - mousePosition;
                    _cameraPosition += diff / _zoomLevel;
                    _initialMousePosition = mousePosition;
                    _draggingCamera = true;
                }

                _initialMousePosition = mousePosition;
            }
            else {
                _initialMousePosition = mousePosition;
                _draggingCamera = false;
            }
            
            var tilePos = Camera.ScreenToWorld(mousePosition, GraphicsDevice.Viewport);
            var x = (int) Math.Floor(tilePos.X);
            var y = (int) Math.Floor(tilePos.Y);
            if (x < 0 || x >= currentMap.Width || y < 0 || y >= currentMap.Height) {
                TileHovered = null;
            }
            else {
                TileHovered = currentMap.Tiles[x, y];
            }
        }
        
        if (state.KeyboardState.IsKeyDown(Keys.LeftAlt) && state.KeyboardState.IsKeyDown(Keys.LeftControl)) {
            Log.Info("Reloading editor UI");
            RefreshUi();
        }

        if (state.KeyboardState.IsKeyDown(Keys.LeftShift)) {
            var scrollDelta = state.MouseState.ScrollWheelValue - _prevInputState.MouseState.ScrollWheelValue;
            switch (scrollDelta) {
                case < 0:
                    _zoomLevel -= _zoomLevel switch {
                        > 64 => 8,
                        > 32 => 4,
                        > 16 => 2,
                        _ => 1
                    };
                    _zoomLevel = Math.Max(1, _zoomLevel);
                    Camera.SetZoom(_zoomLevel);

                    Log.Info($"Camera zoom: {_zoomLevel}");
                    break;
                case > 0:
                    _zoomLevel += _zoomLevel switch {
                        < 16 => 1,
                        < 32 => 2,
                        < 64 => 4,
                        _ => 8
                    };
                    Camera.SetZoom(_zoomLevel);

                    Log.Info($"Camera zoom: {_zoomLevel}");
                    break;
            }
        }

        Camera.Update(_cameraPosition.X, _cameraPosition.Y);

        if (TileHovered != null) {
            _editorObjectDetailsPanel.UpdateDetails(TileHovered);
        }
    }

    public override void Draw(GameTime gameTime) {
        GraphicsDevice.DepthStencilState = DepthStencilState.None;

        var time = gameTime.TotalGameTime.TotalMilliseconds;
        if (time - _lastLogTime > 1000) {
            _lastLogTime = time;
            Log.Info($"FPS: {_frames}");
            _frames = 0;
        }

        _frames++;

        Render.SetShaderParams(gameTime);

        if (_currentMapIndex < _maps.Count) {
            var map = _maps[_currentMapIndex];

            #region Render Tile
            
            Render.StartDrawEditorTile();
            
            foreach (var tile in map.Tiles) {
                tile?.DrawEditorTile();
            }
            
            Render.FlushBufferEditorTile();
            
            #endregion
            
            #region Render Ground Objects
            
            if (map.EntityStorage.TryGetValue(ModelType.PbTile, out var ground)) {
                Render.StartDrawEntity();
                Render.SetEntityModel(ModelType.PbTile);

                foreach (var type in ground) {
                    type.Visible = true;
                    type.SetDepth(0.9f);
                    type.Draw();
                }
                
                Render.FlushBufferEntity();
            }
            
            #endregion
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            #region Render Objects
            
            Render.StartDrawEntity();
            
            foreach (var (modelType, entities) in map.EntityStorage) {
                switch (modelType) {
                    case ModelType.Null or ModelType.PbTile:
                        continue;
                    case ModelType.PbObject:
                        entities.Sort(); // Sort for alpha
                        break;
                }

                Render.SetEntityModel(modelType);
                
                foreach (var entity in entities) {
                    entity.Draw();
                }
                
                Render.FlushBufferEntity();
            }
            
            #endregion

            #region Misc Render
            
            _basicEffect.World = Camera.WorldMatrix;
            _basicEffect.View = Camera.ViewMatrix;
            _basicEffect.Projection = Camera.ProjectionMatrix;

            foreach (var pass in _basicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                
                #region Draw Map Border
                
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, map.MapBorderVertices, 0, 4);
                
                #endregion
                
                if (TileHovered != null && !_draggingCamera) {
                    #region Draw Selected Tile Outline
                    
                    TileOutlineVertices[0].Position.X = TileHovered.X;
                    TileOutlineVertices[0].Position.Y = TileHovered.Y;
                    TileOutlineVertices[1].Position.X = TileHovered.X + 1;
                    TileOutlineVertices[1].Position.Y = TileHovered.Y;
                    TileOutlineVertices[2].Position.X = TileHovered.X + 1;
                    TileOutlineVertices[2].Position.Y = TileHovered.Y + 1;
                    TileOutlineVertices[3].Position.X = TileHovered.X;
                    TileOutlineVertices[3].Position.Y = TileHovered.Y + 1;
                    TileOutlineVertices[4].Position.X = TileHovered.X;
                    TileOutlineVertices[4].Position.Y = TileHovered.Y;
                        
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, TileOutlineVertices, 0, 4);
                    
                    #endregion
                }
            }
            
            #endregion
        }
    }
    
    public MapTile GetTile(int x, int y) {
        if (_maps.Count == 0) {
            return null;
        }
        
        if (x < 0 || y < 0) {
            return null;
        }

        if (x > _maps[_currentMapIndex].Width || y > _maps[_currentMapIndex].Height) {
            return null;
        }

        return _maps[_currentMapIndex].Tiles[x, y];
    }
}