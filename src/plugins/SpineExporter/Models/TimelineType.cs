namespace ZoDream.SpineExporter.Models
{
    public enum TimelineType
    {
        Rotate = 0, X, Y, ScaleX, ScaleY, ShearX, ShearY, Inherit, //
        RGB, Alpha, RGB2, //
        Attachment, Deform, //
        Event, DrawOrder, //
        IkConstraint, TransformConstraint, //
        PathConstraintPosition, PathConstraintSpacing, PathConstraintMix, //
        PhysicsConstraintInertia, PhysicsConstraintStrength, PhysicsConstraintDamping, PhysicsConstraintMass, //
        PhysicsConstraintWind, PhysicsConstraintGravity, PhysicsConstraintMix, PhysicsConstraintReset, //
        Sequence
    }
}
