using Newtonsoft.Json;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;

public class ObjectState //Moveable objects
{
    public string Name = ""; //Internal object name
    public TransformState? Transform; //Position, Rotation, Scale
    public string Nickname = ""; //Name supplied in game
    public string Description = "";
    public ColourState? ColorDiffuse; //Material color tint
    /*Toggles*/
    public bool Locked = false; //Freeze object in place
    public bool Grid = true; //Object will snap to grid
    public bool Snap = true; //Object will snap to snap points  
    public bool Autoraise = true; //Object will raise above others and avoid collision
    public bool Sticky = true; //When picked up objects above this one will be attached to it
    public bool Tooltip = true; //When hovering object will display tooltips
    public bool GridProjection = false; //Grid will project on this object
    /*Nullable to hide object specific properties and save space*/
    public bool? HideWhenFaceDown; //When face down object is question mark hidden
    public bool? Hands; //Object will enter player hands
    public bool? AltSound; //Some objects have 2 materials, with two sound sets
    public int? MaterialIndex; //Some objects can have multiple materials
    public int? MeshIndex; //Some objects can have multiple meshes
    public int? Layer; //Sound Layer
    public int? Number;
    public int? CardID;
    public bool? SidewaysCard;
    public bool? RPGmode;
    public bool? RPGdead;
    public string? FogColor = null;
    public bool? FogHidePointers;
    public bool? FogReverseHiding;
    public bool? FogSeethrough;
    public List<int> DeckIDs = new List<int>();
    public Dictionary<int, CustomDeckState> CustomDeck = new(); //Key matches the hundreth place of the id (ex. id = 354, index = 3)
    public CustomMeshState? CustomMesh;
    public CustomImageState? CustomImage;
    public CustomAssetbundleState? CustomAssetbundle;
    public FogOfWarSaveState? FogOfWar;
    public FogOfWarRevealerSaveState? FogOfWarRevealer;
    public object? Clock = null; //public ClockSaveState Clock;
    public CounterState? Counter;
    public TabletState? Tablet;
    public Mp3PlayerState? Mp3Player;
    public CalculatorState? Calculator;
    public TextState Text = new();
    public string XmlUI = ""; //Custom Xml UI
    public List<CustomAssetState> CustomUIAssets = new();
    public string LuaScript = "";
    public string LuaScriptState = ""; // Serialized running Lua code
    public List<ObjectState> ContainedObjects = new(); //Objects inside this one
    public PhysicsMaterialState PhysicsMaterial = new(); //Use to modify the physics material (friction, bounce, etc.) http://docs.unity3d.com/Manual/class-PhysicMaterial.html
    public RigidbodyState Rigidbody = new(); //Use to modify the physical properties (mass, drag, etc) http://docs.unity3d.com/Manual/class-Rigidbody.html
    public object? JointFixed = null;                    //public JointFixedState JointFixed; //Joints can be used to attached/link objects together check the classes below
    public object? JointHingeState = null;               //public JointHingeState JointHinge;
    public object? JointSpring = null;                   //public JointSpringState JointSpring;
    public string? GUID = null; //Used so objects can reference other objects, ex. joints or scripting
    public List<SnapPointState> AttachedSnapPoints = new(); //Snap points that are stuck to this object, happens when placing a snap point on a locked object
    public List<VectorLineState> AttachedVectorLines = new(); // Vector lines that are stuck to this object, happens when drawing a vector line on a locked object
    public List<DecalState> AttachedDecals = new(); //Decals that are attached to this objects
    public Dictionary<int, ObjectState> States = new(); //Objects can have multiple states which can be swapped between    
    public List<RotationValueState> RotationValues = new(); //Rotation values are tooltip values tied to rotations

    public bool EqualsObject(object obj)
    {
        if (obj == null)
            return false;
        
        if (obj is not ObjectState os)
            return false;

        TransformState? ThisTransform = this.Transform;
        TransformState? OtherTransform = os.Transform;

        ColourState? ThisColor = this.ColorDiffuse;
        ColourState? OtherColor = os.ColorDiffuse;
        /*
        if (ThisColor != null && OtherColor != null && ThisColor.ToColour() != OtherColor.ToColour())
        {
            return false;
        }
        */

        string? ThisGUID = this.GUID;
        string? OtherGUID = os.GUID;

        bool ThisLocked = this.Locked;
        bool ThisGrid = this.Grid;
        bool ThisSnap = this.Snap;
        bool ThisAutoraise = this.Autoraise;
        bool ThisSticky = this.Sticky;
        bool ThisTooltip = this.Tooltip;
        bool ThisGridProjection = this.GridProjection;
        bool? ThisHideWhenFaceDown = this.HideWhenFaceDown;
        bool? ThisHands = this.Hands;

        bool OtherLocked = os.Locked;
        bool OtherGrid = os.Grid;
        bool OtherSnap = os.Snap;
        bool OtherAutoraise = os.Autoraise;
        bool OtherSticky = os.Sticky;
        bool OtherTooltip = os.Tooltip;
        bool OtherGridProjection = os.GridProjection;
        bool? OtherHideWhenFaceDown = os.HideWhenFaceDown;
        bool? OtherHands = os.Hands;

        this.Transform = null;
        os.Transform = null;
        this.ColorDiffuse = null;
        os.ColorDiffuse = null;
        this.GUID = null;
        os.GUID = null;

        this.Locked = false;
        this.Grid = false;
        this.Snap = false;
        this.Autoraise = false;
        this.Sticky = false;
        this.Tooltip = false;
        this.GridProjection = false;
        this.HideWhenFaceDown = false;
        this.Hands = null;

        os.Locked = false;
        os.Grid = false;
        os.Snap = false;
        os.Autoraise = false;
        os.Sticky = false;
        os.Tooltip = false;
        os.GridProjection = false;
        os.HideWhenFaceDown = false;
        os.Hands = null;

        string ThisJson = this.ToJson();// Json.GetJson(this, false);
        string NewJson = this.ToJson(); // Json.GetJson(os, false);

        this.Transform = ThisTransform;
        os.Transform = OtherTransform;
        this.ColorDiffuse = ThisColor;
        os.ColorDiffuse = OtherColor;
        this.GUID = ThisGUID;
        os.GUID = OtherGUID;

        this.Locked = ThisLocked;
        this.Grid = ThisGrid;
        this.Snap = ThisSnap;
        this.Autoraise = ThisAutoraise;
        this.Sticky = ThisSticky;
        this.Tooltip = ThisTooltip;
        this.GridProjection = ThisGridProjection;
        this.HideWhenFaceDown = ThisHideWhenFaceDown;
        this.Hands = ThisHands;

        os.Locked = OtherLocked;
        os.Grid = OtherGrid;
        os.Snap = OtherSnap;
        os.Autoraise = OtherAutoraise;
        os.Sticky = OtherSticky;
        os.Tooltip = OtherTooltip;
        os.GridProjection = OtherGridProjection;
        os.HideWhenFaceDown = OtherHideWhenFaceDown;
        os.Hands = OtherHands;

        return ThisJson == NewJson;
    }

    public ObjectState Clone()
    {
        return JsonConvert.DeserializeObject<ObjectState>(this.ToJson()) ?? new ObjectState();// Json.Clone(this);
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);///.GetJson(this);
    }

    public static List<ObjectState> RemoveDuplicates(List<ObjectState> objectStates)
    {
        List<ObjectState> newObjectStates = new List<ObjectState>();

        for (int i = 0; i < objectStates.Count; i++)
        {
            ObjectState objectState = objectStates[i];

            if (objectState == null) continue;

            bool isDuplicate = false;

            for (int j = 0; j < newObjectStates.Count; j++)
            {
                ObjectState os = newObjectStates[j];

                if (objectState.EqualsObject(os))
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (isDuplicate) continue;

            newObjectStates.Add(objectState);
        }

        return newObjectStates;
    }
}

public enum GridType
{
    Box, HexHorizontal, HexVertical
}

public enum AmbientType
{
    Background, Gradient //Background = ambient light comes from the background, Gradient = ambient light comes from the three ambient colors
}

public enum HidingType
{
    Default, Reverse, Disable //Default = only owner can see, Reverse = opposite of default, Disable = hiding is disabled
};

public class HandTransformState
{
    public string? Color;
    public TransformState? Transform;
}

public class TurnsState
{
    public bool Enable;
    public TurnType Type = TurnType.Auto;
    public List<string> TurnOrder = new List<string>();
    public bool Reverse;
    public bool SkipEmpty;
    public bool DisableInteractions;
    public bool PassTurns = true;
    public string? TurnColor;
}

public enum TurnType
{
    Auto, Custom //Auto = turn order is based on positioning of hands on around table, Custom = turn order is based on an user color list
}

public class TextState
{
    public string? Text;
    public ColourState? colorstate;
    public int fontSize = 64;
}

public class TabState
{
    public string? title;
    public string? body;
    public string? color;
    public ColourState? visibleColor;
    public int id = -1;

    [JsonConstructor]
    public TabState() { }
}

public class SnapPointState
{
    public VectorState? Position; //World position when not attached and local position when attached to an object
    public VectorState? Rotation; //Rotate is only set for rotation snap points
}

public class DecalState
{
    public TransformState? Transform;
    public CustomDecalState? CustomDecal;
}

public class CustomDecalState
{
    public string? Name;
    //[Tag(TagType.URL)]
    public string? ImageURL;
    public float? Size; //Size in inches
}

public class RotationValueState
{
    public object? Value;
    public VectorState? Rotation;
}

public class CustomAssetState
{
    public string? Name;
    //[Tag(TagType.URL)]
    public string? URL;
}

public class TransformState
{
    public float posX;
    public float posY;
    public float posZ;

    public float rotX;
    public float rotY;
    public float rotZ;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    [JsonConstructor]
    public TransformState() { }
}

public class ColourState
{
    public float r, g, b;

    public ColourState() { }

    [JsonConstructor]
    public ColourState(float r, float g, float b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
}

public class VectorState
{
    public float x, y, z;

    public VectorState() { }

    [JsonConstructor]
    public VectorState(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class RigidbodyState
{
    public float Mass = 1f; //The mass of the object (arbitrary units). You should not make masses more or less than 100 times that of other Rigidbodies.
    public float Drag = 0.1f; //How much air resistance affects the object when moving from forces. 0 means no air resistance, and infinity makes the object stop moving immediately.
    public float AngularDrag = 0.1f; //How much air resistance affects the object when rotating from torque. 0 means no air resistance. Note that you cannot make the object stop rotating just by setting its Angular Drag to infinity.
    public bool UseGravity = true; //If enabled, the object is affected by gravity.
}

public class PhysicsMaterialState
{
    public float StaticFriction = 0.4f; //The friction used when an object is laying still on a surface. Usually a value from 0 to 1. A value of zero feels like ice, a value of 1 will make it very hard to get the object moving.
    public float DynamicFriction = 0.4f; //The friction used when already moving. Usually a value from 0 to 1. A value of zero feels like ice, a value of 1 will make it come to rest very quickly unless a lot of force or gravity pushes the object.
    public float Bounciness = 0f; //How bouncy is the surface? A value of 0 will not bounce. A value of 1 will bounce without any loss of energy.
    public PhysicMaterialCombine FrictionCombine = PhysicMaterialCombine.Average; //How the friction of two colliding objects is combined. 0 = Average, 1 = Minimum, 2 = Maximum, 3 = Multiply.
    public PhysicMaterialCombine BounceCombine = PhysicMaterialCombine.Average; //How the bounciness of two colliding objects is combined. 0 = Average, 1 = Minimum, 2 = Maximum, 3 = Multiply.
}

public enum PhysicMaterialCombine
{
    Average,
    Minimum,
    Maximum,
    Multiply
}

public class CustomDeckState
{
    //[Tag(TagType.URL)]
    public string FaceURL = "";
    //[Tag(TagType.URL)]
    public string BackURL = "";
    public int? NumWidth;
    public int? NumHeight;
    public bool BackIsHidden = false; //Back of cards becames the hidden card when in a hand
    public bool UniqueBack = false; //Each back is a unique card just like the front
}

public class CustomImageState
{
   // [Tag(TagType.URL)]
    public string ImageURL = "";
    //[Tag(TagType.URL)]
    public string ImageSecondaryURL = "";
    public float WidthScale; //Holds the scaled size of the object based on the image dimensions
    public CustomDiceState? CustomDice;
    public CustomTokenState? CustomToken;
    public CustomJigsawPuzzleState? CustomJigsawPuzzle;
    public CustomTileState? CustomTile;
}

public class CustomAssetbundleState
{
    //[Tag(TagType.URL)]
    public string AssetbundleURL = "";
    //[Tag(TagType.URL)]
    public string AssetbundleSecondaryURL = "";
    public int MaterialIndex = 0; //0 = Plastic, 1 = Wood, 2 = Metal, 3 = Cardboard   
    public int TypeIndex = 0; //0 = Generic, 1 = Figurine, 2 = Dice, 3 = Coin, 4 = Board, 5 = Chip, 6 = Bag, 7 = Infinite   
    public int LoopingEffectIndex = 0;
}

public class CustomDiceState
{
    public DiceType Type;
}

public enum DiceType
{
    SixDiced //???
}

public class CustomTokenState
{
    public float Thickness;
    public float MergeDistancePixels;
    public bool Stackable = false;
}

public class CustomTileState
{
    public TileType Type; //0 = Box, 1 = Hex, 2 = Circle, 3 = Rounded
    public float Thickness;
    public bool Stackable = false;
    public bool Stretch = false;
}

public enum TileType
{
    Box,
    Hex,
    Circle,
    Rounded
}

public class CustomJigsawPuzzleState
{
    public int NumPuzzlePieces = 80;
    public bool ImageOnBoard = true;
}

public class CustomMeshState
{
    //[Tag(TagType.URL)]
    public string MeshURL = "";
    //[Tag(TagType.URL)]
    public string DiffuseURL = "";
    //[Tag(TagType.URL)]
    public string NormalURL = "";
    //[Tag(TagType.URL)]
    public string ColliderURL = "";
    public bool Convex = true;
    public int MaterialIndex = 0; //0 = Plastic, 1 = Wood, 2 = Metal, 3 = Cardboard
    public int TypeIndex = 0; //0 = Generic, 1 = Figurine, 2 = Dice, 3 = Coin, 4 = Board, 5 = Chip, 6 = Bag, 7 = Infinite
    public CustomShaderState? CustomShader; //Used to override the shader
    public bool CastShadows = true;
}

public class CustomShaderState
{
    public ColourState SpecularColor = new ColourState(0.9f, 0.9f, 0.9f);
    public float SpecularIntensity = 0.1f;
    public float SpecularSharpness = 3f; //Range: 2 - 8
    public float FresnelStrength = 0.1f; //Range: 0 - 1
}

public class FogOfWarSaveState
{
    public bool HideGmPointer;
    public bool HideObjects;
    public float Height;
    public Dictionary<string, HashSet<int>> RevealedLocations = new Dictionary<string, HashSet<int>>();
}

public class FogOfWarRevealerSaveState
{
    public bool Active = false;
    public float? Range;
    public string? Color;
}

public class TabletState
{
    //[Tag(TagType.URL)]
    public string PageURL = "";
}

public class CounterState
{
    public int value = 0;
}

public class Mp3PlayerState
{
    public string songTitle = "";
    public string genre = "";
    public float volume = 0.5f;
    public bool isPlaying = false;
    public bool loopOne = false;
    public string menuTitle = "GENRES";
//       public Menus menu = Menus.GENRES;
}

public class CalculatorState
{
    public string value = "";
    public float memory = 0;
}

public class VectorLineState
{
    public List<VectorState> points3 = new List<VectorState>();
    public ColourState? color;
    public float thickness = 0.1f;
    public VectorState? rotation;
    public bool? loop;
    public bool? square;
}

public class CameraState
{
    public VectorState? Position;
    public VectorState? Rotation;
    public float Distance;
    public bool Zoomed = false;
}