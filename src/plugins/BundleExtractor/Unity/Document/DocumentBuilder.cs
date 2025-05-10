using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Document;

namespace ZoDream.BundleExtractor.Unity.Document
{
    internal class DocumentBuilder(Version version)
    {
        private readonly List<VirtualNode> _items = [];

        public void Add(VirtualNode item)
        {
            _items.Add(item);
        }
        public VirtualNode[] ToArray()
        {
            return DocumentConverter.BuildTree([.._items]);
        }

        public VirtualDocument ToDocument()
        {
            return new(version, DocumentConverter.BuildTree([.. _items]));
        }

        public void AddMonoBehavior(int indent)
        {
            Add(new("MonoBehaviour", "Base", indent, false));
            AddPPtr("GameObject", "m_GameObject", indent + 1);
            Add(new("UInt8", "m_Enabled", indent + 1, true));
            AddPPtr("MonoScript", "m_Script", indent + 1);
            AddString("m_Name", indent + 1);
        }

        public void AddPPtr(string type, string name, int indent)
        {
            Add(new($"PPtr<{type}>", name, indent, false));
            Add(new("int", "m_FileID", indent + 1, false));
            if (version.Major >= 5) //5.0 and up
            {
                Add(new("SInt64", "m_PathID", indent + 1, false));
            }
            else
            {
                Add(new("int", "m_PathID", indent + 1, false));
            }
        }

        public void AddString(string name, int indent)
        {
            Add(new("string", name, indent, false));
            Add(new("Array", "Array", indent + 1, true));
            Add(new("int", "size", indent + 2, false));
            Add(new("char", "data", indent + 2, false));
        }

        public void AddArray(int indent)
        {
            Add(new("Array", "Array", indent, false));
            Add(new("int", "size", indent + 1, false));
        }

        public void AddAnimationCurve(string name, int indent)
        {
            Add(new("AnimationCurve", name, indent, false));
            Add(new("vector", "m_Curve", indent + 1, false));
            AddArray(indent + 2); //TODO 2017 and up Array align but no effect 
            Add(new("Keyframe", "data", indent + 3, false));
            Add(new("float", "time", indent + 4, false));
            Add(new("float", "value", indent + 4, false));
            Add(new("float", "inSlope", indent + 4, false));
            Add(new("float", "outSlope", indent + 4, false));
            if (version.Major >= 2018) //2018 and up
            {
                Add(new("int", "weightedMode", indent + 4, false));
                Add(new("float", "inWeight", indent + 4, false));
                Add(new("float", "outWeight", indent + 4, false));
            }
            Add(new("int", "m_PreInfinity", indent + 1, false));
            Add(new("int", "m_PostInfinity", indent + 1, false));
            if (version.GreaterThanOrEquals(5, 3)) //5.3 and up
            {
                Add(new("int", "m_RotationOrder", indent + 1, false));
            }
        }

        public void AddGradient(string name, int indent)
        {
            Add(new("Gradient", name, indent, false));
            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                AddColorRGBA("key0", indent + 1);
                AddColorRGBA("key1", indent + 1);
                AddColorRGBA("key2", indent + 1);
                AddColorRGBA("key3", indent + 1);
                AddColorRGBA("key4", indent + 1);
                AddColorRGBA("key5", indent + 1);
                AddColorRGBA("key6", indent + 1);
                AddColorRGBA("key7", indent + 1);
            }
            else
            {
                AddColor32("key0", indent + 1);
                AddColor32("key1", indent + 1);
                AddColor32("key2", indent + 1);
                AddColor32("key3", indent + 1);
                AddColor32("key4", indent + 1);
                AddColor32("key5", indent + 1);
                AddColor32("key6", indent + 1);
                AddColor32("key7", indent + 1);
            }
            Add(new("UInt16", "ctime0", indent + 1, false));
            Add(new("UInt16", "ctime1", indent + 1, false));
            Add(new("UInt16", "ctime2", indent + 1, false));
            Add(new("UInt16", "ctime3", indent + 1, false));
            Add(new("UInt16", "ctime4", indent + 1, false));
            Add(new("UInt16", "ctime5", indent + 1, false));
            Add(new("UInt16", "ctime6", indent + 1, false));
            Add(new("UInt16", "ctime7", indent + 1, false));
            Add(new("UInt16", "atime0", indent + 1, false));
            Add(new("UInt16", "atime1", indent + 1, false));
            Add(new("UInt16", "atime2", indent + 1, false));
            Add(new("UInt16", "atime3", indent + 1, false));
            Add(new("UInt16", "atime4", indent + 1, false));
            Add(new("UInt16", "atime5", indent + 1, false));
            Add(new("UInt16", "atime6", indent + 1, false));
            Add(new("UInt16", "atime7", indent + 1, false));
            if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
            {
                Add(new("int", "m_Mode", indent + 1, false));
            }
            Add(new("UInt8", "m_NumColorKeys", indent + 1, false));
            Add(new("UInt8", "m_NumAlphaKeys", indent + 1, true));
        }

        public void AddGUIStyle(string name, int indent)
        {
            Add(new("GUIStyle", name, indent, false));
            AddString("m_Name", indent + 1);
            AddGUIStyleState("m_Normal", indent + 1);
            AddGUIStyleState("m_Hover", indent + 1);
            AddGUIStyleState("m_Active", indent + 1);
            AddGUIStyleState("m_Focused", indent + 1);
            AddGUIStyleState("m_OnNormal", indent + 1);
            AddGUIStyleState("m_OnHover", indent + 1);
            AddGUIStyleState("m_OnActive", indent + 1);
            AddGUIStyleState("m_OnFocused", indent + 1);
            AddRectOffset("m_Border", indent + 1);
            if (version.Major >= 4) //4 and up
            {
                AddRectOffset("m_Margin", indent + 1);
                AddRectOffset("m_Padding", indent + 1);
            }
            else
            {
                AddRectOffset("m_Padding", indent + 1);
                AddRectOffset("m_Margin", indent + 1);
            }
            AddRectOffset("m_Overflow", indent + 1);
            AddPPtr("Font", "m_Font", indent + 1);
            if (version.Major >= 4) //4 and up
            {
                Add(new("int", "m_FontSize", indent + 1, false));
                Add(new("int", "m_FontStyle", indent + 1, false));
                Add(new("int", "m_Alignment", indent + 1, false));
                Add(new("bool", "m_WordWrap", indent + 1, false));
                Add(new("bool", "m_RichText", indent + 1, true));
                Add(new("int", "m_TextClipping", indent + 1, false));
                Add(new("int", "m_ImagePosition", indent + 1, false));
                AddVector2f("m_ContentOffset", indent + 1);
                Add(new("float", "m_FixedWidth", indent + 1, false));
                Add(new("float", "m_FixedHeight", indent + 1, false));
                Add(new("bool", "m_StretchWidth", indent + 1, false));
                Add(new("bool", "m_StretchHeight", indent + 1, true));
            }
            else
            {
                Add(new("int", "m_ImagePosition", indent + 1, false));
                Add(new("int", "m_Alignment", indent + 1, false));
                Add(new("bool", "m_WordWrap", indent + 1, true));
                Add(new("int", "m_TextClipping", indent + 1, false));
                AddVector2f("m_ContentOffset", indent + 1);
                AddVector2f("m_ClipOffset", indent + 1);
                Add(new("float", "m_FixedWidth", indent + 1, false));
                Add(new("float", "m_FixedHeight", indent + 1, false));
                if (version.Major >= 3) //3 and up
                {
                    Add(new("int", "m_FontSize", indent + 1, false));
                    Add(new("int", "m_FontStyle", indent + 1, false));
                }
                Add(new("bool", "m_StretchWidth", indent + 1, true));
                Add(new("bool", "m_StretchHeight", indent + 1, true));
            }
        }

        public void AddGUIStyleState(string name, int indent)
        {
            Add(new("GUIStyleState", name, indent, false));
            AddPPtr("Texture2D", "m_Background", indent + 1);
            AddColorRGBA("m_TextColor", indent + 1);
        }

        public void AddVector2f(string name, int indent)
        {
            Add(new("Vector2f", name, indent, false));
            Add(new("float", "x", indent + 1, false));
            Add(new("float", "y", indent + 1, false));
        }

        public void AddRectOffset(string name, int indent)
        {
            Add(new("RectOffset", name, indent, false));
            Add(new("int", "m_Left", indent + 1, false));
            Add(new("int", "m_Right", indent + 1, false));
            Add(new("int", "m_Top", indent + 1, false));
            Add(new("int", "m_Bottom", indent + 1, false));
        }

        public void AddColorRGBA(string name, int indent)
        {
            Add(new("ColorRGBA", name, indent, false));
            Add(new("float", "r", indent + 1, false));
            Add(new("float", "g", indent + 1, false));
            Add(new("float", "b", indent + 1, false));
            Add(new("float", "a", indent + 1, false));
        }

        public void AddColor32(string name, int indent)
        {
            Add(new("ColorRGBA", name, indent, false));
            Add(new("unsigned int", "rgba", indent + 1, false));
        }

        public void AddMatrix4x4(string name, int indent)
        {
            Add(new("Matrix4x4f", name, indent, false));
            Add(new("float", "e00", indent + 1, false));
            Add(new("float", "e01", indent + 1, false));
            Add(new("float", "e02", indent + 1, false));
            Add(new("float", "e03", indent + 1, false));
            Add(new("float", "e10", indent + 1, false));
            Add(new("float", "e11", indent + 1, false));
            Add(new("float", "e12", indent + 1, false));
            Add(new("float", "e13", indent + 1, false));
            Add(new("float", "e20", indent + 1, false));
            Add(new("float", "e21", indent + 1, false));
            Add(new("float", "e22", indent + 1, false));
            Add(new("float", "e23", indent + 1, false));
            Add(new("float", "e30", indent + 1, false));
            Add(new("float", "e31", indent + 1, false));
            Add(new("float", "e32", indent + 1, false));
            Add(new("float", "e33", indent + 1, false));
        }

        public void AddSphericalHarmonicsL2(string name, int indent)
        {
            Add(new("SphericalHarmonicsL2", name, indent, false));
            Add(new("float", "sh[ 0]", indent + 1, false));
            Add(new("float", "sh[ 1]", indent + 1, false));
            Add(new("float", "sh[ 2]", indent + 1, false));
            Add(new("float", "sh[ 3]", indent + 1, false));
            Add(new("float", "sh[ 4]", indent + 1, false));
            Add(new("float", "sh[ 5]", indent + 1, false));
            Add(new("float", "sh[ 6]", indent + 1, false));
            Add(new("float", "sh[ 7]", indent + 1, false));
            Add(new("float", "sh[ 8]", indent + 1, false));
            Add(new("float", "sh[ 9]", indent + 1, false));
            Add(new("float", "sh[10]", indent + 1, false));
            Add(new("float", "sh[11]", indent + 1, false));
            Add(new("float", "sh[12]", indent + 1, false));
            Add(new("float", "sh[13]", indent + 1, false));
            Add(new("float", "sh[14]", indent + 1, false));
            Add(new("float", "sh[15]", indent + 1, false));
            Add(new("float", "sh[16]", indent + 1, false));
            Add(new("float", "sh[17]", indent + 1, false));
            Add(new("float", "sh[18]", indent + 1, false));
            Add(new("float", "sh[19]", indent + 1, false));
            Add(new("float", "sh[20]", indent + 1, false));
            Add(new("float", "sh[21]", indent + 1, false));
            Add(new("float", "sh[22]", indent + 1, false));
            Add(new("float", "sh[23]", indent + 1, false));
            Add(new("float", "sh[24]", indent + 1, false));
            Add(new("float", "sh[25]", indent + 1, false));
            Add(new("float", "sh[26]", indent + 1, false));
        }

        public void AddPropertyName(string name, int indent)
        {
            Add(new("PropertyName", name, indent, false));
            AddString("id", indent + 1);
        }


        #region CubismLive2D
        public void AddMonoCubismModel(int indent)
        {
            AddPPtr("CubismMoc", "_moc", indent);
        }

        public void AddMonoCubismMoc(int indent)
        {
            Add(new("vector", "_bytes", indent, align: true));
            AddArray(indent + 2);
            Add(new("UInt8", "data", indent + 2, false));
        }

        public void AddMonoCubismPosePart(int indent)
        {
            Add(new("int", "GroupIndex", indent, false));
            Add(new("int", "PartIndex", indent, false));
            Add(new("vector", "Link", indent, align: false));
            AddArray(indent + 2);
            AddString("data", indent + 2);
        }

        public void AddMonoCubismDisplayInfo(int indent)
        {
            AddString("Name", indent);
            AddString("DisplayName", indent);
        }

        public void AddMonoCubismFadeController(int indent)
        {
            AddPPtr("CubismFadeMotionList", "CubismFadeMotionList", indent);
        }

        public void AddMonoCubismFadeList(int indent)
        {
            Add(new("vector", "MotionInstanceIds", indent, align: false));
            AddArray(indent + 2);
            Add(new("int", "data", indent + 2, align: false));
            Add(new("vector", "CubismFadeMotionObjects", indent, align: false));
            AddArray(indent + 2);
            AddPPtr("CubismFadeMotionData", "data", indent + 2);
        }

        public void AddMonoCubismFadeData(int indent)
        {
            AddString("MotionName", indent);
            Add(new("float", "FadeInTime", indent, false));
            Add(new("float", "FadeOutTime", indent, false));
            Add(new("vector", "ParameterIds", indent, align: false));
            AddArray(indent + 2);
            AddString("data", indent + 2);
            Add(new("vector", "ParameterCurves", indent, align: false));
            AddArray(indent + 2);
            AddAnimationCurve("data", indent + 2);
            Add(new("vector", "ParameterFadeInTimes", indent, align: false));
            AddArray(indent + 2);
            Add(new("float", "data", indent + 2, align: false));
            Add(new("vector", "ParameterFadeOutTimes", indent, align: false));
            AddArray(indent + 2);
            Add(new("float", "data", indent + 2, align: false));
            Add(new("float", "MotionLength", indent, align: false));
        }

        public void AddMonoCubismExpressionController(int indent)
        {
            AddPPtr("CubismExpressionList", "ExpressionsList", indent);
            Add(new("int", "CurrentExpressionIndex", indent, false));
        }

        public void AddMonoCubismExpressionList(int indent)
        {
            Add(new("vector", "CubismExpressionObjects", indent, align: false));
            AddArray(indent + 2);
            AddPPtr("CubismExpressionData", "data", indent + 2);
        }

        private void AddMonoCubismExpressionParameter(string name, int indent)
        {
            Add(new("SerializableExpressionParameter", name, indent, false));
            AddString("Id", indent + 1);
            Add(new("float", "Value", indent + 1, false));
            Add(new("int", "Blend", indent + 1, false));
        }

        public void AddMonoCubismExpressionData(int indent)
        {
            AddString("Type", indent);
            Add(new("float", "FadeInTime", indent, false));
            Add(new("float", "FadeOutTime", indent, false));
            Add(new("SerializableExpressionParameter", "Parameters", indent, align: false));
            AddArray(indent + 2);
            AddMonoCubismExpressionParameter("data", indent + 2);
        }

      
        #endregion
    }
}
