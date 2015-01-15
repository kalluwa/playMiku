using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace PmdModelLib
{
    //a frame contains rotation and movement
    public struct VmdKeyframe
    {
        public float Time;
        public Vector3 Position;
        public Quaternion Rotation;
    };

    /// <summary>
    /// frame of face
    /// </summary>
    public struct VmdFaceFrame
    {
        //frame index 2 time
        public string MorphName;
        public float IndexOfFrame;//they ared sorted
        public float WeightOfBaseVertex;//calculate final position
    }
    /// <summary>
    /// we use this class to store animation data
    /// </summary>
    public class VmdAnimation
    {
        /// <summary>
        /// Bones count
        /// </summary>
        public int BonesCount;
        /// <summary>
        /// we use this to store keyframe
        /// </summary>
        public Dictionary<string,List<VmdKeyframe>> VmdAnimationFrames;
        /// <summary>
        /// speed of animation
        /// </summary>
        public float AnimationSpeed = 1.0f;

        public VmdFaceFrame[] VmdFaceFrames;
        /// <summary>
        /// xna will use this function to read my own .xnb file
        /// </summary>
        /// <param name="reader"></param>
        public void Load(ContentReader reader)
        {
            //read the speed of animation
            AnimationSpeed = reader.ReadSingle();
            //first of all:it's bones count
            BonesCount = reader.ReadInt32();

            VmdAnimationFrames = new Dictionary<string,List<VmdKeyframe>>();
            for (int i = 0; i < BonesCount; i++)
            {
                //for each bone

                //1 bone name
                string boneName = reader.ReadString();
                List<VmdKeyframe> framesOfThisBone = new List<VmdKeyframe>();
                //2 keyframe count
                int keyframeCountOfThisBone = reader.ReadInt32();
                //3 get all frame of this bone and push them into VmdFrames
                for (int j = 0; j < keyframeCountOfThisBone; j++)
                {
                    //get pos and rotation
                    VmdKeyframe keyframe = new VmdKeyframe();
                    //read time
                    keyframe.Time = reader.ReadSingle();
                    keyframe.Position = reader.ReadVector3();
                    keyframe.Rotation = reader.ReadQuaternion();
                    //push it into array
                    framesOfThisBone.Add(keyframe);
                }

                VmdAnimationFrames[boneName] = framesOfThisBone;
            }

            #region Face Frames
            //face frame count
            int faceFrameCount = reader.ReadInt32();
            //just a copy of sth
            VmdFaceFrames=new VmdFaceFrame[faceFrameCount];
            for (int i = 0; i < faceFrameCount; i++)
            {
                VmdFaceFrames[i].MorphName = reader.ReadString();
                VmdFaceFrames[i].IndexOfFrame=reader.ReadSingle();
                VmdFaceFrames[i].WeightOfBaseVertex=reader.ReadSingle();
            }
            #endregion
        }
    }
}
