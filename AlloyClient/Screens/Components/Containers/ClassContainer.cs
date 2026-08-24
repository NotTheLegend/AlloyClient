using System;
using System.Collections.Generic;
using System.Linq;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.Assets.Libraries;
using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Game;
using AlloyClient.Screens.Components.CharacterSelection;
using AlloyClient.Ui;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Graphics;
using AlloyClient.Ui.Components.Scrollbars;
using AlloyClient.Utils;

namespace AlloyClient.Screens.Components.Containers;

public sealed class ClassContainer : Container {
    private const int ScreenWidth = Settings.DefaultScreenWidth;
    private const int ScreenHeight = Settings.DefaultScreenHeight;
    private const int TopSeparatorY = 100;

    private const int ClassColumns = 5;
    private const int ClassGapX = 20;
    private const int ClassGapY = 14;

    private const int SkinListHeight = TitleMenuRibbon.TopY - TopSeparatorY;

    private readonly Container _classScreen;
    private readonly Container _detailScreen;
    private readonly List<SkinChoiceRow> _skinRows = [];
    private readonly Action _onBack;

    private ushort _selectedClassType;
    private ushort _selectedSkinType;
    private int _screenWidth = ScreenWidth;
    private ObjectRect _detailPortrait;

    private int DetailDividerX => (int)Math.Round(_screenWidth * (520d / ScreenWidth)) + 10;
    private int SkinListWidth => _screenWidth - DetailDividerX;

    public ClassContainer(Action onBack)
        : base(new ContainerConfig { Width = ScreenWidth, Height = ScreenHeight }) {
        _onBack = onBack;

        _classScreen = new Container(new ContainerConfig { Width = ScreenWidth, Height = ScreenHeight });
        _detailScreen = new Container(new ContainerConfig { Width = ScreenWidth, Height = ScreenHeight });
        _detailScreen.Visible = false;

        AddChild(_classScreen);
        AddChild(_detailScreen);
        AddEventListener(Event.EnterFrame, AnimateDetailPortrait);

        BuildClassScreen();
    }

    public void ResizeLayout(int width) {
        width = Math.Max(1, width);
        if (_screenWidth == width) {
            return;
        }

        _screenWidth = width;
        Resize(_screenWidth, ScreenHeight);
        _classScreen.Resize(_screenWidth, ScreenHeight);
        _detailScreen.Resize(_screenWidth, ScreenHeight);

        _classScreen.RemoveChildren();
        BuildClassScreen();

        if (_detailScreen.Visible) {
            RebuildDetailScreen();
        }
    }

    private void BuildClassScreen() {
        var classes = ObjectLibrary.TypeToClassProps.Keys.ToArray();
        var gridWidth = ClassColumns * ClassChoiceCard.CardWidth + (ClassColumns - 1) * ClassGapX;
        var rows = (int)Math.Ceiling(classes.Length / (double)ClassColumns);
        var gridHeight = rows * ClassChoiceCard.CardHeight + Math.Max(0, rows - 1) * ClassGapY;
        var startX = (_screenWidth - gridWidth) / 2;
        var startY = TopSeparatorY + (TitleMenuRibbon.TopY - TopSeparatorY - gridHeight) / 2;

        for (var i = 0; i < classes.Length; i++) {
            var type = classes[i];
            var stats = GetClassStats(type);
            var card = new ClassChoiceCard(type, stats?.BestFame ?? 0, ShowClassDetails) {
                X = startX + i % ClassColumns * (ClassChoiceCard.CardWidth + ClassGapX),
                Y = startY + i / ClassColumns * (ClassChoiceCard.CardHeight + ClassGapY)
            };

            _classScreen.AddChild(card);
        }

        _classScreen.AddChild(new MenuBarButton(new TextButtonConfig {
            Text = "back",
            FontSize = 32,
            FontType = FontType.Bold,
            OutlineThickness = 4,
            OnClicked = _onBack,
            X = _screenWidth / 2,
            Y = TitleMenuRibbon.MenuCenterY,
            Anchor = UiAnchor.Middle
        }));
    }

    private void ShowClassDetails(ClassChoiceCard card) {
        _selectedClassType = card.Type;
        _selectedSkinType = 0;

        _classScreen.Visible = false;
        _detailScreen.Visible = true;
        RebuildDetailScreen();
    }

    private void RebuildDetailScreen() {
        _detailScreen.RemoveChildren();
        _skinRows.Clear();

        _detailScreen.AddChild(new ColorRect(new ColorRectConfig {
            X = DetailDividerX,
            Y = TopSeparatorY,
            Width = 2,
            Height = SkinListHeight,
            Color = 0x777777,
            Alpha = 0.75f
        }));

        BuildClassDetails();
        BuildSkinList();
        BuildDetailNavigation();
    }

    private void BuildClassDetails() {
        var props = ObjectLibrary.TypeToObjectProps[_selectedClassType];
        var stats = GetClassStats(_selectedClassType);
        var bestFame = stats?.BestFame ?? 0;
        var stars = FameUtils.FameToStar(bestFame);
        var textureData = ObjectLibrary.TypeToTextureData[_selectedClassType];
        var contentWidth = Math.Min(DetailDividerX - 40, 480);
        var centerX = contentWidth / 2;
        var info = new Container(new ContainerConfig { Width = contentWidth });

        var faceRight = textureData.AnimatedTextures.FaceRight;
        var portraitTexture = faceRight is { Length: > 0 } ? faceRight[0] : textureData.Texture;
        _detailPortrait = new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.Create(portraitTexture, TextureType.GameAtlas),
            X = centerX,
            Y = 52,
            Width = 104,
            Height = 104,
            Anchor = UiAnchor.Middle,
            OutlineEnabled = false,
            GlowEnabled = false
        });

        info.AddChild(_detailPortrait);

        info.AddChild(new SimpleText(new TextConfig {
            Text = props.DisplayName,
            FontSize = 28,
            FontType = FontType.Bold,
            Color = 0xFFFFFF,
            X = centerX,
            Y = 122,
            Anchor = UiAnchor.Middle
        }));

        var description = new SimpleText(new TextConfig {
            Text = props.Description,
            FontSize = 17,
            Color = 0xE0E0E0,
            X = centerX,
            Y = 150,
            MaxWidth = Math.Min(contentWidth - 30, 400),
            Anchor = UiAnchor.MiddleTop
        });

        info.AddChild(description);

        var statsStartY = description.Y + description.Height + 24;
        var labelEndX = centerX + 55;
        var starsX = centerX + 85;
        var numericValueX = starsX - 8;
        AddDetailLabel(info, "Class Quests Completed", labelEndX, statsStartY);
        AddDetailStars(info, stars, starsX, statsStartY);
        AddDetailLabel(info, "Highest Level Achieved", labelEndX, statsStartY + 32);
        AddDetailValue(info, (stats?.BestLevel ?? 0).ToString(), numericValueX, statsStartY + 32);
        AddDetailLabel(info, "Most Fame Achieved", labelEndX, statsStartY + 64);
        var fameValue = AddDetailValue(info, bestFame.ToString(), numericValueX, statsStartY + 64);
        info.AddChild(new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.FromGameAtlas("lofiObj3", 0xE0),
            X = fameValue.X + fameValue.Width + 7,
            Y = fameValue.Y,
            Width = 18,
            Height = 18,
            Anchor = UiAnchor.MiddleLeft,
            OutlineEnabled = false,
            GlowEnabled = false
        }));

        var nextGoalLabelY = statsStartY + 120;
        info.AddChild(new SimpleText(new TextConfig {
            Text = "Next Goal:",
            FontSize = 18,
            FontType = FontType.Bold,
            Color = 0xFFFFFF,
            X = centerX,
            Y = nextGoalLabelY,
            Anchor = UiAnchor.Middle
        }));

        var nextFame = FameUtils.NextStarFame(bestFame, 0);
        var nextGoal = nextFame < 0 ? "All class stars earned" : $"Earn {nextFame} Fame with a {props.DisplayName}";
        var nextGoalText = new SimpleText(new TextConfig {
            Text = nextGoal,
            FontSize = 17,
            Color = 0xE0E0E0,
            X = centerX,
            Y = nextGoalLabelY + 32,
            MaxWidth = Math.Min(contentWidth - 30, 420),
            Anchor = UiAnchor.MiddleTop
        });

        info.AddChild(nextGoalText);

        var contentHeight = nextGoalText.Y + nextGoalText.Height;
        info.Resize(contentWidth, contentHeight);
        info.X = DetailDividerX / 2;
        info.Y = TopSeparatorY + SkinListHeight / 2;
        info.SetAnchor(UiAnchor.Middle);
        _detailScreen.AddChild(info);
    }

    private void BuildSkinList() {
        const int rowGap = 2;
        const int topInset = 5;
        var visibleHeight = SkinListHeight - topInset - 2;
        var listClip = new Container(new ContainerConfig {
            X = DetailDividerX + 2,
            Y = TopSeparatorY + topInset,
            Width = SkinListWidth - 2,
            Height = visibleHeight,
            EnableClip = true
        });

        _detailScreen.AddChild(listClip);

        var rowContainer = new Container { X = 5 };
        listClip.AddChild(rowContainer);

        var rowWidth = SkinListWidth - 27;
        var rowIndex = 0;
        AddSkinRow(rowContainer, rowWidth, rowIndex++, _selectedClassType, 0, "Classic", false);

        var ownedSkins = GlobalData.Get<CharacterListData>()?.OwnedSkins ?? [];
        var skins = ObjectLibrary.TypeToObjectProps.Values
            .Where(props => props.Skin && !props.NoSkinSelect && props.PlayerClassType == _selectedClassType)
            .OrderBy(props => props.DisplayName);

        foreach (var skinProps in skins) {
            var skinType = skinProps.ObjectType;
            AddSkinRow(rowContainer, rowWidth, rowIndex++, skinType, skinType, skinProps.DisplayName,
                !ownedSkins.Contains(skinType));
        }

        var contentHeight = rowIndex * (SkinChoiceRow.RowHeight + rowGap) - rowGap;
        if (contentHeight <= visibleHeight) {
            return;
        }

        listClip.AddChild(new VerticalScrollBar(listClip, new VerticalScrollBarConfig {
            X = SkinListWidth - 12,
            Width = 10,
            Height = visibleHeight,
            TotalContentHeight = contentHeight,
            VisibleContentHeight = visibleHeight,
            ScrollStep = SkinChoiceRow.RowHeight + rowGap,
            OnValueChanged = value => rowContainer.Y = -value
        }));
    }

    private void AddSkinRow(Container parent, int width, int index, ushort textureType, ushort skinType,
        string name, bool locked) {
        var row = new SkinChoiceRow(width, textureType, skinType, name, locked, SelectSkin) {
            Y = index * (SkinChoiceRow.RowHeight + 2)
        };

        parent.AddChild(row);
        _skinRows.Add(row);

        if (skinType == _selectedSkinType) {
            row.SetSelected(true);
        }
    }

    private void SelectSkin(SkinChoiceRow selectedRow) {
        _selectedSkinType = selectedRow.SkinType;
        foreach (var row in _skinRows) {
            row.SetSelected(row == selectedRow);
        }
    }

    private void BuildDetailNavigation() {
        _detailScreen.AddChild(new MenuBarButton(new TextButtonConfig {
            Text = "back",
            FontSize = 32,
            FontType = FontType.Bold,
            OutlineThickness = 4,
            OnClicked = ShowClassScreen,
            X = 50,
            Y = TitleMenuRibbon.MenuCenterY,
            Anchor = UiAnchor.MiddleLeft
        }));

        _detailScreen.AddChild(new MenuBarButton(new TextButtonConfig {
            Text = "play",
            FontSize = 46,
            FontType = FontType.Bold,
            OutlineThickness = 4,
            OnClicked = Play,
            X = _screenWidth / 2,
            Y = TitleMenuRibbon.MenuCenterY,
            Anchor = UiAnchor.Middle
        }));
    }

    private void ShowClassScreen() {
        _detailScreen.Visible = false;
        _classScreen.Visible = true;
        _detailPortrait = null;
    }

    private void Play() {
        GlobalData.CharacterType = _selectedClassType;
        GlobalData.CharacterSkin = _selectedSkinType;
        ScreenManager.FadeToScreen(new GameScreen(), Easing.SineInOut, 1000, 0x0);
    }

    private static void AddDetailLabel(Container parent, string text, int x, int y) {
        parent.AddChild(new SimpleText(new TextConfig {
            Text = text,
            FontSize = 17,
            FontType = FontType.Bold,
            Color = 0xFFFFFF,
            X = x,
            Y = y,
            Anchor = UiAnchor.MiddleRight
        }));
    }

    private static SimpleText AddDetailValue(Container parent, string text, int x, int y) {
        var value = new SimpleText(new TextConfig {
            Text = text,
            FontSize = 17,
            Color = 0xFFFFFF,
            X = x,
            Y = y,
            Anchor = UiAnchor.MiddleLeft
        });

        parent.AddChild(value);
        return value;
    }

    private void AnimateDetailPortrait() {
        if (!_detailScreen.Visible || _detailPortrait == null ||
            !ObjectLibrary.TypeToTextureData.TryGetValue(_selectedClassType, out var textureData)) {
            return;
        }

        var frames = textureData.AnimatedTextures.FaceRight;
        if (frames is not { Length: > 2 }) {
            return;
        }

        const int frameDuration = 250;
        var frameIndex = 1 + (int)(Stage.GameTime.TotalMs / frameDuration) % 2;
        _detailPortrait.ChangeTexture(TextureHelper.Create(frames[frameIndex], TextureType.GameAtlas));
    }

    private static void AddDetailStars(Container parent, int earnedStars, int x, int y) {
        const int size = 16;
        for (var i = 0; i < FameUtils.StarFameRequirements.Length; i++) {
            var star = new ObjectRect(new ObjectRectConfig {
                Texture = TextureHelper.FromUiAtlas("CharacterList/StarGraphic"),
                X = x + i * size,
                Y = y,
                Width = size,
                Height = size,
                Anchor = UiAnchor.Middle,
                OutlineEnabled = false,
                GlowEnabled = false
            });

            if (i >= earnedStars) {
                star.ColorTransformation = new ColorTransform(0.45f, 0.45f, 0.45f, 1f);
            }

            parent.AddChild(star);
        }
    }

    private static ClassStats GetClassStats(ushort type) {
        return GlobalData.Get<AccountData>()?.Stats?.ClassStats?.FirstOrDefault(stats => stats.ObjectType == type);
    }

}
