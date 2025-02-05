using MonoClient.UiLib.Core;

namespace MonoClient.Utils;

public static class ItemConstants {

    public const int NoItem = -1;
    public const int AllType = 0;
    public const int SwordType = 1;
    public const int DaggerType = 2;
    public const int BowType = 3;
    public const int TomeType = 4;
    public const int ShieldType = 5;
    public const int LeatherType = 6;
    public const int PlateType = 7;
    public const int WandType = 8;
    public const int RingType = 9;
    public const int PotionType = 10;
    public const int SpellType = 11;
    public const int SealType = 12;
    public const int CloakType = 13;
    public const int RobeType = 14;
    public const int QuiverType = 15;
    public const int HelmType = 16;
    public const int StaffType = 17;
    public const int PoisonType = 18;
    public const int SkullType = 19;
    public const int TrapType = 20;
    public const int OrbType = 21;
    public const int PrismType = 22;
    public const int ScepterType = 23;
    public const int KatanaType = 24;
    public const int ShurikenType = 25;

    public static TextureInfo GetSlot(int slotType) {
        switch (slotType) {
            case AllType:
                break;
            case SwordType:
                return TextureInfo.FromGameAtlas("lofiObj5", 48);
            case DaggerType:
                return TextureInfo.FromGameAtlas("lofiObj5", 96);
            case BowType:
                return TextureInfo.FromGameAtlas("lofiObj5", 80);
            case TomeType:
                return TextureInfo.FromGameAtlas("lofiObj6", 80);
            case ShieldType:
                return TextureInfo.FromGameAtlas("lofiObj6", 112);
            case LeatherType:
                return TextureInfo.FromGameAtlas("lofiObj5", 0);
            case PlateType:
                return TextureInfo.FromGameAtlas("lofiObj5", 32);
            case WandType:
                return TextureInfo.FromGameAtlas("lofiObj5", 64);
            case RingType:
                return TextureInfo.FromGameAtlas("lofiObj", 44);
            case SpellType:
                return TextureInfo.FromGameAtlas("lofiObj6", 64);
            case SealType:
                return TextureInfo.FromGameAtlas("lofiObj6", 160);
            case CloakType:
                return TextureInfo.FromGameAtlas("lofiObj6", 32);
            case RobeType:
                return TextureInfo.FromGameAtlas("lofiObj5", 16);
            case QuiverType:
                return TextureInfo.FromGameAtlas("lofiObj6", 48);
            case HelmType:
                return TextureInfo.FromGameAtlas("lofiObj6", 96);
            case StaffType:
                return TextureInfo.FromGameAtlas("lofiObj5", 112);
            case PoisonType:
                return TextureInfo.FromGameAtlas("lofiObj6", 128);
            case SkullType:
                return TextureInfo.FromGameAtlas("lofiObj6", 0);
            case TrapType:
                return TextureInfo.FromGameAtlas("lofiObj6", 16);
            case OrbType:
                return TextureInfo.FromGameAtlas("lofiObj6", 144);
            case PrismType:
                return TextureInfo.FromGameAtlas("lofiObj6", 176);
            case ScepterType:
                return TextureInfo.FromGameAtlas("lofiObj6", 192);
            case KatanaType:
                return TextureInfo.FromGameAtlas("lofiObj3", 540);
            case ShurikenType:
                return TextureInfo.FromGameAtlas("lofiObj3", 555);
        }

        return AssetUtils.GetTextureInfo(0x0096);
    }
}