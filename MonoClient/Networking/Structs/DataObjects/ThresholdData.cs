using System.Collections.Generic;

namespace MonoClient.Networking.Structs.DataObjects;

public struct ThresholdData : IDataObject {
    private List<byte> _thresholdTypes;
    private List<double> _baseThresholds;

    public void Reset() {
        _thresholdTypes = null;
        _baseThresholds = null;
    }

    public void Read(NetworkReader reader) {
        var count = reader.ReadByte();
        _thresholdTypes = new List<byte>(count);
        _baseThresholds = new List<double>(count);

        for (var i = 0; i < count; i++) {
            _thresholdTypes.Add(reader.ReadByte());
            _baseThresholds.Add(reader.ReadDouble());
        }
    }

    public void Write(NetworkWriter writer) {
        writer.Write((byte)_thresholdTypes.Count);

        for (var i = 0; i < _thresholdTypes.Count; i++) {
            writer.Write(_thresholdTypes[i]);
            writer.Write(_baseThresholds[i]);
        }
    }

    public override string ToString() {
        return $"ThresholdTypes: {_thresholdTypes}, BaseThresholds: {_baseThresholds}";
    }
}