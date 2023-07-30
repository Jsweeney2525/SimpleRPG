using System;
using System.Collections.Generic;

namespace SimpleRPG.Screens
{
    public class ColorString
    {
        public ConsoleColor Color { get; protected set; }

        public string Value { get; protected set; }

        public List<ColorString> SubStrings { get; protected set; }

        public ColorString(string value, ConsoleColor? color = null)
        {
            Color = color ?? Globals.DefaultColor;
            Value = value;
            SubStrings = new List<ColorString>();
        }

        public ColorString(params ColorString[] subStrings) : this(null as string)
        {
            foreach (ColorString subString in subStrings)
            {
                SubStrings.Add(subString);
            }
        }

        public static implicit operator ColorString(string value)
        {
            return value == null ? null : new ColorString(value);
        }

        public string GetFullString()
        {
            string ret = Value ?? "";

            foreach (ColorString subString in SubStrings)
            {
                ret += subString.GetFullString();
            }

            return ret;
        }
    }
}
