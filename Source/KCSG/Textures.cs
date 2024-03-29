﻿using UnityEngine;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    internal static class Textures
    {
        public static readonly Texture2D helpIcon = ContentFinder<Texture2D>.Get("UI/CSG/help");
    }
}