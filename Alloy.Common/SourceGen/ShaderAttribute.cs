namespace Alloy.Common.SourceGen;

[AttributeUsage(AttributeTargets.Property)]
public class ShaderAttribute(string name) : Attribute {
    public readonly string Name = name;
}