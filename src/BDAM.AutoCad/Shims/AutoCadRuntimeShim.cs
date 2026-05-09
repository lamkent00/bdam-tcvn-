#if !AUTOCAD
namespace Autodesk.AutoCAD.Runtime;

[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandMethodAttribute : Attribute
{
    public CommandMethodAttribute(string globalName)
    {
        GlobalName = globalName;
    }

    public string GlobalName { get; }
}

public interface IExtensionApplication
{
    void Initialize();

    void Terminate();
}
#endif
