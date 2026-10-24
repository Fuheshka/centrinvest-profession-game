using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // State colors for the Versions tab and ending (look of the rows themselves lives in the prefabs).
    public static class Pal
    {
        public static readonly Color Text = new Color32(0x26, 0x09, 0x00, 255);
        public static readonly Color Dim = new Color32(0x64, 0x72, 0x3D, 255);
        public static readonly Color Panel = new Color32(0xED, 0xF4, 0xE4, 255);
        public static readonly Color Panel2 = new Color32(0xED, 0xF4, 0xE4, 255);
        public static readonly Color Accent = new Color32(0x78, 0x67, 0x10, 255);
        public static readonly Color Green = new Color32(0x34, 0x55, 0x16, 255);
        public static readonly Color Red = new Color32(0xA3, 0x30, 0x21, 255);
        public static readonly Color Yellow = new Color32(0x78, 0x67, 0x10, 255);
        public static readonly Color GreenTint = new Color32(0xD8, 0xE2, 0xA6, 255);
        public static readonly Color AccentTint = new Color32(0xE9, 0xD6, 0x6B, 255);
    }
}
