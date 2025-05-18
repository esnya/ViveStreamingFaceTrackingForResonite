using System;
using Elements.Core;
using FrooxEngine;
using ViveStreamingFaceTrackingModule;

namespace ViveStreamingFaceTrackingForResonite
{
    internal sealed class ViveStreamingMouth : Mouth
    {
        private readonly struct MouthData
        {
            private readonly float[] data;

            public MouthData()
            {
                data = new float[(int)FaceData.LipDataIndex.Max];
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = float.NaN;
                }
            }

            public readonly float this[FaceData.LipDataIndex index] => data[(int)index];

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

        private readonly MouthData mouthData = new();

        public ViveStreamingMouth(InputInterface input) : base(input, "Vive Streaming Lip Tracking", new[]
        {
                MouthParameterGroup.JawPose,
                MouthParameterGroup.JawOpen,
                MouthParameterGroup.TonguePose,
                MouthParameterGroup.TongueRoll,
                MouthParameterGroup.LipRaise,
                MouthParameterGroup.LipHorizontal,
                MouthParameterGroup.SmileFrown,
                MouthParameterGroup.MouthPout,
                MouthParameterGroup.LipOverturn,
                MouthParameterGroup.LipOverUnder,
                MouthParameterGroup.CheekPuffSuck
        })
        {
        }

        public void UpdateInputs(bool connected, ref string? newMouthData)
        {
            IsDeviceActive = connected;
            IsTracking = connected && Input.VR_Active;

            if (newMouthData is null)
            {
                return;
            }

            mouthData.Update(newMouthData);
            newMouthData = null;

            UpdateJaw();
            UpdateLipParameters();
            UpdateCheekParameters();
            UpdateTongueParameters();
        }

        private void UpdateJaw()
        {
            var v = new float3(
                mouthData[FaceData.LipDataIndex.Jaw_Right] - mouthData[FaceData.LipDataIndex.Jaw_Left],
                -mouthData[FaceData.LipDataIndex.Mouth_Ape_Shape],
                mouthData[FaceData.LipDataIndex.Jaw_Forward]
            );
            if (!v.IsNaN)
            {
                Jaw = v;
            }

            var f = mouthData[FaceData.LipDataIndex.Jaw_Open];
            if (!float.IsNaN(f))
            {
                JawOpen = f;
            }
        }

        private void UpdateLipParameters()
        {
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Upper_Left, value => LipUpperLeftRaise = value);
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Upper_Right, value => LipUpperRightRaise = value);
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Lower_Left, value => LipLowerLeftRaise = value);
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Lower_Right, value => LipLowerRightRaise = value);

            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Upper_Right, FaceData.LipDataIndex.Mouth_Upper_Left, value => LipUpperHorizontal = value);
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Lower_Right, FaceData.LipDataIndex.Mouth_Lower_Left, value => LipLowerHorizontal = value);

            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Smile_Left, FaceData.LipDataIndex.Mouth_Sad_Left, value => MouthLeftSmileFrown = value);
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Smile_Right, FaceData.LipDataIndex.Mouth_Sad_Right, value => MouthRightSmileFrown = value);

            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Pout, value => MouthPoutLeft = MouthPoutRight = value);

            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Upper_Overturn, value => LipTopLeftOverturn = LipTopRightOverturn = value);
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Lower_Overturn, value => LipBottomLeftOverturn = LipBottomRightOverturn = value);

            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Upper_Inside, value => LipTopLeftOverUnder = LipTopRightOverUnder = -value);
            UpdateLipParameter(FaceData.LipDataIndex.Mouth_Lower_Inside, FaceData.LipDataIndex.Mouth_Lower_Overlay, value => LipBottomLeftOverUnder = LipBottomRightOverUnder = value);
        }

        private void UpdateCheekParameters()
        {
            UpdateLipParameter(FaceData.LipDataIndex.Cheek_Puff_Left, value => CheekLeftPuffSuck = value);
            UpdateLipParameter(FaceData.LipDataIndex.Cheek_Puff_Right, value => CheekRightPuffSuck = value);

            var f = mouthData[FaceData.LipDataIndex.Cheek_Suck];
            if (!float.IsNaN(f))
            {
                CheekLeftPuffSuck -= f;
                CheekRightPuffSuck -= f;
            }
        }

        private void UpdateTongueParameters()
        {
            var v = new float3(
                mouthData[FaceData.LipDataIndex.Tongue_Right] - mouthData[FaceData.LipDataIndex.Tongue_Left],
                mouthData[FaceData.LipDataIndex.Tongue_Up] - mouthData[FaceData.LipDataIndex.Tongue_Down],
                (mouthData[FaceData.LipDataIndex.Tongue_Longstep1] + mouthData[FaceData.LipDataIndex.Tongue_Longstep2]) * 0.5f
            );
            if (!v.IsNaN)
            {
                Tongue = v;
            }

            var f = mouthData[FaceData.LipDataIndex.Tongue_Roll];
            if (!float.IsNaN(f))
            {
                TongueRoll = f;
            }
        }

        private void UpdateLipParameter(FaceData.LipDataIndex index, Action<float> updateAction)
        {
            var value = mouthData[index];
            if (!float.IsNaN(value))
            {
                updateAction(value);
            }
        }

        private void UpdateLipParameter(FaceData.LipDataIndex index, FaceData.LipDataIndex subtractIndex, Action<float> updateAction)
        {
            var value = mouthData[index] - mouthData[subtractIndex];
            if (!float.IsNaN(value))
            {
                updateAction(value);
            }
        }
    }
}
