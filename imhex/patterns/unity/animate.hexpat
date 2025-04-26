#pragma author zodream
#pragma description Unity Animation Asset

import zodream.io;
import std.io;
import std.sys;

struct UnityMatrix {
    s32 length;
    float value[16 * length];
};

struct PPtr {
    s32 FileID;
    s64 PathID;
};


struct PackedFloatVector {
    u32 NumItems;
    float Range;
    float Start;
    zodream::List<u8> Data;
    zodream::AlignTo<4>;
    u8 BitSize;
    zodream::AlignTo<4>;
};

struct PackedIntVector {
    u32 NumItems;
    zodream::List<u8> Data;
    zodream::AlignTo<4>;
    u8 BitSize;
    zodream::AlignTo<4>;
};

struct PackedQuatVector {
    u32 NumItems;
    zodream::List<u8> Data;
    zodream::AlignTo<4>;
};

struct StreamingInfo {
    s64 offset;
    u32 size;
    zodream::AlignString path;
};


struct Keyframe<T> {
    float time;
    T value;
    T inSlope;
    T outSlope;
    s32 weightedMode;
    T inWeight;
    T outWeight;
};

struct AnimationCurve<T> {
    zodream::List<Keyframe<T>> Curve;
    s32 PreInfinity;
    s32 PostInfinity;
    s32 RotationOrder;
};

struct QuaternionCurve {
    AnimationCurve<Vector4> curve;
    zodream::AlignString path;
};

struct CompressedAnimationCurve {
    zodream::AlignString Path;
    PackedIntVector Times;
    PackedQuatVector Values;
    PackedFloatVector Slopes;
    s32 PreInfinity;
    s32 PostInfinity;
};

struct FloatCurve {
    AnimationCurve<float> curve;
    zodream::AlignString attribute;
    zodream::AlignString path;
    s32 classID;
    PPtr script;
    s32 flags;
};

struct PPtrKeyframe {
    float time;
    PPtr value;
};

struct PPtrCurve {
    PPtrKeyframe curve;
    zodream::AlignString attribute;
    zodream::AlignString path;
    s32 classID;
    PPtr script;
    s32 flags;
};

 struct HumanGoal {
    zodream::XForm<Vector3> x;
    float WeightT;
    float WeightR;
    zodream::Vector3 HintT;
    float HintWeightT;
 };

 struct HandPose {
    zodream::XForm<Vector3> GrabX;
    zodream::List<float> DoFArray;
    float Override;
    float CloseOpen;
    float InOut;
    float Grab;
 };

struct HumanPose {
    zodream::XForm<Vector3> RootX;
    zodream::Vector3 LookAtPosition;
    zodream::Vector4 LookAtWeight;
    zodream::List<HumanGoal> GoalArray;
    HandPose LeftHandPose;
    HandPose RightHandPose;
    zodream::List<float> DoFArray;
    zodream::List<Vector3> TDoFArray;
};

struct StreamedClip {
    zodream::List<u32> data;
    u32 curveCount;
};

struct DenseClip {
    s32 FrameCount;
    u32 CurveCount;
    float SampleRate;
    float BeginTime;
    zodream::List<float> SampleArray;
};

struct ConstantClip {
    zodream::List<float> data;
};

 struct Clip {
    StreamedClip streamed;
    DenseClip dense;
    ConstantClip constant;
 };

 struct ValueDelta {
    float Start;
    float Stop;
 };

struct ClipMuscleConstant {
    HumanPose DeltaPose;
    zodream::XForm<Vector3> StartX;
    zodream::XForm<Vector3> StopX;
    zodream::XForm<Vector3> LeftFootStartX;
    zodream::XForm<Vector3> RightFootStartX;
    zodream::Vector3 AverageSpeed;
    Clip clip;
    float StartTime;
    float StopTime;
    float OrientationOffsetY;
    float Level;
    float CycleOffset;
    float AverageAngularSpeed;
    zodream::AlignTo<4>;
    zodream::List<s32> IndexArray;
    zodream::List<ValueDelta> ValueArrayDelta;
    zodream::List<float> ValueArrayReferencePose;
    bool Mirror;
    bool LoopTime;
    bool LoopBlend;
    bool LoopBlendOrientation;
    bool LoopBlendPositionY;
    bool LoopBlendPositionXZ;
    bool StartAtOrigin;
    bool KeepOriginalOrientation;
    bool KeepOriginalPositionY;
    bool KeepOriginalPositionXZ;
    bool HeightFromFeet;
    zodream::AlignTo<4>;
};

struct GenericBinding {
    u32 path;
    u32 attribute;
    PPtr script;
    s32 typeID;
    u8 customType;
    u8 isPPtrCurve;
    u8 isIntCurve;
    zodream::AlignTo<4>;
};

struct AnimationClipBindingConstant {
    zodream::List<GenericBinding> genericBindings;
    zodream::List<PPtr> pptrCurveMapping;
};

struct AnimationEvent {
    float time;
    zodream::AlignString functionName;
    zodream::AlignString data;
    PPtr objectReferenceParameter;
    float floatParameter;
    s32 intParameter;
    s32 messageOptions;
};

struct AnimationClip {
    zodream::AlignString name;
  
    bool Legacy;
    bool Compressed;
    bool UseHighQualityCurve;
    zodream::AlignTo<4>;
    zodream::List<QuaternionCurve> RotationCurves;
    zodream::List<CompressedAnimationCurve> compressedRotationCurves;
    zodream::List<FloatCurve> floatCurves;
    zodream::List<PPtrCurve> PPtrCurves;
    float SampleRate;
    s32 WrapMode;
    AABB Bounds;
    u32 MuscleClipSize;
    if (MuscleClipSize > 0) {
        u32 _un;
    }
    ClipMuscleConstant MuscleClip;
    StreamingInfo StreamData;

    AnimationClipBindingConstant clipBindingConstant;
    bool HasGenericRootTransform;
    bool HasMotionFloatCurves;
    zodream::AlignTo<4>;
    zodream::List<AnimationEvent> events;
    zodream::AlignTo<4>;
};

AnimationClip model @ 0x0;