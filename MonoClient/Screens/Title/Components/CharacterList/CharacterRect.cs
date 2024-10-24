using System;
using Common;
using Common.Atlas;
using MonoClient.Assets.Libraries;
using MonoClient.Data;
using MonoClient.Data.XmlModels;
using MonoClient.Display;
using MonoClient.Screens.Game;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using ColorRect = MonoClient.UiLib.BuiltIn.ColorRect;
using Container = MonoClient.UiLib.BuiltIn.Container;
using ObjectRect = MonoClient.UiLib.BuiltIn.ObjectRect;

namespace MonoClient.Screens.Title.Components.CharacterList;

public enum CharacterRectType {
    Character,
    GraveyardCharacter,
    NewCharacter
}

public sealed class CharacterRect : Container {
    private const int NumberStatsMaxed = 8;
    private const int NumberStars = 5;

    private const int NumberCharacters = 15;

    private readonly CharacterListScreen _characterListScreen;

    private int _statsMaxed;
    private int _baseFame;

    public CharacterRect(CharacterListScreen characterListScreen) : base(new ContainerConfig { Width = 200, Height = 200, EnableClip = true }) {
        _characterListScreen = characterListScreen;

        var background = new ColorRect(new ColorRectConfig {
            Width = 200,
            Height = 200,
            Color = 0x2B2B2B,
            Alpha = 0.7f,
        });
        AddChild(background);
    }

    public void Initialize(CharacterRectType type, CharacterModel character = null, int remainingSlots = 0) {
        var charNameText = new SimpleText(new TextConfig {
            Text = "New Character",
            FontSize = 18,
            Bold = true,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.Middle,
        });
        charNameText.X = Width / 2;
        charNameText.Y = charNameText.Height / 2 + 10;
        AddChild(charNameText);
        
        var textButton = new TextButton(new TextButtonConfig {
            Text = type switch {
                CharacterRectType.Character => "Play",
                CharacterRectType.GraveyardCharacter => "View",
                CharacterRectType.NewCharacter => "Create",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            },
            FontSize = 24,
            Anchor = UiAnchor.Middle,
            OnClicked = type switch {
                CharacterRectType.Character => () => {
                    if (character == null) {
                        return;
                    }

                    Account.SelectedCharacterId = character.Id;
                    ScreenManager.FadeToScreen(new GameScreen(), Easing.SineInOut, 1000, 0x0);
                },
                CharacterRectType.GraveyardCharacter => () => {
                    if (character == null) {
                        return;
                    }
                },
                CharacterRectType.NewCharacter => () => { _characterListScreen.ShowCharacterCreate(); },
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            }
        });
        textButton.X = Width / 2;
        textButton.Y = Height - textButton.Height / 2 - 12;
        AddChild(textButton);

        if (character != null) {
            #region Character and Graveyard Character

            var props = ObjectLibrary.TypeToObjectProps[character.ObjectType];
            charNameText.SetText(props.ObjectId);

            _statsMaxed = GetStatsMaxed(character);
            var statsMaxedText = new SimpleText(new TextConfig {
                Text = $"{_statsMaxed}/{NumberStatsMaxed}",
                FontSize = 16,
                Bold = true,
                Color = 0xCFCFCF,
                Anchor = UiAnchor.Middle,
            });
            statsMaxedText.X = Width / 2;
            statsMaxedText.Y = charNameText.Height + statsMaxedText.Height / 2 + 13;
            AddChild(statsMaxedText);

            switch (_statsMaxed) {
                case NumberStatsMaxed:
                    statsMaxedText.SetColor(0xFCDF00);
                    break;
                case > 0:
                    statsMaxedText.SetColor(0xFFFFFF);
                    break;
            }

            var texture = props.AnimatedTexture;
            var atlasAnimationData = UiRender.GameAtlas.GetAnimationAtlasData(texture.File, texture.Index);
            var atlasData = atlasAnimationData.FaceDown[0];
            atlasData.RemovePadding();
            var charPortrait = new ObjectRect(new ObjectRectConfig {
                Texture = new TextureInfo(atlasData, TextureType.GameAtlas),
                Width = 40,
                Height = 40,
                Anchor = UiAnchor.Middle,
                
            });
            charPortrait.X = Width / 2;
            charPortrait.Y = statsMaxedText.Y + charPortrait.Height / 2 + 14;
            AddChild(charPortrait);
            
            _baseFame = character.CurrentFame;
            
            var fameText = new SimpleText(new TextConfig {
                Text = _baseFame.ToString(),
                FontSize = 16,
                Bold = true,
                Color = 0xFFFFFF,
                Anchor = UiAnchor.Middle,
            });
            fameText.X = Width / 2;
            fameText.Y = charPortrait.Y + charPortrait.Height / 2 + 14;
            AddChild(fameText);

            const int middleStarIndex = NumberStars / 2;
            const int starWidth = 16;
            var startX = Width / 2 - middleStarIndex * starWidth;
            var numStars = GetStars(character);
            for (var i = 0; i < NumberStars; i++) {
                var star = new ObjectRect(new ObjectRectConfig {
                    Texture = TextureInfo.FromUiAtlas("CharacterList/StarGraphic"),
                    Width = starWidth,
                    Height = starWidth,
                    Anchor = UiAnchor.Middle,
                    X = startX + i * starWidth,
                    Y = fameText.Y + starWidth / 2 + 12,
                });
                AddChild(star);

                if (i >= numStars) {
                    star.ColorTransformation = new ColorTransform(0.5f, 0.5f, 0.5f, 1);
                }
            }

            #endregion
        }
        else {
            #region New Character
            
            var index = Random.Shared.Next(0, NumberCharacters);
            var frames = UiRender.GameAtlas.GetAnimationAtlasData("players", index);
            var charPortrait = new ObjectRect(new ObjectRectConfig {
                Texture = new TextureInfo(frames.FaceDown[0], TextureType.GameAtlas),
                Width = 50,
                Height = 50,
                Anchor = UiAnchor.Middle,
            });
            charPortrait.X = Width / 2;
            charPortrait.Y = charNameText.Height + charPortrait.Height / 2 + 25;
            charPortrait.ColorTransformation = new ColorTransform(0, 0, 0, 0.5f);
            AddChild(charPortrait);
            
            var remainingSlotsText = new SimpleText(new TextConfig {
                Text = $"{remainingSlots} Character Slots",
                FontSize = 18,
                Bold = true,
                Color = 0xFFFFFF,
                Anchor = UiAnchor.Middle,
            });
            remainingSlotsText.X = Width / 2;
            remainingSlotsText.Y = charPortrait.Y + charPortrait.Height / 2 + 25;
            AddChild(remainingSlotsText);
            
            var remainingSlotsText2 = new SimpleText(new TextConfig {
                Text = "Remaining",
                FontSize = 18,
                Bold = true,
                Color = 0xFFFFFF,
                Anchor = UiAnchor.Middle,
            });
            remainingSlotsText2.X = Width / 2;
            remainingSlotsText2.Y = remainingSlotsText.Y + remainingSlotsText.Height / 2 + 12;
            AddChild(remainingSlotsText2);

            #endregion
        }
    }

    public int ComputeSortValue() {
        return _statsMaxed * 1000 + _baseFame;
    }

    private static int GetStatsMaxed(CharacterModel characterModel) {
        var player = ObjectLibrary.TypeToObjectProps[characterModel.ObjectType];
        var playerProps = player.PlayerProperties;

        var numStatsMaxed = 0;
        if (characterModel.MaxHitPoints >= playerProps.MaxHp) {
            numStatsMaxed++;
        }

        if (characterModel.MaxMagicPoints >= playerProps.MaxMp) {
            numStatsMaxed++;
        }

        if (characterModel.Attack >= playerProps.MaxAttack) {
            numStatsMaxed++;
        }

        if (characterModel.Defense >= playerProps.MaxDefense) {
            numStatsMaxed++;
        }

        if (characterModel.Speed >= playerProps.MaxSpeed) {
            numStatsMaxed++;
        }

        if (characterModel.Dexterity >= playerProps.MaxDexterity) {
            numStatsMaxed++;
        }

        if (characterModel.HpRegen >= playerProps.MaxVitality) {
            numStatsMaxed++;
        }

        if (characterModel.MpRegen >= playerProps.MaxWisdom) {
            numStatsMaxed++;
        }

        return numStatsMaxed;
    }

    private static int GetStars(CharacterModel characterModel) {
        var fame = characterModel.CurrentFame;
        var accountModel = Account.Model;
        foreach (var classStatModel in accountModel.Stats.ClassStats) {
            var objTypeStr = classStatModel.ObjectType;
            var objectType = Convert.ToUInt16(objTypeStr, XmlUtil.GetBase(objTypeStr));
            if (objectType != characterModel.ObjectType) {
                continue;
            }

            if (fame < classStatModel.BestFame) {
                fame = classStatModel.BestFame;
            }
        }

        return fame switch {
            < 20 => 0,
            < 150 => 1,
            < 400 => 2,
            < 800 => 3,
            < 2000 => 4,
            _ => 5,
        };
    }
}