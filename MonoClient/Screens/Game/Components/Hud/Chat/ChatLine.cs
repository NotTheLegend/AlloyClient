using MonoClient.Data;
using MonoClient.UiLib;
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

    public ChatLine(int time, ChatLineData data) {
        _time = time;
        _numStars = data.NumStars;
        _sender = data.Name;
        _senderObjectId = data.ObjectId;
        _recipient = data.Recipient;
        _text = data.Txt;
        _toMe = _sender == Account.Username;
    }

    private Sprite _lineSprite = null;
    public Sprite GetSprite() {
        if (_lineSprite is not null) 
            return _lineSprite;
        
        var lineSprite = new Container(new ContainerConfig {
            Anchor = UiAnchor.LeftBottom,
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
            nameFormat.Bold = true;
            if (!_toMe) {
                prefix = "To: ";
                name = _recipient;
            }
        }
        
        if (_numStars >= 0) {
            var starSprite = FameUtils.StarsToIcon(_numStars);
            lineSprite.AddChild(starSprite);
            
            xOffset += starSprite.Width;
        }

        if (!string.IsNullOrEmpty(name)) {
            nameFormat.Bold = true;
            
            var nameSprite = new SimpleText(nameFormat);
            nameSprite.SetText($"{prefix}<{name}> ");
            
            nameSprite.X = xOffset;
            lineSprite.AddChild(nameSprite);

            xOffset += nameSprite.Width;
        }
        
        var textSprite = new SimpleText(textFormat);
        textSprite.SetText(_text);
        
        textSprite.X = xOffset;
        lineSprite.AddChild(textSprite);

        _lineSprite = lineSprite;
        return lineSprite;
    }
}