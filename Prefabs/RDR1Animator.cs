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
        public Dictionary<uint, Vector4> AnimValues = new();
        public bool EnableRootMotion = true;

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            Skeleton = Target?.Skeleton as Rsc6SkeletonData;
            if (Skeleton == null) return;

            if (Clip is Rsc6ClipSingle sclip)
                UpdateClip(sclip);
            else if (Clip is Rsc6ClipMulti mclip)
                UpdateClip(mclip);
            else if (Anim != null)
                UpdateAnim(Anim, (float)CurrentTime);

            UpdateSkeleton();

            Skeleton.UpdateBoneTransforms();
            Target?.Piece?.UpdateRigs();
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

            if (boneids == null) return;
            var f = anim.GetFramePosition(t);

            for (int i = 0; i < boneids.Length; i++)
            {
                ref var boneid = ref boneids[i];
                var key = boneid.Packed;

                var v = anim.Evaluate(f, i);
                v = new Vector4(v.Z, v.X, v.Y, v.W);

                if (boneid.TrackType == Rsc6TrackType.Quaternion) //Quaternion - needs to be normalized
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
                if (boneid.TypeId == 0xFF) continue; //This is a UV channel - will be handled later
                if (bonemap.TryGetValue(boneid.BoneId, out var bone) == false) continue;
                if (bone == null) continue;

                var v = kvp.Value;
                switch (boneid.TrackId)
                {
                    case 0: //Bone translation
                    case 3: //Mover translation
                        bone.AnimTranslation = v.XYZ();
                        break;
                    case 1: //Bone rotation
                    case 4: //Mover rotation
                        bone.AnimRotation = v.ToQuaternion();
                        break;
                    case 2: //Bone scale
                        bone.AnimScale = v.XYZ();
                        break;
                    case 5: //Root motion vector
                        if (EnableRootMotion) bone.AnimTranslation += v.XYZ(); //Let's really hope a bone position track came first
                        break;
                    case 6: //Quaternion... root rotation
                        if (EnableRootMotion) bone.AnimRotation = v.ToQuaternion() * bone.AnimRotation; //As above
                        break;
                    default:
                        break;
                }
            }
        }
    }
}