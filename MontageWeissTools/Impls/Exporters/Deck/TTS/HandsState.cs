namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;

public class HandsState
{
    public bool Enable = true;
    public bool DisableUnused = false;
    public HidingType Hiding = HidingType.Default;
    public List<HandTransformState> HandTransforms = new List<HandTransformState>();
}
