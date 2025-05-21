public interface IUsable
{
    void EnableUsableFunction();
    void DisableUsableFunction();
    void Use();
    bool IsInUsableMode(); // true when the usable mode is currently active
}
