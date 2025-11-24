namespace AlloyClient.Networking.Enums;

public enum EffectType {
    Heal = 1, // target, color
    Teleport = 2, // pos1
    Stream = 3, // pos1, pos2, color
    Throw = 4, // target, pos1, color, duration
    Nova = 5, // target, radius, color
    Poison = 6, // target, color
    Trail = 7, // pos1, color
    Burst = 8, // target, pos1, pos2, color
    Flow = 9, // target, pos1, color
    Trap = 10, // target, radius, color
    Lightning = 11, // target, pos1, color, particle size
    Collapse = 12, // target, pos1, pos2, color
    ConeBlast = 13, // target, pos1, radius, color
    Earthquake = 14, // oryx shake. TODO: add params
    Flashing = 15, // target, color, period, cycles
    ObjectToss = 16, //sprite=targetObjId, rotate,
    Vortex = 17, // target, radius, color
    FadeToBlack = 18 // fades screen to black over the course of x milliseconds
}