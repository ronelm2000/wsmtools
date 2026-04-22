namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;

public class GridState
{
    public GridType Type = GridType.Box;
    public bool Lines = false;
    public ColourState Color = new ColourState(0, 0, 0);
    public float Opacity = 0.75f; //0-1 Alpha opacity
    public bool ThickLines = false;
    public bool Snapping = false; //Line snapping
    public bool Offset = false; //Center snapping
    public bool BothSnapping = false; //Both snapping
    public float xSize = 2f;
    public float ySize = 2f;
    public VectorState PosOffset = new VectorState(0, 1, 0);
}
