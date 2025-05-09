using System;
using Elements.Core;
using FrooxEngine;
using ViveStreamingFaceTrackingModule;

namespace ViveStreamingFaceTrackingForResonite
{
    internal sealed class ViveStreamingEyes : Eyes
    {
        private readonly struct EyeData
        {
            private readonly float[] data;

            public EyeData()
            {
                data = new float[(int)FaceData.EyeDataIndex.MAX];
                data.SetValue(float.NaN, 0);
            }

            public float this[FaceData.EyeDataIndex index] => data[(int)index];

            public void Update(string status)
            {
                var parts = status.Split(',');
                for (int i = 0; i < parts.Length && i < data.Length; i++)
                {
                    if (float.TryParse(parts[i], out float value))
                    {
                        data[i] = value;
                    }
                    else
                    {
                        data[i] = float.NaN;
                    }
                }
            }
        }

        private float _timeSinceLastValidEyeData = float.MaxValue;
        private readonly EyeData eyeData = new();

        public ViveStreamingEyes(InputInterface input) : base(input, "Vive Streaming Eye Tracking", true)
        {
        }

        public void UpdateInputs(bool connected, ref string? newData, float deltaTime)
        {
            //IsDeviceActive = LeftEye.IsDeviceActive = RightEye.IsDeviceActive = CombinedEye.IsDeviceActive = connected;
            IsEyeTrackingActive = connected && Input.VR_Active;

            _timeSinceLastValidEyeData += deltaTime;

            if (newData is null)
            {
                if (_timeSinceLastValidEyeData > 0.2f)
                {
                    SetTracking(false);
                }

                return;
            }

            SetTracking(true);
            _timeSinceLastValidEyeData = 0f;

            eyeData.Update(newData);
            newData = null;

            UpdateEyeDirection(LeftEye, FaceData.EyeDataIndex.LEFT_EYE_DIRECTION_X, FaceData.EyeDataIndex.LEFT_EYE_DIRECTION_Y, FaceData.EyeDataIndex.LEFT_EYE_DIRECTION_Z);
            UpdateEyeDirection(RightEye, FaceData.EyeDataIndex.RIGHT_EYE_DIRECTION_X, FaceData.EyeDataIndex.RIGHT_EYE_DIRECTION_Y, FaceData.EyeDataIndex.RIGHT_EYE_DIRECTION_Z);
            UpdateEyeDirection(CombinedEye, FaceData.EyeDataIndex.COMBINE_EYE_DIRECTION_X, FaceData.EyeDataIndex.COMBINE_EYE_DIRECTION_Y, FaceData.EyeDataIndex.COMBINE_EYE_DIRECTION_Z);

            UpdateEyeParameter(LeftEye, FaceData.EyeDataIndex.LEFT_EYE_OPENNESS, FaceData.EyeDataIndex.LEFT_BLINK, (eye, value) => eye.Openness = value);
            UpdateEyeParameter(RightEye, FaceData.EyeDataIndex.RIGHT_EYE_OPENNESS, FaceData.EyeDataIndex.RIGHT_BLINK, (eye, value) => eye.Openness = value);

            UpdateEyeParameter(LeftEye, FaceData.EyeDataIndex.LEFT_WIDE, (eye, value) => eye.Widen = value);
            UpdateEyeParameter(RightEye, FaceData.EyeDataIndex.RIGHT_WIDE, (eye, value) => eye.Widen = value);

            UpdateEyeParameter(LeftEye, FaceData.EyeDataIndex.LEFT_SQUEEZE, (eye, value) => eye.Squeeze = value);
            UpdateEyeParameter(RightEye, FaceData.EyeDataIndex.RIGHT_SQUEEZE, (eye, value) => eye.Squeeze = value);

            UpdateEyeParameter(LeftEye, FaceData.EyeDataIndex.LEFT_PUPIL_DIAMETER, (eye, value) => eye.PupilDiameter = value * 0.001f);
            UpdateEyeParameter(RightEye, FaceData.EyeDataIndex.RIGHT_PUPIL_DIAMETER, (eye, value) => eye.PupilDiameter = value * 0.001f);

            var timestamp = eyeData[FaceData.EyeDataIndex.TIMESTAMP];
            if (!float.IsNaN(timestamp))
            {
                Timestamp = timestamp * 0.001f;
            }

            CombinedEye.Openness = (LeftEye.Openness + RightEye.Openness) * 0.5f;
            CombinedEye.PupilDiameter = (LeftEye.PupilDiameter + RightEye.PupilDiameter) * 0.5f;
            ComputeCombinedEyeParameters();
            FinishUpdate();
        }

        private void UpdateEyeDirection(Eye eye, FaceData.EyeDataIndex xIndex, FaceData.EyeDataIndex yIndex, FaceData.EyeDataIndex zIndex)
        {
            var direction = new float3(
                eyeData[xIndex],
                eyeData[yIndex],
                -eyeData[zIndex]
            ).Normalized;

            if (!direction.IsNaN && !direction.Approximately(float3.Zero, 0.001f))
            {
                eye.UpdateWithDirection(direction);
            }
        }

        private void UpdateEyeParameter(Eye eye, FaceData.EyeDataIndex index, Action<Eye, float> updateAction)
        {
            var value = eyeData[index];
            if (!float.IsNaN(value))
            {
                updateAction(eye, value);
            }
        }

        private void UpdateEyeParameter(Eye eye, FaceData.EyeDataIndex index, FaceData.EyeDataIndex subtractIndex, Action<Eye, float> updateAction)
        {
            var value = eyeData[index] - eyeData[subtractIndex];
            if (!float.IsNaN(value))
            {
                updateAction(eye, MathX.Clamp01(value));
            }
        }
    }
}
