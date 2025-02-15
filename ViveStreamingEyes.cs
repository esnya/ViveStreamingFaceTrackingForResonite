using Elements.Core;
using FrooxEngine;
using ViveStreamingFaceTrackingModule;

namespace ViveStreamingFaceTrackingForResonite
{
    internal sealed class ViveStreamingEyes : Eyes
    {
        private string? newData;

        private readonly struct EyeData
        {
            private readonly float[] data;

            public EyeData()
            {
                data = new float[(int)FaceData.EyeDataIndex.MAX];
                data.SetValue(float.NaN, 0);
            }

            public float this[FaceData.EyeDataIndex index] => data[(int)index];

            public bool Update(string status)
            {
                var active = false;
                var parts = status.Split(',');
                for (int i = 0; i < parts.Length && i < data.Length; i++)
                {
                    if (float.TryParse(parts[i], out float value))
                    {
                        data[i] = value;
                        active = true;
                    }
                    else
                    {
                        data[i] = float.NaN;
                    }
                }
                return active;
            }
        }

        private readonly EyeData eyeData = new();

        public ViveStreamingEyes(InputInterface input, string name, bool supportsPupilTracking) : base(input, name, supportsPupilTracking)
        {
        }

        public void UpdateStatus(string value)
        {
            newData = value;
        }

        public void UpdateInputs()
        {
            SetTracking(LeftEye.IsDeviceActive = RightEye.IsDeviceActive = CombinedEye.IsDeviceActive = IsDeviceActive);

            if (newData is null)
            {
                return;
            }
            eyeData.Update(newData);
            newData = null;

            var v = new float3(
                eyeData[FaceData.EyeDataIndex.LEFT_EYE_DIRECTION_X],
                eyeData[FaceData.EyeDataIndex.LEFT_EYE_DIRECTION_Y],
                eyeData[FaceData.EyeDataIndex.LEFT_EYE_DIRECTION_Z]
            ).Normalized;
            if (!v.IsNaN && !v.Approximately(float3.Zero, 0.001f))
            {
                LeftEye.UpdateWithDirection(v);
            }

            v = new float3(
                eyeData[FaceData.EyeDataIndex.RIGHT_EYE_DIRECTION_X],
                eyeData[FaceData.EyeDataIndex.RIGHT_EYE_DIRECTION_Y],
                eyeData[FaceData.EyeDataIndex.RIGHT_EYE_DIRECTION_Z]
            ).Normalized;
            if (!v.IsNaN && !v.Approximately(float3.Zero, 0.001f))
            {
                RightEye.UpdateWithDirection(v);
            }

            v = new float3(
                eyeData[FaceData.EyeDataIndex.COMBINE_EYE_DIRECTION_X],
                eyeData[FaceData.EyeDataIndex.COMBINE_EYE_DIRECTION_Y],
                eyeData[FaceData.EyeDataIndex.COMBINE_EYE_DIRECTION_Z]
            ).Normalized;
            if (!v.IsNaN && !v.Approximately(float3.Zero, 0.001f))
            {
                CombinedEye.UpdateWithDirection(v);
            }

            var f = eyeData[FaceData.EyeDataIndex.LEFT_EYE_OPENNESS] - eyeData[FaceData.EyeDataIndex.LEFT_BLINK];
            if (!float.IsNaN(f))
            {
                LeftEye.Openness = MathX.Clamp01(f);
            }

            f = eyeData[FaceData.EyeDataIndex.RIGHT_EYE_OPENNESS] - eyeData[FaceData.EyeDataIndex.RIGHT_BLINK];
            if (!float.IsNaN(f))
            {
                RightEye.Openness = MathX.Clamp01(f);
            }

            f = eyeData[FaceData.EyeDataIndex.LEFT_WIDE];
            if (!float.IsNaN(f))
            {
                LeftEye.Widen = f;
            }

            f = eyeData[FaceData.EyeDataIndex.RIGHT_WIDE];
            if (!float.IsNaN(f))
            {
                RightEye.Widen = f;
            }

            f = eyeData[FaceData.EyeDataIndex.LEFT_SQUEEZE];
            if (!float.IsNaN(f))
            {
                LeftEye.Squeeze = f;
            }

            f = eyeData[FaceData.EyeDataIndex.RIGHT_SQUEEZE];
            if (!float.IsNaN(f))
            {
                RightEye.Squeeze = f;
            }

            f = eyeData[FaceData.EyeDataIndex.LEFT_PUPIL_DIAMETER];
            if (!float.IsNaN(f))
            {
                LeftEye.PupilDiameter = f * 0.001f;
            }

            f = eyeData[FaceData.EyeDataIndex.RIGHT_PUPIL_DIAMETER];
            if (!float.IsNaN(f))
            {
                RightEye.PupilDiameter = f * 0.001f;
            }

            f = eyeData[FaceData.EyeDataIndex.TIMESTAMP];
            if (!float.IsNaN(f))
            {
                Timestamp = f * 0.001f;
            }

            CombinedEye.Openness = (LeftEye.Openness + RightEye.Openness) * 0.5f;
            CombinedEye.PupilDiameter = (LeftEye.PupilDiameter + RightEye.PupilDiameter) * 0.5f;
            ComputeCombinedEyeParameters();
            FinishUpdate();
        }
    }
}
