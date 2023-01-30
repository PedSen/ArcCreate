using System.Collections.Generic;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public class EditorBeatlineGenerator : IBeatlineGenerator
    {
        private readonly List<BeatlineColorSetting> beatlineColorSettings;
        private readonly Color defaultColor;

        public EditorBeatlineGenerator(List<BeatlineColorSetting> beatlineColorSettings, Color defaultColor)
        {
            this.beatlineColorSettings = beatlineColorSettings;
            beatlineColorSettings.Sort((a, b) => a.Density.CompareTo(b.Density));
            this.defaultColor = defaultColor;
        }

        public IEnumerable<Beatline> Generate(TimingGroup tg)
        {
            List<TimingEvent> timings = tg.Timings;
            for (int i = 0; i < timings.Count - 1; i++)
            {
                TimingEvent currentTiming = timings[i];
                int limit = timings[i + 1].Timing;

                float distanceBetweenTwoLine =
                    currentTiming.Bpm == 0 ?
                    float.MaxValue :
                    60000f / Mathf.Abs(currentTiming.Bpm) / Values.BeatlineDensity.Value;

                if (distanceBetweenTwoLine <= 0)
                {
                    continue;
                }

                int count = 0;
                for (float timing = currentTiming.Timing; timing < limit; timing += distanceBetweenTwoLine)
                {
                    Color beatlineColor = ResolveColor(count, Values.BeatlineDensity.Value);
                    yield return new Beatline(
                        tg.GetFloorPosition(Mathf.RoundToInt(timing)),
                        Values.EditorBeatlineThickness,
                        beatlineColor);
                    count++;
                }
            }

            // Last timing event extend until end of audio
            {
                TimingEvent lastTiming = timings[timings.Count - 1];
                int limit = Services.Gameplay.Audio.AudioLength;

                float distanceBetweenTwoLine =
                    lastTiming.Bpm == 0 ?
                    float.MaxValue :
                    60000f / Mathf.Abs(lastTiming.Bpm) * lastTiming.Divisor;

                if (distanceBetweenTwoLine > 0)
                {
                    int count = 0;
                    for (float timing = lastTiming.Timing; timing < limit; timing += distanceBetweenTwoLine)
                    {
                        Color beatlineColor = ResolveColor(count, Values.BeatlineDensity.Value);
                        yield return new Beatline(
                            tg.GetFloorPosition(Mathf.RoundToInt(timing)),
                            Values.EditorBeatlineThickness,
                            beatlineColor);
                        count++;
                    }
                }
            }
        }

        private Color ResolveColor(int count, float beatlineDensity)
        {
            foreach (BeatlineColorSetting setting in beatlineColorSettings)
            {
                float density = setting.Density;
                Color color = setting.Color;

                if (count % (beatlineDensity / density) == 0)
                {
                    return color;
                }
            }

            return defaultColor;
        }
    }
}