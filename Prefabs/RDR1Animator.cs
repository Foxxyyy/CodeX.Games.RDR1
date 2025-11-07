using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Games.RDR1.RSC6;
using System.Collections.Generic;
using System.Numerics;

namespace CodeX.Games.RDR1.Prefabs
{
    public class RDR1Animator : Animator
    {
        public Rsc6Clip Clip; //Clip to play
        public Rsc6Animation Anim; //For use when playing a specific anim. (when Clip==null)
        public Rsc6SkeletonData Skeleton;//Skeleton to animate
        public Dictionary<uint, Vector4> AnimValues = [];
        public bool EnableRootMotion = false;
        public string ClipName;
        public string AnimName;

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            Skeleton = Target?.Skeleton as Rsc6SkeletonData;
            if (Skeleton == null) return;

            if (Clip is Rsc6ClipSingle sclip)
            {
                if (ClipName != sclip.Name)
                {
                    AnimValues = [];
                    ClipName = sclip.Name;
                }
                UpdateClip(sclip);
            }
            else if (Clip is Rsc6ClipMulti mclip)
            {
                UpdateClip(mclip);
            }
            else if (Anim != null)
            {
                if (AnimName != Anim.RefactoredName)
                {
                    AnimValues = [];
                    AnimName = Anim.RefactoredName;
                }
                UpdateAnim(Anim, (float)CurrentTime);
            }

            UpdateSkeleton();

            Skeleton.AnimateRenderables = true;
            Skeleton.UpdateBoneTransforms();
        }

        private void UpdateClip(Rsc6ClipSingle clip, int uvIndex = -1)
        {
            var anim = clip.AnimationRef.Item;
            if (anim == null) return;
            var t = clip.GetPlaybackTime(CurrentTime);
            UpdateAnim(anim, t, uvIndex);
        }

        private void UpdateClip(Rsc6ClipMulti clip, int uvIndex = -1)
        {
            return;
        }

        private void UpdateAnim(Rsc6Animation anim, float t, int uvIndex = -1)
        {
            if (anim == null) return;
            var boneids = anim.BoneIds.ToArray();
            var f = anim.GetFramePosition(t);

            for (int i = 0; i < boneids.Length; i++)
            {
                var boneid = boneids[i];
                var key = boneid.Packed;

                var v = anim.Evaluate(f, i);
                v = new Vector4(v.Z, v.X, v.Y, v.W);

                if ((boneid.TypeId & 0x3) == (byte)Rsc6TrackType.QUATERNION) //Quaternion - needs to be normalized
                {
                    v = Vector4.Normalize(v);
                }
                AnimValues[key] = v;
            }
        }

        private void UpdateSkeleton()
        {
            var bonemap = Skeleton?.BonesMap;
            if (bonemap == null) return;

            foreach (var kvp in AnimValues)
            {
                var boneid = new Rsc6AnimBoneId(kvp.Key);
                if ((boneid.TypeId & 0x3) == 0xFF) continue; //This is a UV channel - will be handled later
                if (bonemap.TryGetValue(boneid.ID, out var bone) == false) continue;
                if (bone == null) continue;

                var v = kvp.Value;
                switch (boneid.TrackId)
                {
                    case Rsc6TrackID.BONE_TRANSLATION:
                        bone.AnimTranslation = v.XYZ();
                        break;

                    case Rsc6TrackID.BONE_ROTATION:
                        bone.AnimRotation = v.ToQuaternion();
                        break;

                    case Rsc6TrackID.BONE_SCALE:
                        bone.AnimScale = v.XYZ();
                        break;

                    case Rsc6TrackID.MOVER_TRANSLATION:
                        if (EnableRootMotion) bone.AnimTranslation += v.XYZ();
                        break;

                    case Rsc6TrackID.MOVER_ROTATION:
                        if (EnableRootMotion) bone.AnimRotation = v.ToQuaternion() * bone.AnimRotation;
                        break;

                    case Rsc6TrackID.MOVER_SCALE:
                        if (EnableRootMotion) bone.AnimScale += v.XYZ();
                        break;

                    default:
                        break;
                }
            }
        }
    }
}