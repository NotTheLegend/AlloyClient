using System;

namespace MonoClient.Objects.Util.ItemDatas;

public class ConditionEffectDesc : ItemData {

    public int EffectId;
    public string EffectName;
    public int DurationMS;
    public float Range;
    
    public ConditionEffectDesc(string eff, float duration, float range = 0f) {
        EffectName = eff;
        EffectId = ConditionEffectUtil.GetConditionEffectId(eff);
        if (duration < 100) { // nah, this is crazy
            duration *= 1000;
        }
        DurationMS = (int)duration;
        Range = range;
    }

    public string GetTextEffectName() {
        var ret = "";
        for (var i = 0; i < EffectName.Length; i++) {
            if (i != 0 && EffectName[i] == char.ToUpper(EffectName[i])) {
                ret += " ";
            }
            ret += EffectName[i];
        }
        return ret;
    }
}