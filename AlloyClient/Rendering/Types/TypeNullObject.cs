using System.Collections.Generic;
using AlloyClient.Assets;
using AlloyClient.Rendering.VertexData;
using Alloy.Common.Structs;

namespace AlloyClient.Rendering.Types;

public sealed class TypeNullObject : RenderBase {
    public override ModelType ModelType {
        get => ModelType.Null;
    }

    public override bool HasShadow {
        get => false;
    }
    
    public override void SetPosition(float x, float y, float z = 0) { }

    public override void SetTexture(AtlasData tex, bool _) { }
    
    public override void SetVisibility(bool _) { }
    
    public override void SetDepth(float _) { }
    
    public override void SetAlpha(float _) { }
    
    public override void SetName(string name) { }
    
    public override void Draw(List<VertexObject> targets) { }
}