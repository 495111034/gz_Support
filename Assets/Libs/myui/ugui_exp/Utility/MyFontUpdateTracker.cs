using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    public static class MyFontUpdateTracker
    {
        static Dictionary<Font, List<MyText>> m_Tracked = new Dictionary<Font, List<MyText>>();

        public static void TrackText(MyText t)
        {
            if (t.font == null)
                return;

            List<MyText> exists;
            m_Tracked.TryGetValue(t.font, out exists);
            if (exists == null)
            {
                // The textureRebuilt event is global for all fonts, so we add our delegate the first time we register *any* Text
                if (m_Tracked.Count == 0)
                    Font.textureRebuilt += RebuildForFont;

                exists = new List<MyText>();
                m_Tracked.Add(t.font, exists);
            }

            if (!exists.Contains(t))
                exists.Add(t);
        }

        private static void RebuildForFont(Font f)
        {
            List<MyText> texts;
            m_Tracked.TryGetValue(f, out texts);

            if (texts == null)
                return;

            for (var i = 0; i < texts.Count; i++)
                texts[i].FontTextureChanged();
        }

        public static void UntrackText(MyText t)
        {
            if (t.font == null)
                return;

            List<MyText> texts;
            m_Tracked.TryGetValue(t.font, out texts);

            if (texts == null)
                return;

            texts.Remove(t);

            if (texts.Count == 0)
            {
                m_Tracked.Remove(t.font);

                // There is a global textureRebuilt event for all fonts, so once the last Text reference goes away, remove our delegate
                if (m_Tracked.Count == 0)
                    Font.textureRebuilt -= RebuildForFont;
            }
        }
    }
}
