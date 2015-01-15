using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace PmdModelLib
{
    public class IkManager
    {
        //array of chains
        IkChain[] ikChains;

        internal IkChain[] Chains
        {
            get { return ikChains; }
            set { ikChains = value; }
        }
        //number of chains
        ushort iKChainCount;

        public ushort IKChainCount
        {
            get { return iKChainCount; }
            set { iKChainCount = value; }
        }
        //get ikchains
        public IkChain[] IkChains
        {
            get { return ikChains; }
            set { ikChains = value; }
        }

        public IkManager()
        {

        }

        public void Reader(ContentReader reader)
        {
            //count of chains
            IKChainCount = reader.ReadUInt16();
            //for all chain
            ikChains = new IkChain[IKChainCount];
            for (int i = 0; i < IKChainCount; i++)
            {
                IkChains[i] = new IkChain();
                IkChains[i].Read(reader);
            }
        }
        ///// <summary>
        ///// update transform
        ///// </summary>
        ///// <param name="bonesFinalPos"></param>
        //public void Update(ref Matrix[] bonesFinalPos,ref Matrix[] boneLocalPos)
        //{
        //    //TODO:test
        //    //ikChains[4].Update(ref bonesFinalPos);
        //    for (int i = 0; i < iKChainCount; i++)//iKChainCount; i++)
        //    {
        //        ikChains[i].isFirstTime = true;
        //        for (int j = 0; j < ikChains[i].MaxIterationCount; j++)
        //        {
        //            ikChains[i].Update(ref bonesFinalPos, ref boneLocalPos);
        //            //ikChains[i].UpdateNew(ref bonesFinalPos, ref boneLocalPos);
        //        }
        //    }
        //}

        ///// <summary>
        ///// update bone's ik bones
        ///// </summary>
        ///// <param name="boneFinalPos"></param>
        ///// <param name="boneLocalPose"></param>
        //public void Update(ref Transform[] boneLocalTransform, ref Transform[] boneLocalPose)
        //{
        //    for (int i = 0; i < iKChainCount; i++)//iKChainCount; i++)
        //    {
        //        ikChains[i].isFirstTime = true;
        //        for (int j = 0; j < ikChains[i].MaxIterationCount; j++)
        //        {
        //            ikChains[i].Update(ref boneLocalTransform, ref boneLocalPose,2);
        //            //ikChains[i].UpdateNew(ref bonesFinalPos, ref boneLocalPos);
        //            ikChains[i].isFirstTime = false;
        //        }
        //    }
        //}
    }
    public class IkChain
    {
        #region variables
        //all is the same as PMD_IK
        public ushort IkTarget;

        public ushort IkEndBone;

        public byte IkNodeCount;

        public ushort MaxIterationCount;

        public float MaxAngleBetween;

        public ushort[] IkNodes;

        #endregion

        #region helper variables
        
        public float[] distances;

        public int ParentIndex;
        #endregion

        //array of nodes
        //List<IkNode> nodes = new List<IkNode>();
        //store fraing device
        //GraphicsDevice device;
       // ContentManager content;
        //draw effect
        //BasicEffect be;

        //float defaultLength = 5.0f;

        //public int NodeCount
        //{
        //    get
        //    {
        //        return nodes.Count;
        //    }
        //}

        public IkChain()//GraphicsDevice _device, ContentManager _content)
        {
            //device = _device;
            //content = _content;

            //be = new BasicEffect(device, null);
        }
        /// <summary>
        /// extract data
        /// </summary>
        /// <param name="reader"></param>
        public void Read(ContentReader reader)
        {
            //this is just  as single chain
            IkTarget = reader.ReadUInt16();
            IkEndBone = reader.ReadUInt16();
            IkNodeCount = reader.ReadByte();
            IkNodeCount += 1;
            MaxIterationCount = reader.ReadUInt16();
            MaxAngleBetween = reader.ReadSingle();
            IkNodes = new ushort[IkNodeCount];
            for (int i = 0; i < IkNodeCount-1; i++)
            {
                IkNodes[IkNodeCount-2-i] = reader.ReadUInt16();
            }
            IkNodes[IkNodeCount - 1] = IkEndBone;
            //initialize matrix for transforming node
            //accompanyMatrix = new Matrix[IkNodeCount];
            //accompanyTransform = new Transform[IkNodeCount];
            //local = new Matrix[IkNodeCount];
        }
        #region Initialize Nodes

        //public void Initialize(Matrix[] boneFinalPos,int parentIndex)
        //{
        //    //for (int i = 0; i < 8; i++)
        //    //    nodes.Add(new IkNode(content, new Vector3(i * defaultLength, 0, 0)));
        //    if (IkNodes == null)
        //        throw new InvalidOperationException("you should fill iknodes first");
        //    //calculate default distance between every two neighbors
            
        //    distances = new float[IkNodeCount-1];//
        //    for (int i = 0; i < IkNodeCount-1; i++)
        //    {
        //        //find two bone
        //        Vector3 offset = boneFinalPos[IkNodes[i + 1]].Translation - boneFinalPos[IkNodes[i]].Translation;
        //        distances[i] = offset.Length();

                
        //    }
        //    ParentIndex = parentIndex;
        //    index = IkNodeCount-1;// nodes.Count - 1;
        //}

        //internal void Initialize(Transform[] boneFinalPos, int parentIndex)
        //{
        //    if (IkNodes == null)
        //        throw new InvalidOperationException("you should fill iknodes first");
        //    //calculate default distance between every two neighbors

        //    distances = new float[IkNodeCount - 1];//
        //    for (int i = 0; i < IkNodeCount - 1; i++)
        //    {
        //        //find two bone
        //        Vector3 offset = boneFinalPos[IkNodes[i + 1]].Pos - boneFinalPos[IkNodes[i]].Pos;
        //        distances[i] = offset.Length();
        //    }
        //    ParentIndex = parentIndex;
        //    index = IkNodeCount - 1;// nodes.Count - 1;
        //}
        #endregion

        #region Update Nodes
        //test
        //int index;
        //public bool isFirstTime = false;
        ////KeyboardState lastState;
        ///// <summary>
        ///// using CCD Solution
        ///// </summary>
        //public void Update(ref Matrix[] boneFinalPos,ref Matrix[] boneLocalPos)//Vector3 goalPos)
        //{
            
        //    //for (int i = IkNodeCount-1; i >= 0; i--)
        //    //{
        //    //    //from end-1 to start 
        //    //    boneFinalPos[IkNodes[i]].Translation = boneFinalPos[IkTarget].Translation;

        //    //}

            


        //    Vector3 dir=Vector3.Zero;
        //    Vector3 dirOrigin = Vector3.Zero;
        //    for (int i = IkNodeCount - 2; i >= 0; i--)
        //    {
        //        //from end-1 to start 
        //        AdjustNodes(i, ref boneFinalPos);

        //    }
        //    //return;
        //    //for rotation
        //    Matrix tmp = Matrix.Identity;
        //    for (int i = 0; i < IkNodeCount-1; i++)
        //    {
        //        dir = boneFinalPos[IkNodes[i + 1]].Translation - boneFinalPos[IkNodes[i]].Translation;
        //        dir.Normalize();
        //        dirOrigin = boneLocalPos[IkNodes[i + 1]].Translation;
        //        dirOrigin.Normalize();
        //        Vector3 tmpPos = boneFinalPos[IkNodes[i]].Translation;
        //        boneFinalPos[IkNodes[i]].Translation = Vector3.Zero;

        //        boneFinalPos[IkNodes[i]] = ChangeDirectionTp(dir, dirOrigin);

                
        //        //boneFinalPos[IkNodes[i]] = GetRotationMatrix(boneFinalPos[ParentIndex]) * boneFinalPos[IkNodes[i]];

        //        if (i == 0)
        //        {
        //            boneFinalPos[IkNodes[i]] = GetRotationMatrix(boneFinalPos[ParentIndex]) * boneFinalPos[IkNodes[i]];
        //            tmp = GetRotationMatrix(boneFinalPos[ParentIndex]);
        //        }
        //        if (i >= 1)
        //        {
        //            tmp = GetRotationMatrix(boneLocalPos[IkNodes[i - 1]]) * tmp;
        //            boneFinalPos[IkNodes[i]] = tmp * boneFinalPos[IkNodes[i]];
        //        }
        //        boneFinalPos[IkNodes[i]].Translation = tmpPos;
        //    }
        //    //end pos
        //    //dir = boneFinalPos[IkTarget].Translation - boneFinalPos[IkNodes[IkNodeCount - 1]].Translation;
        //    //dir.Normalize();
        //    //dirOrigin = boneLocalPos[IkNodes[IkNodeCount - 1]].Translation;
        //    //Vector3 tmpPos2 = boneFinalPos[IkNodes[IkNodeCount - 1]].Translation;
        //    //boneFinalPos[IkNodes[IkNodeCount - 1]].Translation = Vector3.Zero;
        //    //boneFinalPos[IkNodes[IkNodeCount - 1]] = boneFinalPos[IkNodes[IkNodeCount - 2]];//ChangeDirectionTp(dir, dirOrigin);
        //    //boneFinalPos[IkNodes[IkNodeCount - 1]].Translation = tmpPos2;
        //}
        ///// <summary>
        ///// revise nodes after this node
        ///// </summary>
        ///// <param name="i"></param>
        //private void AdjustNodes(int currentNode, ref Matrix[] boneFinalPos)//Vector3 goalPos)
        //{
        //    //two vector
        //    Vector3 toGoal = boneFinalPos[IkTarget].Translation - boneFinalPos[IkNodes[currentNode]].Translation;// goalPos - nodes[currentNode].Pos;
        //    toGoal.Normalize();

        //    Vector3 toEnd = boneFinalPos[IkEndBone].Translation - boneFinalPos[IkNodes[currentNode]].Translation;// nodes[NodeCount - 1].Pos - nodes[currentNode].Pos;
        //    toEnd.Normalize();
        //    //test
        //    //boneFinalPos[IkNodes[currentNode]].Translation = boneFinalPos[IkTarget].Translation;
        //     //return;
        //    //angel between them
        //    float angle = -(float)Math.Acos(Vector3.Dot(toGoal, toEnd));

        //    if (angle > MathHelper.PiOver2 && angle < -MathHelper.PiOver2)
        //        return;
        //    if (angle > MaxAngleBetween || angle < -MaxAngleBetween)
        //        angle = MaxAngleBetween * (angle > 0 ? 1 : -1);

        //    //caculate rotate axis
        //    Vector3 axis = Vector3.Cross(toGoal, toEnd);
        //    axis.Normalize();
        //    if (axis.Length() == 0)
        //        return;

        //    //move node after currentNode[sometime contain error]
        //    Matrix rotation = Matrix.CreateFromAxisAngle(axis, angle);

        //    //if this ik is leg ik we should constaint it to rotate around X
        //    if (IkNodeCount == 3 )
        //        if (isFirstTime)
        //        {
        //            rotation = Matrix.CreateRotationX(Math.Abs(angle));
        //        }
        //        else
        //        {
        //            //rotation = Matrix.CreateRotationX(angle);
        //        }

        //    //transform myself
        //    //Matrix lastMatrix = boneFinalPos[IkNodes[currentNode]];
        //    //boneFinalPos[IkNodes[currentNode]] = rotation * boneFinalPos[IkNodes[currentNode]];
        //    //for (int i = currentNode + 1; i < IkNodeCount; i++)
        //    //{
        //    //    //from last to current local transformation
        //    //    Matrix local = boneFinalPos[IkNodes[i]]*Matrix.Invert(lastMatrix);
        //    //    local *= boneFinalPos[IkNodes[i - 1]];//get out new matrix desend from parent
        //    //    local.M11 = 1.0f;
        //    //    local.M22 = 1.0f;
        //    //    local.M33 = 1.0f;
        //    //    local.M44 = 1.0f;
        //    //    //refresh lastMatrix
        //    //    lastMatrix = boneFinalPos[IkNodes[i]];
        //    //    //update out new transform
        //    //    boneFinalPos[IkNodes[i]] = local;

        //    //}
        //    //return;
        //    //
        //    for (int i = currentNode + 1; i < IkNodeCount; i++)
        //    {
        //        //offset vector
        //        Vector3 offsetToCurrentNode = boneFinalPos[IkNodes[i]].Translation - boneFinalPos[IkNodes[currentNode]].Translation;//nodes[i].Pos - nodes[currentNode].Pos;

        //        float lengthOfOffset = offsetToCurrentNode.Length();
        //        //[handle error (if the goal is within the distance of maxLength of Nodes)]
        //        if (lengthOfOffset <= 0.01f)
        //            return;
        //        offsetToCurrentNode.Normalize();
        //        //rotate
        //        offsetToCurrentNode = Vector3.TransformNormal(offsetToCurrentNode, rotation);

        //        offsetToCurrentNode.Normalize();
        //        //[handle error if we got at invalide rotation]
        //        if (float.IsNaN(offsetToCurrentNode.X) || float.IsNaN(offsetToCurrentNode.Y)||
        //            float.IsNaN(offsetToCurrentNode.Z))
        //            return;
        //        //apply
        //        //nodes[i].Pos = nodes[currentNode].Pos + lengthOfOffset * offsetToCurrentNode;
        //        boneFinalPos[IkNodes[i]].Translation = boneFinalPos[IkNodes[currentNode]].Translation + lengthOfOffset * offsetToCurrentNode;
        //        //adjust the distance from last one
        //        Vector3 dir = boneFinalPos[IkNodes[i]].Translation - boneFinalPos[IkNodes[i-1]].Translation;//nodes[i].Pos - nodes[i - 1].Pos;
        //        dir.Normalize();
        //        //nodes[i].Pos = dir * defaultLength + nodes[i - 1].Pos;
        //        boneFinalPos[IkNodes[i]].Translation = dir * distances[i-1] + boneFinalPos[IkNodes[i - 1]].Translation;
        //    }
        //}

        ///// <summary>
        ///// change direction
        ///// </summary>
        ///// <param name="dir"></param>
        ///// <returns></returns>
        //Matrix ChangeDirectionTp(Vector3 dir,Vector3 dirOrigin)
        //{
        //    //default direction:(0,0,-1)
        //    dirOrigin.Normalize();
        //    Vector3 defaultVector = dirOrigin;
        //    dir.Normalize();
        //    //if (Math.Abs(dir.Length()-1.0f)>float.Epsilon)
        //        //throw new InvalidProgramException("dir error");
        //    float angle = (float)Math.Acos(Vector3.Dot(defaultVector, dir));
        //    if (float.IsNaN(angle)||Math.Abs(angle)<0.02f)
        //    {
        //        return Matrix.Identity;
        //    }
        //    Vector3 normal=Vector3.Cross(defaultVector,dir);
        //    normal.Normalize();
        //    return Matrix.CreateFromAxisAngle(normal,angle);
        //}

        //Matrix GetRotationMatrix(Matrix mm)
        //{
        //    Matrix mmNew=new Matrix();
        //    mmNew=mm;
        //    mmNew.Translation=new Vector3(0,0,0);
        //    return mmNew;
        //}
        #endregion

        #region Matrix Update

        //Vector3 EndPos=Vector3.Zero;
        //Matrix[] accompanyMatrix;
        //Matrix[] local;
        ///// <summary>
        ///// update ik node by Matrix Method
        ///// </summary>
        ///// <param name="boneFinalTransform"></param>
        ///// <param name="boneLocalPose"></param>
        //public void UpdateNew(ref Matrix[] boneFinalTransform, ref Matrix[] boneLocalPose)
        //{
        //    //1from end to start node
        //    for (int i = 0; i < IkNodeCount; i++)
        //    {
        //        if (i >= 1)
        //        {
        //            local[i] = boneFinalTransform[IkNodes[i]] * Matrix.Invert(boneFinalTransform[IkNodes[i - 1]]);
        //        }
        //        else
        //        {
        //            local[i] = Matrix.Identity;
        //        }
        //    }
        //    if (IkTarget == 80)
        //    {
        //        string iamhere = "here";
        //    }
        //    //we got end position of nodes
        //    EndPos = boneFinalTransform[IkNodes[IkNodeCount - 1]].Translation;
        //    //2 CCD to calculate rotation angle
        //    for (int i = 0; i < IkNodeCount; i++)
        //    {
        //        //initialize
        //        accompanyMatrix[i] = Matrix.Identity;
        //    }
        //    for (int i = IkNodeCount-2; i >=0; i--)
        //    {
                
        //        //calculate angle
        //        Vector3 toTarget = boneFinalTransform[IkTarget].Translation - boneFinalTransform[IkNodes[i]].Translation;
        //        //we transform EnsPos only
        //        Vector3 toEnd = EndPos - boneFinalTransform[IkNodes[i]].Translation;

        //        if (Vector3.Distance(toTarget, toEnd) < 0.03f)
        //            break;

        //        toTarget.Normalize();
        //        toEnd.Normalize();
        //        //angle 
        //        Vector3 CrossNormal = Vector3.Cross(toTarget,toEnd);
        //        if (CrossNormal.Length() <= 0.01f)
        //            continue;
        //        CrossNormal.Normalize();
        //        if (float.IsNaN(CrossNormal.X))
        //            continue;
        //        float angle = -(float)Math.Acos(Vector3.Dot(toTarget,toEnd));
        //        //if we reach to target
        //        //if (Math.Abs(angle) < 0.1f && i == IkNodeCount - 2) break;
        //        if (angle > MathHelper.PiOver2 && angle < -MathHelper.PiOver2)
        //            continue;
        //        //if we can't get right angle skip it to avoid error

        //        //angle 2 matrix
        //        Matrix rotation = Matrix.CreateFromAxisAngle(CrossNormal, angle);

        //        Vector3 offset = EndPos - boneFinalTransform[IkNodes[i]].Translation;
        //        float distance = offset.Length();
        //        if (distance <= 0.01f)
        //            continue;
        //        offset.Normalize();
        //        Vector3 dir = Vector3.Transform(offset, rotation);
        //        dir.Normalize();
        //        if (float.IsNaN(dir.X))
        //            continue;

        //        accompanyMatrix[i] = rotation;
        //        EndPos = dir * distance + boneFinalTransform[IkNodes[i]].Translation;
        //    }
        //    //and then is final step
        //    //transform this chain
        //    //Matrix accumulateMatrix=Matrix.Identity;
        //    //accompanyMatrix[IkNodeCount - 1] = Matrix.Identity;
        //    for (int i = 0; i < IkNodeCount; i++)
        //    {
        //        //accumulateMatrix = accompanyMatrix[i] *accumulateMatrix;
        //        //boneFinalTransform[IkNodes[i]] = boneFinalTransform[IkNodes[i]] *accumulateMatrix;
        //        if (i >= 1)
        //        {
        //            boneFinalTransform[IkNodes[i]] = local[i] * accompanyMatrix[i - 1] * boneFinalTransform[IkNodes[i-1]];
        //            local[i] = local[i] * accompanyMatrix[i - 1];// boneFinalTransform[IkNodes[i]] * Matrix.Invert(boneFinalTransform[IkNodes[i - 1]]);//Matrix.CreateTranslation(nodes[i].World.Translation - nodes[i - 1].World.Translation);
        //        }
        //        else
        //            boneFinalTransform[IkNodes[i]] = boneFinalTransform[IkNodes[i]];
        //    }

            
        //}

        ///// <summary>
        ///// transform method 3 
        ///// </summary>
        ///// <param name="boneFinalTransform"></param>
        ///// <param name="boneLocalPose"></param>
        //public void Update3(ref Matrix[] boneFinalTransform, ref Matrix[] boneLocalPose)
        //{
        //    //initialize
        //    for (int i = 0; i < IkNodeCount; i++)
        //    {
        //        if (i >= 1)
        //        {
        //            local[i] = boneFinalTransform[IkNodes[i]] * Matrix.Invert(boneFinalTransform[IkNodes[i - 1]]);
        //            local[i].M41 = 0.0f;
        //            local[i].M42 = 0.0f;
        //            local[i].M43 = 0.0f;
        //        }
        //        else
        //        {
        //            local[i] = Matrix.Identity;
        //        }
        //    }
        //    for (int i = 0; i <IkNodeCount ; i++)
        //    {
        //        accompanyMatrix[i] = local[i];
        //    }
        //    //for iterations
        //    //single step
        //    for (int k = 0; k < IkNodeCount; k++)
        //    {

        //    }
        //}

        //void CCD_TwoAndTarget(ref Matrix start,ref Matrix end,ref Matrix target,float limitAngle,int ikIndex)
        //{
        //    ////two vector
        //    //Vector3 toTarget = target.Translation - start.Translation;
        //    //Vector3 toEnd = end.Translation - start.Translation;
        //    ////
        //    ////Matrix rot=start*Matrix.CreateTranslation(start.Translation);
        //    ////Quaternion invRotation =;// 求出四元数的共轭四元数
        //    //Matrix invStart=Matrix.Invert(start);
        //    //toTarget = Vector3.TransformNormal(toTarget, invStart);
        //    //toEnd = Vector3.TransformNormal(toEnd, invStart);

        //    ////if we reach the target
        //    //if(Vector3.Distance(toEnd,toTarget)<0.01f)
        //    //    return;
        //    //toTarget.Normalize();
        //    //toEnd.Normalize();

        //    //float deltaAngle = (float)Math.Acos(Vector3.Dot(toTarget,toEnd));

        //    ////if angle is tosmall or contain some error
        //    //if (Math.Abs(deltaAngle) < 0.1f || float.IsNaN(deltaAngle))
        //    //{
        //    //    return;
        //    //}

        //    //limitAngle = (float)Math.Abs(limitAngle);
        //    //if (deltaAngle > limitAngle) deltaAngle = limitAngle;
        //    //else if (deltaAngle < -limitAngle) deltaAngle = -limitAngle;

        //    ////cross axis
        //    //Vector3 axis=Vector3.Cross(toTarget,toEnd);
        //    //axis.Normalize();

        //    //Quaternion qRot = Quaternion.CreateFromAxisAngle(axis, deltaAngle);

        //    //accompanyMatrix[ikIndex] *= Matrix.CreateFromQuaternion(qRot);

        //    //if(ikIndex>=1)
        //    //start=accompanyMatrix[ikIndex]*/*parent rotation matrix*/
        //}
        #endregion

        #region Update by Transform[Vector3+Quaternion]
        /// <summary>
        /// update Chain Transform
        /// </summary>
        /// <param name="boneFinalPos"></param>
        /// <param name="boneLocalPose"></param>
        //public void Update(ref Transform[] boneFinalPos, ref Transform[] boneLocalPose)
        //{
        //    for (int i = IkNodeCount - 2; i >= 0; i--)
        //    {
        //        //from end-1 to start 
        //        AdjustNodes(i, ref boneFinalPos);

        //    }
        //    if(isFirstTime)
        //    isFirstTime = false;
        //}
        //Transform[] accompanyTransform;
        //void AdjustNodes(int currentNode, ref Transform[] boneFinalPos)
        //{
        //    Quaternion qInvRotation = boneFinalPos[currentNode].Rot;
        //    qInvRotation.Conjugate();

        //    //two vector
        //    Vector3 toGoal = boneFinalPos[IkTarget].Pos - boneFinalPos[IkNodes[currentNode]].Pos;// goalPos - nodes[currentNode].Pos;
        //    //toGoal = Vector3.Transform(toGoal, qInvRotation);
            

        //    Vector3 toEnd = boneFinalPos[IkEndBone].Pos - boneFinalPos[IkNodes[currentNode]].Pos;// nodes[NodeCount - 1].Pos - nodes[currentNode].Pos;
        //    //toEnd = Vector3.Transform(toEnd, qInvRotation);

        //    if (Vector3.Distance(toGoal, toEnd) < 0.01f)
        //        return;
        //    toGoal.Normalize();
        //    toEnd.Normalize();


        //    //test
        //    //boneFinalPos[IkNodes[currentNode]].Translation = boneFinalPos[IkTarget].Translation;
        //    //return;
        //    //angel between them
        //    float angle = -(float)Math.Acos(Vector3.Dot(toGoal, toEnd));

        //    if (angle > MathHelper.PiOver2 && angle < -MathHelper.PiOver2)
        //        angle = MaxAngleBetween * (angle > 0 ? 1 : -1);
            
        //    if (angle > MaxAngleBetween || angle < -MaxAngleBetween)
        //        angle = MaxAngleBetween * (angle > 0 ? 1 : -1);

        //    //caculate rotate axis
        //    Vector3 axis = Vector3.Cross(toGoal, toEnd);
            
        //    if (axis.Length() < 0.01f)
        //        return;
        //    axis.Normalize();
        //    if (float.IsNaN(axis.X))
        //        return;
        //    //move node after currentNode[sometime contain error]
        //    Quaternion rotation = Quaternion.CreateFromAxisAngle(axis, angle);

        //    //if this ik is leg ik we should constaint it to rotate around X
        //    if (IkNodeCount == 3)
        //        if (isFirstTime)
        //        {
        //            rotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), Math.Abs(angle*0.2f));// Matrix.CreateRotationX(Math.Abs(angle));
        //        }
        //        else //if(false)
        //        {
        //            //rotation = Matrix.CreateRotationX(angle);
        //            Vector3 angles = Quaternion2Euler(rotation);
        //            Vector3 currentAngles = Quaternion2Euler(boneFinalPos[IkNodes[currentNode]].Rot);

        //            DecompositeQuaternion(rotation, out angles.X, out angles.Y, out angles.Z);
        //            DecompositeQuaternion(boneFinalPos[IkNodes[currentNode]].Rot, out currentAngles.X, out currentAngles.Y, out currentAngles.Z);

        //            float Yaw = angles.X;
        //            float CurrentYaw = currentAngles.X;

        //            if (float.IsNaN(Yaw) || ((float)Math.Abs(Yaw)) < 0.001f)
        //                return;
        //            //if (currentAngles.Y  < 0)
        //                //angles.Y = (float)Math.Abs(currentAngles.Y);

        //            //limit the yaw angle

        //            //if (Yaw > MathHelper.Pi - CurrentYaw)
        //            //    Yaw = MathHelper.Pi - CurrentYaw;
        //            //else if (Yaw < -0.002f - CurrentYaw)
        //            //    Yaw = -0.002f - CurrentYaw;

        //            //euler 2 quaternion
        //            rotation=Quaternion.CreateFromYawPitchRoll(Yaw,angles.Y, angles.Z);
        //        }
        //    boneFinalPos[IkNodes[currentNode]].Rot = rotation *boneFinalPos[IkNodes[currentNode]].Rot;
        //    for (int i = currentNode + 1; i < IkNodeCount; i++)
        //    {
        //        //offset vector
        //        Vector3 offsetToCurrentNode = boneFinalPos[IkNodes[i]].Pos - boneFinalPos[IkNodes[currentNode]].Pos;//nodes[i].Pos - nodes[currentNode].Pos;

        //        float lengthOfOffset = offsetToCurrentNode.Length();
        //        //[handle error (if the goal is within the distance of maxLength of Nodes)]
        //        if (lengthOfOffset <= 0.01f)
        //            return;
        //        offsetToCurrentNode.Normalize();
        //        //rotate
        //        offsetToCurrentNode = Vector3.Transform(offsetToCurrentNode, rotation);

        //        offsetToCurrentNode.Normalize();
        //        //[handle error if we got at invalide rotation]
        //        if (float.IsNaN(offsetToCurrentNode.X) || float.IsNaN(offsetToCurrentNode.Y) ||
        //            float.IsNaN(offsetToCurrentNode.Z))
        //            return;
        //        //apply
        //        //nodes[i].Pos = nodes[currentNode].Pos + lengthOfOffset * offsetToCurrentNode;
        //        boneFinalPos[IkNodes[i]].Pos = boneFinalPos[IkNodes[currentNode]].Pos + lengthOfOffset * offsetToCurrentNode;
        //        //adjust the distance from last one
        //        Vector3 dir = boneFinalPos[IkNodes[i]].Pos - boneFinalPos[IkNodes[i - 1]].Pos;//nodes[i].Pos - nodes[i - 1].Pos;
        //        dir.Normalize();
        //        //nodes[i].Pos = dir * defaultLength + nodes[i - 1].Pos;
        //        boneFinalPos[IkNodes[i]].Pos = dir * distances[i - 1] + boneFinalPos[IkNodes[i - 1]].Pos;
        //        //skip the end bone[WE SHOULDN'T ROTATE IT]
        //        if(i<IkNodeCount-1)
        //        boneFinalPos[IkNodes[i]].Rot = rotation * boneFinalPos[IkNodes[i]].Rot;
        //    }
        //}

        //public Vector3 Quaternion2Euler(Quaternion q1)
        //{
        //    //float sqw = q1.W*q1.W;
        //    //float sqx = q1.X*q1.X;
        //    //float sqy = q1.Y*q1.Y;
        //    //float sqz = q1.Z*q1.Z;
        //    //float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
        //    //float test = q1.X*q1.Y + q1.Z*q1.W;

        //    //float heading, attitude, bank;
        //    //if (test > 0.499*unit) { // singularity at north pole
        //    //    heading =(float)( 2 * Math.Atan2(q1.X,q1.W));
        //    //    attitude = (float)Math.PI/2;
        //    //    bank = 0.0f;
        //    //    return new Vector3(heading,attitude,bank);
        //    //}
        //    //if (test < -0.499*unit) { // singularity at south pole
        //    //    heading = -(float)(2 * Math.Atan2(q1.X, q1.W));
        //    //    attitude = (float)(-Math.PI/2);
        //    //    bank = 0;
        //    //    return new Vector3(heading,attitude,bank);
        //    //}
        //    //heading = (float)Math.Atan2(2*q1.Y*q1.W-2*q1.X*q1.Z , sqx - sqy - sqz + sqw);
        //    //attitude = (float)Math.Asin(2 * test / unit);
        //    //bank = (float)Math.Atan2(2 * q1.X* q1.W - 2 * q1.Y * q1.Z, -sqx + sqy - sqz + sqw);

        //    //return new Vector3(heading, attitude, bank);
        //    Vector3 euler=new Vector3(0,0,0);
        //    double sqw = q1.W * q1.W;
        //    double sqx = q1.X * q1.X;
        //    double sqy = q1.Y * q1.Y;
        //    double sqz = q1.Z * q1.Z;

        //    // heading = rotation about z-axis  [ROLL]
        //    euler.Z = (float)(Math.Atan2(2.0 * (q1.X * q1.Y + q1.Z * q1.W), (sqx - sqy - sqz + sqw)));

        //    // bank = rotation about x-axis     [Pitch]
        //    euler.X = (float)(Math.Atan2(2.0 * (q1.Y * q1.Z + q1.X * q1.W), (-sqx - sqy + sqz + sqw)));

        //    // attitude = rotation about y-axis [YAW]
        //    euler.Y = (float)(Math.Asin(-2.0 * (q1.X * q1.Z - q1.Y * q1.W)));

        //    return euler;
        //}

        ///// <summary>
        ///// クォータニオンをYaw(Y回転), Pitch(X回転), Roll(Z回転)に分解する関数
        ///// </summary>
        ///// <param name="input">分解するクォータニオン</param>
        ///// <param name="YRot">Y軸回転</param>
        ///// <param name="XRot">X軸回転(-PI/2～PI/2)</param>
        ///// <param name="ZRot">Z軸回転</param>
        ///// <returns>ジンバルロックが発生した時はfalse。ジンバルロックはX軸回転で発生</returns>
        //public bool DecompositeQuaternion(Quaternion input, out float YRot, out float XRot, out float ZRot)
        //{
        //    //クォータニオンの正規化
        //    Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
        //    inputQ.Normalize();
        //    //マトリクスを生成する
        //    Matrix rot = Matrix.CreateFromQuaternion(inputQ);
        //    //ヨー(X軸周りの回転)を取得
        //    if (rot.M32 > 1 - 1.0e-4 || rot.M32 < -1 + 1.0e-4)
        //    {//ジンバルロック判定
        //        XRot = (rot.M32 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
        //        ZRot = 0; YRot = -(float)Math.Atan2(rot.M21, rot.M11);

        //        return false;
        //    }
        //    XRot = -(float)Math.Asin(rot.M32);
        //    //ピッチを取得
        //    YRot = -(float)Math.Atan2(-rot.M31, rot.M33);
        //    //ロールを取得
        //    ZRot = -(float)Math.Atan2(-rot.M12, rot.M22);
        //    return true;
        //}
        #endregion


        #region Method in MMD

        
        //internal void Update(ref Transform[] boneLocalTransform, ref Transform[] boneLocalPose, int p)
        //{
        //    throw new NotImplementedException();
        //} 
        #endregion
    }
}
