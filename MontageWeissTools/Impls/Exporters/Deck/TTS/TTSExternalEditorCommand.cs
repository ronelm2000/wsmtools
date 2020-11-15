using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS
{
    public class TTSExternalEditorCommand
    {
        [JsonProperty("messageID")]
        public CommandType CommandType { get; set; } = CommandType.GetLuaScripts;

        [JsonProperty("guid", NullValueHandling = NullValueHandling.Ignore)]
        public string GUID { get; set; } = null;
        [JsonProperty("script", NullValueHandling = NullValueHandling.Ignore)]
        public string Script { get; set; } = null;
       
        [JsonProperty("scriptStates", NullValueHandling = NullValueHandling.Ignore)]
        public List<ObjectState> ObjectStates { get; set; } = null;

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
}
