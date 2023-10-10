public interface IButtonStateService
{
    void LoadSet(string fileName, string setName);
    int? GetMidiNoteFromButtons(bool[] buttons);
}
