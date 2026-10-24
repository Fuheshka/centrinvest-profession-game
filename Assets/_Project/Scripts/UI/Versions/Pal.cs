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
        public static readonly Color Text = new Color32(0xEC, 0xED, 0xEF, 255);
        public static readonly Color Dim = new Color32(0xA9, 0xAD, 0xB8, 255);
        public static readonly Color Panel = new Color32(0x1D, 0x20, 0x28, 255);
        public static readonly Color Panel2 = new Color32(0x25, 0x28, 0x33, 255);
        public static readonly Color Accent = new Color32(0xFF, 0x8A, 0x4C, 255);
        public static readonly Color Green = new Color32(0x3D, 0xBE, 0x86, 255);
        public static readonly Color Red = new Color32(0xFF, 0x5A, 0x5F, 255);
        public static readonly Color Yellow = new Color32(0xF2, 0xC9, 0x4C, 255);
        public static readonly Color GreenTint = new Color32(0x1E, 0x35, 0x2C, 255);
        public static readonly Color AccentTint = new Color32(0x4A, 0x32, 0x24, 255);
    }
}
