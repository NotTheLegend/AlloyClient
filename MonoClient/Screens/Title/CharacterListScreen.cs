using System.Collections.Generic;
using MonoClient.Data;
using MonoClient.Display;
using MonoClient.Screens.Game;
using MonoClient.Screens.Title.Components;
using MonoClient.Screens.Title.Components.CharacterList;
using MonoClient.Screens.Title.Components.Panels;
using MonoClient.State;
using MonoClient.Ui.Components.Buttons;
using MonoClient.Ui.Components.Scrollbars;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using ColorRect = MonoClient.UiLib.BuiltIn.ColorRect;
using Container = MonoClient.UiLib.BuiltIn.Container;

namespace MonoClient.Screens.Title;

public class CharacterListScreen : TitleScreenBase {
    private const int PlayFontSize = 55;
    private const int FontSize = 35;
    
    private readonly Container _scrollContainer;

    private readonly Container _characterListContainer;
    private readonly Container _graveyardContainer;
    
    private readonly TextButton _charactersButton;
    private readonly TextButton _graveyardButton;
    
    private readonly List<CharacterRect> _characterListRects = [];
    private CharacterRect _newCharacterRect;
    
    private List<CharacterRect> _graveyardCharacterRects = [];
    
    private VerticalScrollBar _scrollBar;

    private int _selectedCharacterId = -1;

    public CharacterListScreen() {
        #region Title Buttons

        var playButton = new MenuBarButton("play",  PlayFontSize, () => {
            var charList = CharacterList.Model.Characters;
            if (charList == null || charList.Length <= 0) 
            {
                ShowCharacterCreate();
                return;
            }
            Account.SelectedCharacterId = _selectedCharacterId;
            ScreenManager.FadeToScreen(new GameScreen(), Easing.SineInOut, 1000, 0x0);
        }, true);
        playButton.SetAnchor(UiAnchor.Middle);
        playButton.X = Settings.DefaultScreenWidth / 2;
        playButton.Y = Settings.DefaultScreenHeight - 50;
        AddChild(playButton);

        var classesButton = new MenuBarButton("classes", FontSize, ShowCharacterCreate);
        classesButton.SetAnchor(UiAnchor.MiddleLeft);
        classesButton.X = playButton.X + playButton.Width / 2 + 50;
        classesButton.Y = Settings.DefaultScreenHeight - 50;
        AddChild(classesButton);
        
        var backButton = new MenuBarButton("back", FontSize, () => {
            ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0);
        });
        backButton.SetAnchor(UiAnchor.MiddleRight);
        backButton.X = playButton.X - playButton.Width / 2 - 50;
        backButton.Y = Settings.DefaultScreenHeight - 50;
        AddChild(backButton);
        
        #endregion
        
        #region Class Creation Popup
        
        #endregion

        #region Decoration

        var lineDivider = new ColorRect(new ColorRectConfig {
            Y = 100,
            Width = Settings.DefaultScreenWidth,
            Height = 5,
            Color = 0x2B2B2B,
        });
        AddChild(lineDivider);
        
        #endregion
        
        #region Name and Currency
        
        var nameText = new TextButton(new TextButtonConfig() {
            Text = Account.Username,
            FontSize = 32,
            FontType = FontType.Bold,
            X = Settings.DefaultScreenWidth / 2,
            Y = 50,
            OnClicked = () => OverlayManager.Enqueue(new NameContainer()),
            ActiveColor = 0xB3B3B3,
            InactiveColor = 0xB3B3B3,
            Anchor = UiAnchor.Middle,
        });
        nameText.SetState(!CharacterList.Model.Account.NameChosen);
        AddChild(nameText);
        
        var goldIcon = new ObjectRect(new ObjectRectConfig {
            Texture = TextureInfo.FromGameAtlas("lofiObj3", 0xE1),
            X = Settings.DefaultScreenWidth - 15,
            Y = 88,
            Width = 16,
            Height = 16,
            Anchor = UiAnchor.RightBottom,
        });
        AddChild(goldIcon);
        
        var goldText = new SimpleText(new TextConfig {
            Text = CharacterList.Model.Account.Credits.ToString(),
            FontSize = 24,
            FontType = FontType.Normal,
            X = goldIcon.X - goldIcon.Width - 5,
            Y = 93,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.RightBottom,
        });
        AddChild(goldText);
        
        var fameIcon = new ObjectRect(new ObjectRectConfig {
            Texture = TextureInfo.FromGameAtlas("lofiObj3", 0xE0),
            X = goldText.X - goldText.Width - 10,
            Y = 88,
            Width = 16,
            Height = 16,
            Anchor = UiAnchor.RightBottom,
        });
        AddChild(fameIcon);

        var fameText = new SimpleText(new TextConfig {
            Text = CharacterList.Model.Account.Stats.Fame.ToString(),
            FontSize = 24,
            FontType = FontType.Normal,
            X = fameIcon.X - fameIcon.Width - 5,
            Y = 93,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.RightBottom,
        });
        AddChild(fameText);
        
        #endregion
        
        #region Containers

        var containerY = lineDivider.Y + lineDivider.Height;
        var containerHeight = Settings.DefaultScreenHeight - containerY - 80;
        _scrollContainer = new Container(new ContainerConfig {
            Y = lineDivider.Y + lineDivider.Height,
            Width = Settings.DefaultScreenWidth,
            Height = containerHeight,
            EnableClip = true
        });
        AddChild(_scrollContainer);
        
        _characterListContainer = new Container();
        _scrollContainer.AddChild(_characterListContainer);
        
        _graveyardContainer = new Container();
        _scrollContainer.AddChild(_graveyardContainer);
        
        _graveyardContainer.Visible = false;

        #endregion
        
        #region Tab Buttons

        _charactersButton = new TextButton(new TextButtonConfig {
            Text = "Characters",
            FontSize = 24,
            OnClicked = () => {
                _characterListContainer.Visible = true;
                _graveyardContainer.Visible = false;

                if (_graveyardButton == null || _charactersButton == null) {
                    return;
                }

                _graveyardButton.Alpha = 0.6f;
                _charactersButton.Alpha = 1f;
            },
            X = 15
        });
        _charactersButton.Y = lineDivider.Y - _charactersButton.Height / 2 - 15;
        AddChild(_charactersButton);
        
        _graveyardButton = new TextButton(new TextButtonConfig {
            Text = "Graveyard",
            FontSize = 24,
            OnClicked = () => {
                _graveyardContainer.Visible = true;
                _characterListContainer.Visible = false;

                if (_graveyardButton == null || _charactersButton == null) {
                    return;
                }

                _charactersButton.Alpha = 0.6f;
                _graveyardButton.Alpha = 1f;
            },
            X = _charactersButton.X + _charactersButton.Width + 25
        });
        _graveyardButton.Alpha = 0.6f;
        _graveyardButton.Y = lineDivider.Y - _graveyardButton.Height / 2 - 15;
        AddChild(_graveyardButton);
        
        #endregion
        
        MouseEnabled = true;
        
        AddEventListener(CharacterList.LoadAsync(), () => {
            LoadCharacterList();
            LoadGraveyardList();
        });
    }

    private void LoadCharacterList() {
        var charModel = CharacterList.Model;
        if (charModel == null) {
            return;
        }
        
        var characters = charModel.Characters;
        const int baseX = 5;
        const int baseY = 12;
        if (characters != null) {
            foreach (var character in characters) {
                if (_selectedCharacterId == -1) {
                    _selectedCharacterId = character.Id;
                }
                
                var charRect = new CharacterRect(this) {
                    X = baseX,
                    Y = baseY
                };
                charRect.EnableClipRect = true;
                charRect.Initialize(CharacterRectType.Character, character);
                _characterListContainer.AddChild(charRect);
                
                _characterListRects.Add(charRect);
            }
        }
        
        _characterListRects.Sort((a, b) => {
            var aSortValue = a.ComputeSortValue();
            var bSortValue = b.ComputeSortValue();
            return bSortValue.CompareTo(aSortValue);
        });

        for (var i = 1; i < _characterListRects.Count; i++) {
            var characterRect = _characterListRects[i];
            var row = i / 6;
            var col = i % 6;
            characterRect.X = baseX + col * 210;
            characterRect.Y = baseY + row * 210;
        }

        var remainingSlots = charModel.MaxNumChars - _characterListRects.Count;
        _newCharacterRect = new CharacterRect(this) {
            X = baseX + _characterListRects.Count % 6 * 210,
            Y = baseY + _characterListRects.Count / 6 * 210
        };
        _newCharacterRect.Initialize(CharacterRectType.NewCharacter, remainingSlots: remainingSlots);
        _characterListContainer.AddChild(_newCharacterRect);
        
        var totalContentHeight = _newCharacterRect.Y + 210;
        var visibleContentHeight = Settings.DefaultScreenHeight - 180;
        _scrollBar = new VerticalScrollBar(_scrollContainer, new VerticalScrollBarConfig {
            X = Settings.DefaultScreenWidth - 10,
            Width = 10,
            Height = _scrollContainer.Height,
            TotalContentHeight = totalContentHeight,
            VisibleContentHeight = visibleContentHeight,
            OnValueChanged = value => {
                _characterListContainer.Y = -value;
            }
        });
        _scrollContainer.AddChild(_scrollBar);
    }

    private void LoadGraveyardList() {
        // TODO: Implement graveyard
        
        // _scrollBar = new VerticalScrollBar(Settings.DefaultScreenWidth - 10, 0, 10, Settings.DefaultScreenHeight - 180, 0, 100, 0, 0, value => {
        //     _graveyardContainer.Y = value;
        // });
    }

    public void ShowCharacterCreate() {
        OverlayManager.Enqueue(new ClassContainer());
    }
    
    public void HideCharacterCreate() {
    }
}