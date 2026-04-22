using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Impls.Exporters.Deck.TTS;

public record SaveState //Holds a state of the game
{
    required public string SaveName { get; init; } = "";
    required public string GameMode { get; init; } = "";
    required public float Gravity { get; init; } = 0.5f;
    required public float PlayArea { get; init; } = 0.5f;
    required public string Date { get; init; } = "";
    required public string Table { get; init; } = "";
    required public string Sky { get; init; } = "";
    required public string Note { get; init; } = "";
    required public string Rules { get; init; } = "";
    required public string XmlUI { get; init; } = ""; //Custom Xml UI
    required public List<CustomAssetState> CustomUIAssets = new();
    required public string LuaScript { get; init; } = "";
    required public string LuaScriptState { get; init; } = ""; // Serialized running Lua code; will run nothing as it's just a save state.    
    required public List<ObjectState> ObjectStates { get; init; } = new List<ObjectState>(); //Objects on the table
    public List<DecalState> Decals { get; init; } = new List<DecalState>(); //Decals not attached to objects
    public Dictionary<int, TabState> TabStates { get; init; } = new Dictionary<int, TabState>(); //Notepad tabs
    public string VersionNumber { get; init; } = "";
}

/*
public class JointState //http://docs.unity3d.com/ScriptReference/Joint.html
{
    public string ConnectedBodyGUID = ""; //A reference to another rigidbody this joint connects to.
    public bool EnableCollision; //Enable collision between bodies connected with the joint.
    public VectorState Axis = new VectorState(); //The Direction of the axis around which the body is constrained.
    public VectorState Anchor = new VectorState(); //The Position of the anchor around which the joints motion is constrained.
    public VectorState ConnectedAnchor = new VectorState(); //Position of the anchor relative to the connected Rigidbody.
    public float BreakForce; //The force that needs to be applied for this joint to break.
    public float BreakTorgue; //The torque that needs to be applied for this joint to break.

    public void Assign(JointState jointState)
    {
        this.ConnectedBodyGUID = jointState.ConnectedBodyGUID;
        this.EnableCollision = jointState.EnableCollision;
        this.Anchor = jointState.Anchor;
        this.Axis = jointState.Axis;
        this.ConnectedAnchor = jointState.ConnectedAnchor;
        this.BreakForce = jointState.BreakForce;
        this.BreakTorgue = jointState.BreakTorgue;
    }
}

public class JointFixedState : JointState //http://docs.unity3d.com/ScriptReference/FixedJoint.html
{
}

public class JointHingeState : JointState //http://docs.unity3d.com/ScriptReference/HingeJoint.html
{
    public bool UseLimits;
    public JointLimits Limits; //Limit of angular rotation on the hinge joint. http://docs.unity3d.com/ScriptReference/JointLimits.html
    public bool UseMotor;
    public JointMotor Motor; //The motor will apply a force up to a maximum force to achieve the target velocity in degrees per second. http://docs.unity3d.com/ScriptReference/JointMotor.html
    public bool UseSpring;
    public JointSpring Spring; //The spring attempts to reach a target angle by adding spring and damping forces. http://docs.unity3d.com/ScriptReference/JointSpring.html
}

public class JointSpringState : JointState //http://docs.unity3d.com/ScriptReference/SpringJoint.html
{
    public float Damper; //The damper force used to dampen the spring force.
    public float MaxDistance; //The maximum distance between the bodies relative to their initial distance.
    public float MinDistance; //The minimum distance between the bodies relative to their initial distance.
    public float Spring; //The spring force used to keep the two objects together.
}
*/
