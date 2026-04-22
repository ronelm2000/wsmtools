namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;

public class LightingState
{
    public float LightIntensity = 0.54f; //0-8
    public ColourState LightColor = new ColourState(1f, 0.9804f, 0.8902f);
    public float AmbientIntensity = 1.3f; //0-8
    public AmbientType AmbientType = AmbientType.Background;
    public ColourState AmbientSkyColor = new ColourState(0.5f, 0.5f, 0.5f);
    public ColourState AmbientEquatorColor = new ColourState(0.5f, 0.5f, 0.5f);
    public ColourState AmbientGroundColor = new ColourState(0.5f, 0.5f, 0.5f);
    public float ReflectionIntensity = 1f; //0-1
    public int LutIndex = 0;
    public float LutContribution = 1f; //0-1
    //[Tag(TagType.URL)]
    public string? LutURL; //LUT 256x16
}
