using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;

public class TTSExternalEditorCommand
{
    [JsonPropertyName("messageID")]
    public CommandType CommandType { get; set; } = CommandType.GetLuaScripts;

    [JsonPropertyName("guid")]
    public string? GUID { get; set; } = null;
    [JsonPropertyName("script")]
    public string? Script { get; set; } = null;
   
    [JsonPropertyName("scriptStates")]
    public List<ObjectState>? ObjectStates { get; set; } = null;

    public TTSExternalEditorCommand() { }
    public TTSExternalEditorCommand(List<ObjectState> objectStates)
    {
        CommandType = CommandType.SaveAndPlay;
        ObjectStates = objectStates;
    }

    public TTSExternalEditorCommand(string guid, string luaScript)
    {
        CommandType = CommandType.ExecuteLua;
        GUID = guid;
        Script = luaScript;
    }
}

public enum CommandType
{
    GetLuaScripts = 0,
    SaveAndPlay = 1,
    SendCustomMessage = 2,
    ExecuteLua =  3
}
