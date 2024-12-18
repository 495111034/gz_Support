//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using UnityEngine;

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WeatherSystem
{
    /// <summary>
    /// A range of integers
    /// </summary>
    [System.Serializable]
    public struct RangeOfIntegers
    {
        /// <summary>Minimum value (inclusive)</summary>
        [Tooltip("Minimum value (inclusive)")]
        public int Minimum;

        /// <summary>Maximum value (inclusive)</summary>
        [Tooltip("Maximum value (inclusive)")]
        public int Maximum;

        /// <summary>
        /// Generate a random number
        /// </summary>
        /// <returns>Random value</returns>
        public int Random() { return UnityEngine.Random.Range(Minimum, Maximum + 1); }

        /// <summary>
        /// Generate a random number using a specific random instance
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns>Random value</returns>
        public int Random(System.Random r) { return r.Next(Minimum, Maximum + 1); }

        /// <summary>
        /// Convert min and max to Vector2
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 ToVector2() { return new Vector2(Minimum, Maximum); }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return "Min: " + Minimum + ", Max: " + Maximum;
        }
    }

    /// <summary>
    /// Represents a range of floats
    /// </summary>
    [System.Serializable]
    public struct RangeOfFloats
    {
        private float? lastValue;
        private float? lastMinimum;
        private float? lastMaximum;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        public RangeOfFloats(float min, float max)
        {
            Minimum = min;
            Maximum = max;
            lastValue = lastMinimum = lastMaximum = null;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return "Min: " + lastMinimum + ", Max: " + lastMaximum + ", Last: " + lastValue;
        }

        /// <summary>
        /// The last value
        /// </summary>
        public float LastValue
        {
            get
            {
                if (lastValue == null || lastMinimum == null || lastMinimum.Value != Minimum || lastMaximum == null || lastMaximum.Value != Maximum)
                {
                    lastMinimum = Minimum;
                    lastMaximum = Maximum;
                    lastValue = Random();
                }
                return lastValue.Value;
            }
            set
            {
                lastMinimum = Minimum;
                lastMaximum = Maximum;
                lastValue = value;
            }
        }

        /// <summary>Minimum value (inclusive)</summary>
        [Tooltip("Minimum value (inclusive)")]
        public float Minimum;

        /// <summary>Maximum value (inclusive)</summary>
        [Tooltip("Maximum value (inclusive)")]
        public float Maximum;

        /// <summary>
        /// Generate a random value between min and max
        /// </summary>
        /// <returns>Random value</returns>
        public float Random() { return (LastValue = UnityEngine.Random.Range(Minimum, Maximum)); }

        /// <summary>
        /// Generate a random value between min and max using a specific random instance
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns>Random value</returns>
        public float Random(System.Random r) { return (LastValue = Minimum + ((float)r.NextDouble() * (Maximum - Minimum))); }

        /// <summary>
        /// Convert the min and max to Vector2
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 ToVector2() { return new Vector2(Minimum, Maximum); }
    }

    /// <summary>
    /// Single line attribute
    /// </summary>
    public class SingleLineAttribute : PropertyAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tooltip">Tooltip</param>
        public SingleLineAttribute(string tooltip) { Tooltip = tooltip; }

        /// <summary>
        /// Tooltip
        /// </summary>
        public string Tooltip { get; private set; }
    }

    /// <summary>
    /// Single line attribute with clamping
    /// </summary>
    public class SingleLineClampAttribute : SingleLineAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tooltip">Tooltip</param>
        /// <param name="minValue">Min value</param>
        /// <param name="maxValue">Max value</param>
        public SingleLineClampAttribute(string tooltip, float minValue, float maxValue) : base(tooltip)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <summary>
        /// Min value
        /// </summary>
        public float MinValue { get; private set; }

        /// <summary>
        /// Max value
        /// </summary>
        public float MaxValue { get; private set; }
    }

    /// <summary>
    /// Helper methods when serializing / deserializing fields with dynamic editor scripts
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Header byte for floats
        /// </summary>
        public const byte HeaderFloat = 0;

        /// <summary>
        /// Header byte for ints
        /// </summary>
        public const byte HeaderInt = 1;

        /// <summary>
        /// Header byte for shorts
        /// </summary>
        public const byte HeaderShort = 2;

        /// <summary>
        /// Header byte for bytes
        /// </summary>
        public const byte HeaderByte = 3;

        /// <summary>
        /// Header byte for colors
        /// </summary>
        public const byte HeaderColor = 4;

        /// <summary>
        /// Header byte for Vector2s
        /// </summary>
        public const byte HeaderVector2 = 5;

        /// <summary>
        /// Header byte for Vector3s
        /// </summary>
        public const byte HeaderVector3 = 6;

        /// <summary>
        /// Header byte for Vector4s
        /// </summary>
        public const byte HeaderVector4 = 7;

        /// <summary>
        /// Header byte for Quaternions
        /// </summary>
        public const byte HeaderQuaternion = 8;

        /// <summary>
        /// Header byte for Enums
        /// </summary>
        public const byte HeaderEnum = 9;

        /// <summary>
        /// Header byte for Bools
        /// </summary>
        public const byte HeaderBool = 10;

        /// <summary>
        /// Header byte for RangeofFloats
        /// </summary>
        public const byte HeaderFloatRange = 11;

        /// <summary>
        /// Header byte for RangeOfInts
        /// </summary>
        public const byte HeaderIntRange = 12;

        /// <summary>
        /// Convert a type to header byte
        /// </summary>
        public static readonly System.Collections.Generic.Dictionary<System.Type, byte> TypesToHeader = new System.Collections.Generic.Dictionary<System.Type, byte>
        {
            { typeof(float), HeaderFloat },
            { typeof(int), HeaderInt },
            { typeof(short), HeaderShort },
            { typeof(byte), HeaderByte },
            { typeof(Color), HeaderColor },
            { typeof(Vector2), HeaderVector2 },
            { typeof(Vector3), HeaderVector3 },
            { typeof(Vector4), HeaderVector4 },
            { typeof(Quaternion), HeaderQuaternion },
            { typeof(System.Enum), HeaderEnum },
            { typeof(bool), HeaderBool },
            { typeof(RangeOfFloats), HeaderFloatRange },
            { typeof(RangeOfIntegers), HeaderIntRange }
        };


        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Serialized bytes</returns>
        public static byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms, System.Text.Encoding.UTF8);
            System.Type t = obj.GetType();
            byte header;
            if (!TypesToHeader.TryGetValue(t, out header))
            {

#if NETFX_CORE

                if (!System.Reflection.IntrospectionExtensions.GetTypeInfo(t).IsEnum)

#else

                if (!t.IsEnum)

#endif

                {
                    return null;
                }
                header = HeaderEnum;
            }
            writer.Write(header);
            switch (header)
            {
                case HeaderFloat: { writer.Write((float)obj); break; }
                case HeaderInt: { writer.Write((int)obj); break; }
                case HeaderShort: { writer.Write((short)obj); break; }
                case HeaderByte: { writer.Write((byte)obj); break; }
                case HeaderColor: { Color c = (Color)obj; writer.Write(c.r); writer.Write(c.g); writer.Write(c.b); writer.Write(c.a); break; }
                case HeaderVector2: { Vector2 v = (Vector2)obj; writer.Write(v.x); writer.Write(v.y); break; }
                case HeaderVector3: { Vector3 v = (Vector3)obj; writer.Write(v.x); writer.Write(v.y); writer.Write(v.z); break; }
                case HeaderVector4: { Vector4 v = (Vector4)obj; writer.Write(v.x); writer.Write(v.y); writer.Write(v.z); writer.Write(v.w); break; }
                case HeaderQuaternion: { Quaternion q = (Quaternion)obj; writer.Write(q.x); writer.Write(q.y); writer.Write(q.z); writer.Write(q.w); break; }
                case HeaderBool: { writer.Write((bool)obj); break; }
                case HeaderFloatRange: { RangeOfFloats v = (RangeOfFloats)obj; writer.Write(v.Minimum); writer.Write(v.Maximum); break; }
                case HeaderIntRange: { RangeOfIntegers v = (RangeOfIntegers)obj; writer.Write(v.Minimum); writer.Write(v.Maximum); break; }
                case HeaderEnum: { writer.Write((int)obj); break; }
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="bytes">Bytes to deserialize</param>
        /// <param name="type">Type of object if known</param>
        /// <returns>Deserialized object</returns>
        public static object Deserialize(byte[] bytes, System.Type type = null)
        {
            if (bytes == null || bytes.Length < 2)
            {
                return null;
            }
            MemoryStream ms = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(ms, System.Text.Encoding.UTF8);
            switch (reader.ReadByte())
            {
                case HeaderFloat: return reader.ReadSingle();
                case HeaderInt: return reader.ReadInt32();
                case HeaderShort: return reader.ReadInt16();
                case HeaderByte: return reader.ReadByte();
                case HeaderColor: return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderVector2: return new Vector2(reader.ReadSingle(), reader.ReadSingle());
                case HeaderVector3: return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderVector4: return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderQuaternion: return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderBool: return reader.ReadBoolean();
                case HeaderFloatRange: return new RangeOfFloats { Minimum = reader.ReadSingle(), Maximum = reader.ReadSingle() };
                case HeaderIntRange: return new RangeOfIntegers { Minimum = reader.ReadInt32(), Maximum = reader.ReadInt32() };
                case HeaderEnum: return (type == null ? reader.ReadInt32() : System.Enum.ToObject(type, reader.ReadInt32()));
                default: return null;
            }
        }
    }

    /// <summary>
    /// Enum flags attribute, allows drawing a flags property drawer
    /// </summary>
    public class EnumFlagAttribute : PropertyAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tooltip">Tooltip</param>
        public EnumFlagAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }

        /// <summary>
        /// Tooltip
        /// </summary>
        public string Tooltip { get; private set; }
    }

    /// <summary>
    /// Read only label attribute
    /// </summary>
    public class ReadOnlyLabelAttribute : PropertyAttribute { }

    /// <summary>
    /// Help box message type
    /// </summary>
    public enum HelpBoxMessageType
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Info
        /// </summary>
        Info,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Error
        /// </summary>
        Error
    }

    /// <summary>
    /// Help box attribute, use to turn field into a help box property drawer
    /// </summary>
    public class HelpBoxAttribute : PropertyAttribute
    {
        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Message type
        /// </summary>
        public HelpBoxMessageType MessageType { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="messageType">Message type</param>
        public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None)
        {
            Text = text;
            MessageType = messageType;
        }
    }

    /// <summary>
    /// Min max slider attribute, used to turn a field into a min/max slider
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        /// <summary>
        /// Max value
        /// </summary>
        public float Max { get; private set; }

        /// <summary>
        /// Max value
        /// </summary>
        public float Min { get; private set; }

        /// <summary>
        /// Tooltip
        /// </summary>
        public string Tooltip { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="tooltip">Tooltip</param>
        public MinMaxSliderAttribute(float min, float max, string tooltip)
        {
            Min = min;
            Max = max;
            Tooltip = tooltip;
        }
    }

}
