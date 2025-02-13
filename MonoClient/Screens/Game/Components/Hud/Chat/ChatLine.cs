using MonoClient.Data;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;

namespace MonoClient.Screens.Game.Components.Hud.Chat;

// TODO:
// NumStars icon
// Text wrapping
// Name click handling

public class ChatLine {

    private const string ServerChatName = "";
    private const string ClientChatName = "*Client*";
    private const string ErrorChatName = "*Error*";
    private const string HelpChatName = "*Help*";
    private const string GuildChatName = "*Guild*";
    private const char EnemyNameChar = '#';
    private const char AdminNameChar = '@';

    private int _time;
    private string _sender;
    private int _numStars;
    private int _senderObjectId;
    private string _recipient;
    private string _text;
    private bool _toMe;

    public Sprite Sprite { get; private set; }

    public ChatLine(int time, ChatLineData data) {
        _time = time;
        _numStars = data.NumStars;
        _sender = data.Name;
        _senderObjectId = data.ObjectId;
        _recipient = data.Recipient;
        _text = data.Txt;
        _toMe = _recipient == Account.Username;

        Sprite = CreateSprite();
    }

    private Sprite CreateSprite() {
        var lineSprite = new Container(new ContainerConfig {
            Anchor = UiAnchor.LeftBottom,
            Width = ChatView.MaxWidth,
        });

        string name = _sender;
        int xOffset = 0;
        
        TextConfig nameFormat = ChatFormats.PlayerFormat;
        TextConfig textFormat = _sender switch {
            ServerChatName => ChatFormats.ServerFormat,
            ClientChatName => ChatFormats.ClientFormat,
            HelpChatName => ChatFormats.HelpFormat,
            ErrorChatName => ChatFormats.ErrorFormat,
            _ => ChatFormats.NormalFormat,
        };

        if (name.StartsWith('*'))
            name = string.Empty;

        if (name.StartsWith(EnemyNameChar)) {
            name = _sender[1..];
            nameFormat = ChatFormats.EnemyFormat;
        }
        
        if (name.StartsWith(AdminNameChar)) {
            name = _sender[1..];
            nameFormat = ChatFormats.EnemyFormat;
        }

        string prefix = string.Empty;

        if (_sender == GuildChatName) {
            nameFormat = textFormat = ChatFormats.GuildFormat;
        } else if (!string.IsNullOrEmpty(_recipient)) {
            nameFormat = textFormat = ChatFormats.TellFormat;
            nameFormat.Bold = 1;
            if (!_toMe) {
                prefix = "To: ";
                name = _recipient;
            }
        }
        
        if (_numStars >= 0) {
            var starIcon = FameUtils.StarsToIcon(_numStars);
            lineSprite.AddChild(starIcon);
            
            xOffset += starIcon.Width;
        }

        if (!string.IsNullOrEmpty(name)) {
            nameFormat.Bold = 1;
            
            var nameText = new SimpleText(nameFormat);
            nameText.SetText($"{prefix}<{name}> ");
            
            nameText.X = xOffset;
            lineSprite.AddChild(nameText);

            nameText.MouseEnabled = true;
            xOffset += nameText.Width;
        }

        textFormat.MaxWidth = lineSprite.Width;
        
        var messageText = new SimpleText(textFormat);
        messageText.OffsetFirstLineBy(xOffset);
        messageText.SetText(_text);
        
        lineSprite.AddChild(messageText);
        
        return lineSprite;
    }
}