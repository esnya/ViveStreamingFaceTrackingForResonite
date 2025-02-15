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
                data.SetValue(float.NaN, 0);
            }

            public readonly float this[FaceData.LipDataIndex index] => data[(int)index];

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


        private string? newMouthData;
        private readonly MouthData mouthData = new();

        public ViveStreamingMouth(InputInterface input, string name) : base(input, name, new[]
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

        public void UpdateStatus(string value)
        {
            newMouthData = value;
        }

        public void UpdateInputs()
        {
            if (newMouthData is null)
            {
                return;
            }

            IsTracking = mouthData.Update(newMouthData);
            newMouthData = null;

            if (!IsTracking)
            {
                return;
            }

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

            f = mouthData[FaceData.LipDataIndex.Mouth_Upper_Left];
            if (!float.IsNaN(f))
            {
                LipUpperLeftRaise = f;
            }
            f = mouthData[FaceData.LipDataIndex.Mouth_Upper_Right];
            if (!float.IsNaN(f))
            {
                LipUpperRightRaise = f;
            }
            f = mouthData[FaceData.LipDataIndex.Mouth_Lower_Left];
            if (!float.IsNaN(f))
            {
                LipLowerLeftRaise = f;
            }
            f = mouthData[FaceData.LipDataIndex.Mouth_Lower_Right];
            if (!float.IsNaN(f))
            {
                LipLowerRightRaise = f;
            }

            f = mouthData[FaceData.LipDataIndex.Mouth_Upper_Right] - mouthData[FaceData.LipDataIndex.Mouth_Upper_Left];
            if (!float.IsNaN(f))
            {
                LipUpperHorizontal = f;
            }
            f = mouthData[FaceData.LipDataIndex.Mouth_Lower_Right] - mouthData[FaceData.LipDataIndex.Mouth_Lower_Left];
            if (!float.IsNaN(f))
            {
                LipLowerHorizontal = f;
            }

            f = mouthData[FaceData.LipDataIndex.Mouth_Smile_Left] - mouthData[FaceData.LipDataIndex.Mouth_Sad_Left];
            if (!float.IsNaN(f))
            {
                MouthLeftSmileFrown = f;
            }
            f = mouthData[FaceData.LipDataIndex.Mouth_Smile_Right] - mouthData[FaceData.LipDataIndex.Mouth_Sad_Right];
            if (!float.IsNaN(f))
            {
                MouthRightSmileFrown = f;
            }

            f = mouthData[FaceData.LipDataIndex.Mouth_Pout];
            if (!float.IsNaN(f))
            {
                MouthPoutLeft = MouthPoutRight = f;
            }

            f = mouthData[FaceData.LipDataIndex.Mouth_Upper_Overturn];
            if (!float.IsNaN(f))
            {
                LipTopLeftOverturn = LipTopRightOverturn = f;
            }
            f = mouthData[FaceData.LipDataIndex.Mouth_Lower_Overturn];
            if (!float.IsNaN(f))
            {
                LipBottomLeftOverturn = LipBottomRightOverturn = f;
            }

            f = mouthData[FaceData.LipDataIndex.Mouth_Upper_Inside];
            if (!float.IsNaN(f))
            {
                LipTopLeftOverUnder = LipTopRightOverUnder = -f;
            }

            f = mouthData[FaceData.LipDataIndex.Mouth_Lower_Inside] - mouthData[FaceData.LipDataIndex.Mouth_Lower_Overlay];
            if (!float.IsNaN(f))
            {
                LipBottomLeftOverUnder = LipBottomRightOverUnder = f;
            }

            f = mouthData[FaceData.LipDataIndex.Cheek_Puff_Left];
            if (!float.IsNaN(f))
            {
                CheekLeftPuffSuck = f;
            }
            f = mouthData[FaceData.LipDataIndex.Cheek_Puff_Right];
            if (!float.IsNaN(f))
            {
                CheekRightPuffSuck = f;
            }
            f = mouthData[FaceData.LipDataIndex.Cheek_Suck];
            if (!float.IsNaN(f))
            {
                CheekLeftPuffSuck -= f;
                CheekRightPuffSuck -= f;
            }

            v = new float3(
                mouthData[FaceData.LipDataIndex.Tongue_Right] - mouthData[FaceData.LipDataIndex.Tongue_Left],
                mouthData[FaceData.LipDataIndex.Tongue_Up] - mouthData[FaceData.LipDataIndex.Tongue_Down],
                (mouthData[FaceData.LipDataIndex.Tongue_Longstep1] + mouthData[FaceData.LipDataIndex.Tongue_Longstep2]) * 0.5f
            );
            if (!v.IsNaN)
            {
                Tongue = v;
            }

            f = mouthData[FaceData.LipDataIndex.Tongue_Roll];
            if (!float.IsNaN(f))
            {
                TongueRoll = f;
            }
        }
    }
}
