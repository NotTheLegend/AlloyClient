using System.Collections.Generic;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.AppEngine;
using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Game;
using AlloyClient.Screens.Components;
using AlloyClient.Screens.Components.CharacterList;
using AlloyClient.Screens.Components.Containers;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Dialogs;
using AlloyClient.Ui.Components.Graphics;
using AlloyClient.Ui.Components.Scrollbars;
using AlloyClient.Utils;

namespace AlloyClient.Screens;

public class CharacterListScreen : TitleScreenBase {
    private const int PlayFontSize = 48;
    private const int FontSize = 30;
    private const int TabY = 70;
    private const int ContentBottomY = TitleMenuRibbon.TopY;

    private readonly Container _content = new(new ContainerConfig {
        Width = Settings.DefaultScreenWidth,
        Height = Settings.DefaultScreenHeight,
    });

    private readonly Container _scrollContainer;

    private readonly ColorRect _lineDivider;
    private readonly TextButton _nameText;
    private readonly ObjectRect _goldIcon;
    private readonly SimpleText _goldText;
    private readonly ObjectRect _fameIcon;
    private readonly SimpleText _fameText;

    private readonly Container _characterListContainer;
    private readonly Container _graveyardContainer;

    private readonly TextButton _charactersButton;
    private readonly TextButton _graveyardButton;

    private readonly List<CharacterRect> _characterListRects = [];
    private CharacterRect _newCharacterRect;

    private List<CharacterRect> _graveyardCharacterRects = [];

    private VerticalScrollBar _scrollBar;
    private ClassContainer _classContainer;

    private int _contentWidth = Settings.DefaultScreenWidth;

    private int _selectedCharacterId = -1;

    private Container _mainBar;
    private Container _backBar;

    public CharacterListScreen() {
        AddChild(_content);

        #region Title Buttons

        var playButton = new MenuBarButton("play", PlayFontSize, () => {
            var data = GlobalData.Get<CharacterListData>();
            if (data == null) {
                return;
            }

            var charList = data.Characters;
            if (charList == null || charList.Length <= 0) {
                ShowCharacterCreate();
                return;
            }

            GlobalData.SelectedCharacterId = _selectedCharacterId;
            ScreenManager.FadeToScreen(new GameScreen(), Easing.SineInOut, 1000, 0x0);
        }, true);

        playButton.SetAnchor(UiAnchor.Middle);
        MenuBar.AddChild(playButton);

        var classesButton = new MenuBarButton("classes", FontSize, ShowCharacterCreate);
        classesButton.SetAnchor(UiAnchor.MiddleLeft);
        classesButton.X = playButton.Width / 2 + MenuGap;
        MenuBar.AddChild(classesButton);

        var backButton = new MenuBarButton("back", FontSize,
            () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });

        backButton.SetAnchor(UiAnchor.MiddleRight);
        backButton.X = -playButton.Width / 2 - MenuGap;
        MenuBar.AddChild(backButton);

        #endregion

        #region Decoration

        _lineDivider = new ColorRect(new ColorRectConfig {
            Y = 100,
            Width = Settings.DefaultScreenWidth,
            Height = 2,
            Color = 0x777777,
            Alpha = 0.75f
        });

        _content.AddChild(_lineDivider);

        #endregion

        #region Name and Currency

        var account = GlobalData.Get<AccountData>();

        //TODO: swap to simple text
        _nameText = new TextButton(new TextButtonConfig {
            Text = account?.Name ?? string.Empty,
            FontSize = 32,
            FontType = FontType.Bold,
            X = Settings.DefaultScreenWidth / 2,
            Y = 50,
            ActiveColor = 0xB3B3B3,
            InactiveColor = 0xB3B3B3,
            Anchor = UiAnchor.Middle,
        });

        _content.AddChild(_nameText);

        _goldIcon = new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.FromGameAtlas("lofiObj3", 0xE1),
            X = Settings.DefaultScreenWidth - 15,
            Y = 82,
            Width = 16,
            Height = 16,
            Anchor = UiAnchor.MiddleRight,
        });

        _content.AddChild(_goldIcon);

        _goldText = new SimpleText(new TextConfig {
            Text = (account?.Stats.Credits ?? 0).ToString(),
            FontSize = 24,
            FontType = FontType.Normal,
            X = _goldIcon.X - _goldIcon.Width - 5,
            Y = _goldIcon.Y,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.MiddleRight,
        });

        _content.AddChild(_goldText);

        _fameIcon = new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.FromGameAtlas("lofiObj3", 0xE0),
            X = _goldText.X - _goldText.Width - 10,
            Y = _goldIcon.Y,
            Width = 16,
            Height = 16,
            Anchor = UiAnchor.MiddleRight,
        });

        _content.AddChild(_fameIcon);

        _fameText = new SimpleText(new TextConfig {
            Text = (account?.Stats.Fame ?? 0).ToString(),
            FontSize = 24,
            FontType = FontType.Normal,
            X = _fameIcon.X - _fameIcon.Width - 5,
            Y = _goldIcon.Y,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.MiddleRight,
        });

        _content.AddChild(_fameText);

        #endregion

        #region Containers

        var containerY = _lineDivider.Y + _lineDivider.Height;
        var containerHeight = ContentBottomY - containerY;
        _scrollContainer = new Container(new ContainerConfig {
            Y = _lineDivider.Y + _lineDivider.Height,
            Width = Settings.DefaultScreenWidth,
            Height = containerHeight,
            EnableClip = true
        });

        _content.AddChild(_scrollContainer);

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
            X = 15,
            Y = TabY,
            Anchor = UiAnchor.LeftTop
        });

        _content.AddChild(_charactersButton);

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
            X = _charactersButton.X + _charactersButton.Width + 25,
            Y = TabY,
            Anchor = UiAnchor.LeftTop
        });

        _graveyardButton.Alpha = 0.6f;
        _content.AddChild(_graveyardButton);

        #endregion

        MouseEnabled = true;

        AddEventListener(AppRequests.GetCharList(), () => {
            LoadCharacterList();
            LoadGraveyardList();
        });

        CheckForAppFailure();
    }

    protected override void OnResize(ResizeEvent args) {
        var scale = Stage.ScreenScale;
        _content.Scale = scale;

        _contentWidth = (int)System.Math.Ceiling(args.Width / scale.X);
        _lineDivider.Resize(_contentWidth, _lineDivider.Height);
        _scrollContainer.Resize(_contentWidth, _scrollContainer.Height);

        if (_classContainer != null) {
            _classContainer.ResizeLayout(_contentWidth);
        }

        _nameText.X = _contentWidth / 2;
        PositionCurrencyDisplay();

        if (_scrollBar is not null) {
            _scrollBar.X = _contentWidth - 10;
        }

        base.OnResize(args);
    }

    private void PositionCurrencyDisplay() {
        _goldIcon.X = _contentWidth - 15;
        _goldText.X = _goldIcon.X - _goldIcon.Width - 5;
        _fameIcon.X = _goldText.X - _goldText.Width - 10;
        _fameText.X = _fameIcon.X - _fameIcon.Width - 5;
    }

    private void LoadCharacterList() {
        var charModel = GlobalData.Get<CharacterListData>();
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
        var visibleContentHeight = _scrollContainer.Height;
        if (totalContentHeight <= visibleContentHeight) {
            return;
        }

        _scrollBar = new VerticalScrollBar(_scrollContainer, new VerticalScrollBarConfig {
            X = _contentWidth - 10,
            Width = 10,
            Height = _scrollContainer.Height,
            TotalContentHeight = totalContentHeight,
            VisibleContentHeight = visibleContentHeight,
            OnValueChanged = value => { _characterListContainer.Y = -value; }
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
        if (_classContainer != null) {
            return;
        }

        _scrollContainer.Visible = false;
        _charactersButton.Visible = false;
        _graveyardButton.Visible = false;
        MenuBar.Visible = false;

        _classContainer = new ClassContainer(HideCharacterCreate);
        _classContainer.ResizeLayout(_contentWidth);
        _content.AddChild(_classContainer);
    }

    private void HideCharacterCreate() {
        if (_classContainer == null) {
            return;
        }

        _content.RemoveChild(_classContainer);
        _classContainer = null;

        _scrollContainer.Visible = true;
        _charactersButton.Visible = true;
        _graveyardButton.Visible = true;
        MenuBar.Visible = true;
    }

    private void CheckForAppFailure() {
        if (!GlobalData.TryRemove<AppRequestFailedFlag>(out var data)) {
            return;
        }

        AddChild(new ScreenDarkenOverlay());

        DialogManager.Enqueue(new RetryLoadDialog(data.Message));
    }
}