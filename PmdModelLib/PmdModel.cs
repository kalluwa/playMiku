using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

//����һ�������ڳ�������ʱ��ȡModel��lib
//����֮ǰ��XNA��ҪImport��Process PMD�ļ�
//�����Ҫ��������lib
namespace PmdModelLib
{
    #region PMD Model�ļ����ݽṹ
    /// <summary>
    /// �������ļ��ṹ��Ϣ
    /// </summary>
    struct MaterialsInfo
    {
        public Vector3 Diffuse;
        public float Alpha;
        public float Shininess;    // �����
        public Vector3 Specular;     // �����
        public Vector3 Ambient;      // ������
        public byte ToonNo;  // �з��ţ���ͨ��ɫ������
        public byte Edge;   // �Ƿ���ߣ������ֽ�>0ʱΪtrue
        public int FaceVertexCount;  // �涥����
        public string Name;
    }
    /// <summary>
    /// ������Ϣ��
    /// һ������index����֮��Ӧ��weight
    /// ��һ������index����֮��Ӧ��weight
    /// </summary>
    struct VertexWithBone
    {
        public Vector3 Position;
        public Vector3 PositionExtra;
        public Int16 Bone1;
        public Int16 Bone2;
        public float Weight;
    }
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public class Bone
    {
        public string Name;
        public short Parent;
        public short To;
        public byte Kind;
        //���������ik��Ϣ��ֻ�е�����������Ϊ�ض���typeʱ�Żᷢ������
        public short IKNum;
        public Vector3 Position;
        public Matrix localTransfrom;
        public ushort BoneNum;
        public List<Bone> children;
        public Bone()
        {
            children = new List<Bone>();
        }

        public void Transform(Matrix transform)
        {
            //if (localTransfrom == null)
            //{
            //    if (Position != null)
            //        localTransfrom = Matrix.CreateTranslation(Position);
            //    else
            //        localTransfrom = Matrix.Identity;
            //}

            localTransfrom = transform;

            //this.Position=Vector3.Transform(this.Position,transform);
            //localTransfrom = Matrix.CreateTranslation(Position) * localTransfrom;
            Position = (Matrix.CreateTranslation(Position) * localTransfrom).Translation;
            foreach (Bone bone in children)
            {
                // Bone parentBone=FindBone(bone.Parent);
                //float length = (bone.Position - Position).Length();
                //bone.Position = Position +length*Vector3.Normalize(Vector3.Transform(bone.Position - Position, transform));// (bone.localTransfrom * transform).Translation; ;
                bone.Transform(transform);
            }
        }

        public Bone FindBone(int i)
        {
            if (i < 1)
                return this;
            for (int p = 0; p < this.children.Count; p++)
            {
                if (i == children[p].BoneNum)
                    return children[p];
                else
                    children[p].FindBone(i);
            }
            //throw new Exception("û�иù���");
            return null;
        }


    } 
    #endregion

    #region Face Morph Information

    struct FaceVertex//=skinVertex
    {
        public uint Index;  // Ƥ����Ӧ����ɶ���
        public Vector3 Position; // ����λ��
        //we will use Position to store local position bone1

        public Vector3 PositionExtra;//we need another pos to store local pos bone2
        public void Read(ContentReader reader)
        {
            Index = reader.ReadUInt32();
            Position = reader.ReadVector3();
        }
    };
    struct FaceMorph//equal PMD_SKIN
    {
        public string MorphName;    // Ƥ������
        public uint VerticeCount; // Ƥ�����еĶ�������

        public FaceVertex[] FaceVertices;//=new SkinVertex[VertexListCount];  // Ƥ�����������ж�����Ϣ
        public void Read(ContentReader reader)
        {
            MorphName = reader.ReadString();
            VerticeCount = reader.ReadUInt32();
            //Warning: no type read
            FaceVertices = new FaceVertex[VerticeCount];
            for (int i = 0; i < VerticeCount; i++)
            {
                FaceVertices[i].Read(reader);
            }
        }
    };
    #endregion

    /// <summary>
    /// custom transform to replace Matrix
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// bone position
        /// </summary>
        public Vector3 Pos;
        /// <summary>
        /// bone rotation
        /// </summary>
        public Quaternion Rot;

        public static Transform Identity
        {
            get
            {
                return new Transform(new Vector3(0, 0, 0), Quaternion.Identity);
            }
        }
        #region Constructor

        public Transform(Vector3 _pos, Quaternion _rot)
        {
            Pos = _pos;
            Rot = _rot;
            Rot.Normalize();
        }
        /// <summary>
        /// default Transform
        /// </summary>
        public Transform()
        {
            this.Pos = new Vector3(0, 0, 0);
            this.Rot = Quaternion.Identity;
        }
        #endregion

        #region Helper Functions

        public static Transform operator *(Transform t1, Transform t2)
        {
            //TODO:1 
            //t1 is child,t2 is parent
            //use parent's rotation to rotate child LOCAL pos
            Transform newT = new Transform();
            newT.Pos = t2.Pos + Vector3.Transform(t1.Pos,t2.Rot);
            //* equals Concatenate
            //newT.Rot = t1.Rot * t2.Rot;
            newT.Rot = Quaternion.Concatenate(t1.Rot, t2.Rot);
            newT.Rot.Normalize();

            return newT;
            //TODO:2
            //Transform newT = new Transform();
            //Matrix m1 = Matrix.CreateTranslation(t1.Pos) * Matrix.CreateFromQuaternion(t1.Rot);
            //Matrix m2 = Matrix.CreateTranslation(t2.Pos) * Matrix.CreateFromQuaternion(t2.Rot);
            //Matrix tmp = m1 * m2;
            //newT.Pos = tmp.Translation;
            //tmp.Translation = Vector3.Zero;
            //newT.Rot = Quaternion.CreateFromRotationMatrix(tmp);
            
            //return newT;
        }

        /// <summary>
        /// replace the operator =
        /// </summary>
        /// <param name="t1"></param>
        public Transform(Transform t1)
        {
            Rot=t1.Rot;
            Pos=t1.Pos;

        }
        public Matrix ToMatrix()
        {
            // |y          o(affected by first bone's rotation)
            // |          / 
            // |         /
            // |       o/
            // |        (translate to this place then rotate the bone)
            //-----------------------------x
            //translation first then rotate
            Matrix t = Matrix.Identity;
            //                  rotate myself            transform to given position
            t = Matrix.CreateFromQuaternion(Rot) * Matrix.CreateTranslation(Pos);
            return t;
        }

        /// <summary>
        /// invert the transform
        /// </summary>
        /// <returns></returns>
        public void Invert()
        {
            Pos = -Pos;
            Rot.Conjugate();
            Rot.Normalize();
        }

        /// <summary>
        /// static invert
        /// </summary>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static Transform Invert(Transform t1)
        {
            Transform t=new Transform(t1);

            t.Invert();
            return t;
        }

        /// <summary>
        /// transform a vector
        /// </summary>
        /// <param name="v"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 TransformVector(Vector3 v, Transform t)
        {
            //eg:rotate a point arount a axis
            //first rotate it 
            Vector3 r = Vector3.Zero;
            r = Vector3.Transform(v, t.Rot);
            //then move it to the axis position
            r += t.Pos;

            return r;
        }
        #endregion
    }
    /// <summary>
    /// ����xna��ȡ��model��
    /// </summary>
    public class PmdModel
    {
        #region Variables
        /// <summary>
        /// variables
        /// </summary>
        /// ʹ�û���effect
        BasicEffect effect;

        //��������ݶ��Ǹ����ļ��洢�ṹ���ɵ�
        //û�кܴ��ʵ�ü�ֵ
        int m_nVerticesCount;

        public int VerticesCount
        {
            get { return m_nVerticesCount; }
            set { m_nVerticesCount = value; }
        }

        VertexWithBone[] vertices;
        VertexPositionNormalTexture[] verticesOnly;
        Bone[] tempBoneList;
        /// <summary>
        /// we will use this for draw IK node
        /// </summary>
        public Bone[] BoneList
        {
            get { return tempBoneList; }
        }

        MaterialsInfo[] materials;
        int currentMaterialIndex = 0;
        short[] indices;
        //�����豸
        GraphicsDevice device;
        //�ܵ�
        ContentManager Content;
        Bone rootBone;

        //��������Ԫ�����������˶�
        int m_nBoneCount;

        public int BoneCount
        {
            get { return m_nBoneCount; }
            set { m_nBoneCount = value; }
        }
        Transform[] boneBindPose;
        Transform[] boneLocalTransform;
        Transform[] boneLocalPose;
        Transform[] boneFinalPos;

        public Transform[] BoneFinalPos
        {
            get { return boneFinalPos; }
            set { boneFinalPos = value; }
        }
        public Transform[] BoneLocalPose
        {
            get { return boneLocalPose; }
            set { boneLocalPose = value; }
        }
        Transform[] bonesInverseBindBose;
        int[] bonesParent;

        public int[] BoneParent
        {
            get { return bonesParent; }
            set { bonesParent = value; }
        }
        //create  custom animation 
        Transform[] customBoneTransformation; 

        //anims
        List<string> bonesNameList; 
        public bool LoopAnimation = true;
        VmdAnimation currentAnimation = null;
        float g_time = 0.0f;
        float game_time = 0.0f;
        /// <summary>
        /// false:play animation
        /// true:stop animation
        /// </summary>
        bool bPause = false;
        /// <summary>
        /// false��play animation
        /// true:stop animation
        /// </summary>
        bool bPauseBoneUpdate = true;

        public bool PauseBoneUpdate
        {
            get { return bPauseBoneUpdate; }
            set { bPauseBoneUpdate = value; }
        }

        public bool Pause
        {
            get { return bPause; }
            set { bPause = value; }
        }

        //use ik
        IkManager IKs = new IkManager();

        public IkManager IK_Chains
        {
            get { return IKs; }
            set { IKs = value; }
        }
        //face morph
        FaceMorph[] faceMorphs;
        int baseMorphIndex = 0;
        Dictionary<String, int> morphNameToFaceIndex = new Dictionary<string, int>();
        #endregion

        
        /// <summary>
        /// ���캯����ʵ�������õ���Load
        /// </summary>
        public PmdModel()
        {

        }
        /// <summary>
        /// �ڶ�ȡ�ļ�֮ǰ��������Ҫ��ʼ���豸��Content
        /// </summary>
        /// <param name="device"></param>
        /// <param name="content"></param>
        public void Initialize(GraphicsDevice device,ContentManager content)
        {
            this.device = device;
            this.Content = content;
            effect = new BasicEffect(device);
            
        }
        /// <summary>
        /// ��ȡModel������ǰ����ļ��ṹ����
        /// </summary>
        /// <param name="reader"></param>
        public void Load(ContentReader reader)
        {
            //֮ǰ�Ѿ���������ļ�ͷ�Ķ�ȡ
            #region Vertex
            //������Ŀ
            int vertexCount = reader.ReadInt32();
            VerticesCount = vertexCount;
            //��������
            vertices = new VertexWithBone[vertexCount];
            verticesOnly = new VertexPositionNormalTexture[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                verticesOnly[i] = new VertexPositionNormalTexture(
                    reader.ReadVector3(),
                    reader.ReadVector3(),
                    reader.ReadVector2()
                    );
                vertices[i].Position = verticesOnly[i].Position;
                vertices[i].Bone1 =
                reader.ReadInt16();
                vertices[i].Bone2 =
                reader.ReadInt16();
                vertices[i].Weight =1.0f-
                reader.ReadByte() / 100.0f;
            } 
            #endregion

            #region Triangles
            //������
            //face ��Ŀ
            uint faceCount = reader.ReadUInt32();
            //face����
            indices = new short[faceCount];
            for (int i = 0; i < faceCount; i++)
            {
                indices[i] = (short)reader.ReadUInt16();
            } 
            #endregion

            #region Materials
            //����
            //������Ŀ
            uint materialCount = reader.ReadUInt32();
            //��������
            materials = new MaterialsInfo[materialCount];
            for (int i = 0; i < materialCount; i++)
            {
                materials[i].Diffuse = reader.ReadVector3();
                materials[i].Alpha = reader.ReadSingle();
                materials[i].Shininess = reader.ReadSingle();
                materials[i].Specular = reader.ReadVector3();
                materials[i].Ambient = reader.ReadVector3();
                materials[i].ToonNo = reader.ReadByte();
                materials[i].Edge = reader.ReadByte();
                materials[i].FaceVertexCount = reader.ReadInt32();
                materials[i].Name = reader.ReadString();
            } 
            #endregion

            #region Bones
            //����
            //��Ŀ
            ushort boneCount = reader.ReadUInt16();

            BoneCount = boneCount;
            //���ӹ�����
            //����model�Ļ���bonePos
            boneBindPose = new Transform[boneCount];
            bonesInverseBindBose = new Transform[boneCount];
            bonesParent = new int[boneCount];

            boneLocalTransform = new Transform[boneCount];
            boneLocalPose = new Transform[boneCount];
            boneFinalPos = new Transform[boneCount];
            customBoneTransformation = new Transform[boneCount];

            //����������������ù���������
            bonesNameList = new List<string>();

            
            //����
            tempBoneList = new Bone[boneCount];
            for (ushort i = 0; i < boneCount; i++)
            {
                //��ȡ��������
                tempBoneList[i] = new Bone();
                tempBoneList[i].BoneNum = i;
                tempBoneList[i].Name = reader.ReadString();
                tempBoneList[i].Parent = reader.ReadInt16();
                tempBoneList[i].To = reader.ReadInt16();
                tempBoneList[i].Position = reader.ReadVector3();

                //for ik
                tempBoneList[i].IKNum = reader.ReadInt16();
                tempBoneList[i].Kind = reader.ReadByte();
                ////��������ı任
                customBoneTransformation[i] = Transform.Identity;
                boneBindPose[i] = Transform.Identity;
                //control matrices
                boneBindPose[i].Pos=tempBoneList[i].Position;
                bonesInverseBindBose[i] = Transform.Invert(boneBindPose[i]);
                boneFinalPos[i] = boneBindPose[i];
                boneLocalPose[i] = boneBindPose[i];

                bonesNameList.Add(tempBoneList[i].Name);
                if (tempBoneList[i].Parent >= 0)
                {
                    bonesParent[i] = bonesNameList.IndexOf(tempBoneList[tempBoneList[i].Parent].Name);
                    //for local transformation
                    boneLocalPose[i] = boneLocalPose[i] * bonesInverseBindBose[bonesParent[i]];
                    boneLocalTransform[i] = boneLocalPose[i];
                }
                else
                    bonesParent[i] = -1;
            }
            //����Ĺ���Ӧ�ó�ȥ
            for (ushort i = 0; i < boneCount; i++)
            {
                for (int j = 0; j < boneCount; j++)
                {
                    if (tempBoneList[i].To == j && tempBoneList[i].To != 0)
                    {
                        if (!tempBoneList[i].children.Contains(tempBoneList[j]))
                            tempBoneList[i].children.Add(tempBoneList[j]);
                    }
                    else if (tempBoneList[i].Parent == j)
                    {
                        if (!tempBoneList[j].children.Contains(tempBoneList[i]))
                            tempBoneList[j].children.Add(tempBoneList[i]);
                    }
                }

            }
            rootBone = tempBoneList[0]; 

            //Important: fix the vertices' position to local
            for (int i = 0; i < VerticesCount; i++)
            {
                Vector3 absolutePos = new Vector3(vertices[i].Position.X,vertices[i].Position.Y,vertices[i].Position.Z);

                vertices[i].Position = Transform.TransformVector(absolutePos, bonesInverseBindBose[vertices[i].Bone1]);
                vertices[i].PositionExtra = Transform.TransformVector(absolutePos, bonesInverseBindBose[vertices[i].Bone2]);
            }
            
            #endregion

            #region IK Nodes
            //read iks
            IKs.Reader(reader);
            //for (int i = 0; i < IKs.IKChainCount; i++)
            //{
            //    int parentIndex=tempBoneList[IKs.IkChains[i].IkNodes[0]].Parent;
            //    IKs.IkChains[i].Initialize(boneFinalPos,parentIndex);
            //}
            #endregion

            #region Face Morph
            //we have multi face morphs
            int faceMorphCount=reader.ReadUInt16();
            faceMorphs = new FaceMorph[faceMorphCount];
            for (int i = 0; i < faceMorphCount; i++)
            {
                faceMorphs[i].Read(reader);
                //trasform face morphs' vertices to world coordinate
                //warning :we need base at morph index=0
                //set base morphs index
                if (faceMorphs[i].MorphName == "base")
                    baseMorphIndex = i;
                if (baseMorphIndex != 0)
                    throw new Exception("Base Morph Index is not 0!!!You need change the function");
                

                if (faceMorphs[i].MorphName != "base")
                {
                    for (int j = 0; j < faceMorphs[i].VerticeCount; j++)
                    {
                        faceMorphs[i].FaceVertices[j].Position +=
                            faceMorphs[baseMorphIndex].FaceVertices[faceMorphs[i].FaceVertices[j].Index].Position;
                    }
                }
            }

            for (int i = 0; i < faceMorphCount; i++)
            {
                //update face vertex pos to local
                for (int j = 0; j < faceMorphs[i].VerticeCount; j++)
			    {
                    int boneIndex_1;
                    int boneIndex_2;

                    if (i == baseMorphIndex)
                    {
                        boneIndex_1 = vertices[faceMorphs[baseMorphIndex].FaceVertices[j].Index].Bone1;
                        boneIndex_2 = vertices[faceMorphs[baseMorphIndex].FaceVertices[j].Index].Bone2;
                    }
                    //get bone
                    else
                    {
                        boneIndex_1 = vertices[faceMorphs[baseMorphIndex].FaceVertices[faceMorphs[i].FaceVertices[j].Index].Index].Bone1;
                        boneIndex_2 = vertices[faceMorphs[baseMorphIndex].FaceVertices[faceMorphs[i].FaceVertices[j].Index].Index].Bone2;
                    }
                    faceMorphs[i].FaceVertices[j].PositionExtra= Transform.TransformVector(faceMorphs[i].FaceVertices[j].Position, bonesInverseBindBose[boneIndex_2]);
                    faceMorphs[i].FaceVertices[j].Position = Transform.TransformVector(faceMorphs[i].FaceVertices[j].Position, bonesInverseBindBose[boneIndex_1]);
                
                }
                
                //create a map to index
                morphNameToFaceIndex[faceMorphs[i].MorphName] = i;
            }
            #endregion
        }

        /// <summary>
        /// set current animation
        /// </summary>
        /// <param name="_anim"></param>
        public void SetAnim(VmdAnimation _anim)
        {
            currentAnimation = _anim;
        }
        
        /// <summary>
        /// ����ģ�͵�״̬
        /// �ر�ע����ǣ�
        /// �����Ŀ��ʱʹ��CPU���й����ı任���ǻ�������ԭ��
        /// 1��sm2.0���֧��59��bone��ʹ��skineEffect���Ҳֻ��֧��72��bone
        /// 2��ʹ��CPU���򵥣������ҳ�bug
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            //customBoneTransformation[3] = Matrix.CreateRotationX(MathHelper.PiOver4 / 45.0f * 30.0f);
            game_time += gameTime.ElapsedGameTime.Milliseconds * 0.001f;
#if !WINDOWS_PHONE
            if(bPauseBoneUpdate&&!bPause)
#endif
            UpdateAnimation(gameTime);

            UpdateFaceAnimation(gameTime);
#if !WINDOWS_PHONE
            if(bPauseBoneUpdate)
#endif
            UpdateBones();
            //insert ik manage code here
            UpdateBoneByIK();
            /////////////////
            UpdateBoneVertices();
            
        }

        private void UpdateFaceAnimation(GameTime gameTime)
        {
            //change localposition
            int frameIndex = 0;

            while (frameIndex < currentAnimation.VmdFaceFrames.Length &&
                currentAnimation.VmdFaceFrames[frameIndex].IndexOfFrame < g_time)
            {
                frameIndex++;
            }
            for (int currenFrameIndex =frameIndex ; currenFrameIndex >0; currenFrameIndex--)
            {


                if (currenFrameIndex >= currentAnimation.VmdFaceFrames.Length)
                    currenFrameIndex = currentAnimation.VmdFaceFrames.Length - 1;

                //extract weight
                //current pos
                //if out model doesn't have the face morph
                if (!morphNameToFaceIndex.ContainsKey(currentAnimation.VmdFaceFrames[currenFrameIndex].MorphName))
                    return;

                int endMorphIndex = morphNameToFaceIndex[currentAnimation.VmdFaceFrames[currenFrameIndex].MorphName];
                for (int j = 0; j < faceMorphs[endMorphIndex].FaceVertices.Length; j++)
                {
                    //bone1 _local
                    #region Bone Position1
                    Vector3 endPos = faceMorphs[endMorphIndex].FaceVertices[j].Position;
                    //startPosIndex: pos in morph "base" or in all vertices?
                    uint startPosIndex = faceMorphs[endMorphIndex].FaceVertices[j].Index;
                    Vector3 startPos = faceMorphs[baseMorphIndex].FaceVertices[startPosIndex].Position;
                    //or
                    //Vector3 startPos = vertices[startPosIndex].Position;

                    endPos = startPos * (1-currentAnimation.VmdFaceFrames[currenFrameIndex].WeightOfBaseVertex )+
                        endPos * ( currentAnimation.VmdFaceFrames[currenFrameIndex].WeightOfBaseVertex);

                    uint srcPos = faceMorphs[baseMorphIndex].FaceVertices[startPosIndex].Index;

                    vertices[srcPos].Position = endPos; 
                    #endregion

                    #region Bone Position2
                    endPos = faceMorphs[endMorphIndex].FaceVertices[j].PositionExtra;
                    //startPosIndex: pos in morph "base" or in all vertices?
                    startPosIndex = faceMorphs[endMorphIndex].FaceVertices[j].Index;
                    startPos = faceMorphs[baseMorphIndex].FaceVertices[startPosIndex].PositionExtra;
                    //or
                    //Vector3 startPos = vertices[startPosIndex].Position;

                    endPos = startPos * (1-currentAnimation.VmdFaceFrames[currenFrameIndex].WeightOfBaseVertex) +
                        endPos * (currentAnimation.VmdFaceFrames[currenFrameIndex].WeightOfBaseVertex);

                    srcPos = faceMorphs[baseMorphIndex].FaceVertices[startPosIndex].Index;

                    vertices[srcPos].PositionExtra = endPos;
                    #endregion

                }

                //#########################find another animation at this time########################################
                if (currentAnimation.VmdFaceFrames[currenFrameIndex].IndexOfFrame ==
                    currentAnimation.VmdFaceFrames[currenFrameIndex - 1].IndexOfFrame)
                {
                    //mouth /eye /etcs
                    //default:currenFrameIndex=currenFrameIndex --
                }
                else
                {
                    break;
                }

            }
            
            
            //Vector3 Pos1 = Transform.TransformVector(vertices[i].Position, boneFinalPos[vertices[i].Bone1]);
            //Vector3 Pos2 = Transform.TransformVector(vertices[i].PositionExtra, boneFinalPos[vertices[i].Bone2]);

            //verticesOnly[i].Position = Vector3.Lerp(Pos1, Pos2, vertices[i].Weight);
        }


        #region Update IKs

        Transform GetWorldTransform(int index)
        {
            if (index < 0)
                //ndex = 0;
                return Transform.Identity;
            //boneLocalTransform[0] = boneLocalPose[0];
            Transform result=boneLocalTransform[index];
            while (BoneParent[index] >= 0)
            {
                result = result*boneLocalTransform[BoneParent[index]];
                index = BoneParent[index];
            }

            return result;
        }
        /// <summary>
        /// extract yaw pitch roll from quaternion
        /// </summary>
        /// <param name="input"></param>
        /// <param name="YRot"></param>
        /// <param name="XRot"></param>
        /// <param name="ZRot"></param>
        /// <returns></returns>
        public bool DecompositeQuaternion(Quaternion input, out float YRot, out float XRot, out float ZRot)
        {
            //�����`���˥������Ҏ��
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //�ޥȥꥯ�������ɤ���
            Matrix rot = Matrix.CreateFromQuaternion(inputQ);
            //��`(X�S�ܤ�λ�ܞ)��ȡ��
            if (rot.M32 > 1 - 1.0e-4 || rot.M32 < -1 + 1.0e-4)
            {//����Х��å��ж�
                XRot = (rot.M32 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = 0; YRot = -(float)Math.Atan2(rot.M21, rot.M11);

                return false;
            }
            XRot = -(float)Math.Asin(rot.M32);
            //�ԥå���ȡ��
            YRot = -(float)Math.Atan2(-rot.M31, rot.M33);
            //��`���ȡ��
            ZRot = -(float)Math.Atan2(-rot.M12, rot.M22);
            return true;
        }

        /// <summary>
        /// limit the bone rotate
        /// </summary>
        /// <param name="bonePos"></param>
        /// <returns></returns>
        Transform  AdjustTotalBoneMove(int boneIndex,IkChain chain,float deltaAngle)
        {
            bool result = false;
            
            for(int i=0;i<chain.IkNodeCount;i++)
            {
                if (tempBoneList[boneIndex].Name == "��Ҥ�" || tempBoneList[boneIndex].Name == "�ҤҤ�")
                    result = true;
            }
            if (!result)
                return boneLocalTransform[boneIndex];//don't change anything

            Matrix boneTrans = boneLocalTransform[boneIndex].ToMatrix();
            Matrix bonePose = boneLocalPose[boneIndex].ToMatrix();
            Matrix moveMat = boneTrans * Matrix.Invert(bonePose);

            //get rotation & translation & ^=___=^
            Vector3 temp, trans;
            Quaternion rot;

            moveMat.Decompose(out temp, out rot, out trans);
            float YRot, XRot, ZRot;
            DecompositeQuaternion(rot, out YRot, out XRot, out ZRot);

            //Watch Out!
            //limit rotation angle
            if (XRot < -MathHelper.PiOver2)
                XRot = -MathHelper.PiOver2;
            else if (XRot > -MathHelper.ToRadians(3f))
                XRot = -MathHelper.ToRadians(3f);

            //XRot=XRot>0?-XRot:XRot;
            //we dont change Yrot and Zrot
            //if (bFirstIteration)
            //{
                //XRot = -Math.Abs(deltaAngle);
            //}
            //if(YRot<0.9f)
            if (!(Math.Abs(YRot) > 1 && Math.Abs(ZRot) > 2))//&& Math.Abs(XRot) < 1 
            {
                YRot = 0.0f;
                ZRot = 0.0f;
            }
            return new Transform(trans, Quaternion.CreateFromYawPitchRoll(YRot, XRot, ZRot)) * boneLocalPose[boneIndex];

        }
        void Solve(Vector3 targetPos, IkChain chain)
        {
            ushort maxIterations = chain.MaxIterationCount;
            Vector3 localTargetPos = Vector3.One;
            Vector3 localEffectorPos = Vector3.Zero;

            //[75,8,7,6]---->6
            //[6,7,8,75]---->6
            Transform IKBase = GetWorldTransform(BoneParent[chain.IkNodes[0]]);
#if true //!WINDOWS_PHONE
            for (int it = 0; it < maxIterations; it++)
#else 
            for (int it = 0; it < 1; it++)
#endif
            {
                //6,7,8---order:8-7-6
                for (int i= chain.IkNodeCount-2; i >=0 ; i--)
                {
                    Transform qTrans = IKBase;
                    for (int j = 0; j <= i; j++)//6->7->8
                    {
                        qTrans = boneLocalTransform[chain.IkNodes[j]] * qTrans;
                    }
                    Transform objectLoc = qTrans;
                    //get new end
                    for (int j = i+1; j < chain.IkNodeCount; j++)//6->7->8
                    {
                        objectLoc = boneLocalTransform[chain.IkNodes[j]] * objectLoc;
                    }

                    Matrix invCoord = Matrix.Invert(qTrans.ToMatrix());
                    //to node i coordinate
                    localEffectorPos = Vector3.Transform(objectLoc.Pos,invCoord);
                    localTargetPos = Vector3.Transform(targetPos, invCoord);
                    //if we reach to the target
                    if ((localEffectorPos - localTargetPos).LengthSquared() < 1.0e-8f)//
                    {
                        return;
                    }
                    Vector3 basis2Effector=Vector3.Normalize(localEffectorPos);
                    Vector3 basis2Target = Vector3.Normalize(localTargetPos);
                    //rotate angle
                    float rotationDotProduct = Vector3.Dot(basis2Effector, basis2Target);
                    float rotationAngle = (float)Math.Acos(rotationDotProduct);

                    //limit the angle TODO:Control weight

                    if (rotationAngle > MathHelper.Pi * chain.MaxAngleBetween)
                        rotationAngle = MathHelper.Pi * chain.MaxAngleBetween;
                    else if (rotationAngle < -MathHelper.Pi * chain.MaxAngleBetween)
                        rotationAngle = -MathHelper.Pi * chain.MaxAngleBetween;

                    if (!float.IsNaN(rotationAngle))
                    {
                        //we get a valid angle
                        Vector3 rotationAxis;
                        rotationAxis = Vector3.Cross(basis2Effector, basis2Target);
                        rotationAxis.Normalize();

                        //fix the bone's rotation
                        if((!float.IsNaN(rotationAxis.X))&&
                            (!float.IsNaN(rotationAxis.Y))&&
                            (!float.IsNaN(rotationAxis.Z)))
                        {
                            boneLocalTransform[chain.IkNodes[i]] = new Transform(new Vector3(0, 0, 0), Quaternion.CreateFromAxisAngle(rotationAxis, rotationAngle)) *
                                boneLocalTransform[chain.IkNodes[i]];
                        }

                        boneLocalTransform[chain.IkNodes[i]] = AdjustTotalBoneMove(chain.IkNodes[i], chain, rotationAngle);
                    }
                }
                if (bFirstIteration)
                    bFirstIteration = false;
            }
        }
        /// <summary>
        /// apply transform to world transform
        /// </summary>
        /// <param name="chain"></param>
        void ApplyTransform(IkChain chain)
        {
            Transform boneBase=GetWorldTransform(BoneParent[chain.IkNodes[0]]);
            for (int i = 0; i < chain.IkNodeCount; i++)
            {
                if(i==0)
                    boneFinalPos[chain.IkNodes[i]] = boneLocalTransform[chain.IkNodes[i]] * boneBase;
                else
                    boneFinalPos[chain.IkNodes[i]] = boneLocalTransform[chain.IkNodes[i]] * boneFinalPos[chain.IkNodes[i-1]]; 

            }
        }

        bool bFirstIteration = true;
        private void UpdateBoneByIK()
        {

            //IKs.Update(ref boneLocalTransform,ref boneLocalPose);
            for (int i = 0; i < IKs.IKChainCount; i++)
            {
                ///////////////////////////part 1
                bFirstIteration = true;
                //chain[6,7,8,75..80]
                //iktarget bone
                Vector3 targetPos = boneLocalTransform[IKs.IkChains[i].IkTarget].Pos;
                //transform target to world
                Matrix targetParentWorld = GetWorldTransform(bonesParent[IKs.IkChains[i].IkTarget]).ToMatrix();
                //transform 
                targetPos = Vector3.Transform(targetPos, targetParentWorld);

                ///////////////////////////part 2
                Solve(targetPos, IKs.IkChains[i]);

                ////////////////////////// part final
                //apply transform to
                ApplyTransform(IKs.IkChains[i]);
            }
        } 
        #endregion

        /// <summary>
        /// update animation
        /// change the customBoneTransformation
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateAnimation(GameTime gameTime)
        {
            if (currentAnimation == null)
                return;
            //according the keyframes in the animation
            //change the customBoneTransformation

            //parameter:0.00005 is adjust for real situation in real world
            g_time += gameTime.ElapsedGameTime.Milliseconds * 0.00005f * currentAnimation.AnimationSpeed;
            //now we have tha animation
            //whether loop the animation
            if(g_time>1.0f)
                if(LoopAnimation)
                    g_time=0.0f;
            //check every bone and find their keyfram according to time
            for (int i = 0; i < bonesNameList.Count; i++)
            {
                //if not exist the current bone
                if (!currentAnimation.VmdAnimationFrames.ContainsKey(bonesNameList[i]))
                {
                    boneLocalTransform[i] = boneLocalPose[i];
                    continue;
                }
                List<VmdKeyframe> frames=currentAnimation.VmdAnimationFrames[bonesNameList[i]];
                if(frames==null)
                    continue;//if we couldn't find the bone
                //find the frame index
                int index=0;
                while ((index < frames.Count) && (frames[index].Time  < g_time))
                        index++;

                if (index == 0)
                {
                    Vector3 internalPos = frames[0].Position;
                    Quaternion internalRot = frames[0].Rotation; ;
                    customBoneTransformation[i] = (new Transform(new Vector3(0, 0, 0), internalRot)) * (new Transform(internalPos, Quaternion.Identity));
                    //customBoneTransformation[i] = new Transform(internalPos, internalRot);
                    //customBoneTransformation[i] = Matrix.CreateFromQuaternion(internalRot) * Matrix.CreateTranslation(internalPos);
                }
                else if (index >= frames.Count)
                {
                    Vector3 internalPos = frames[frames.Count - 1].Position ;
                    Quaternion internalRot = frames[frames.Count - 1].Rotation;
                    customBoneTransformation[i] = (new Transform(new Vector3(0, 0, 0), internalRot)) * (new Transform(internalPos, Quaternion.Identity));
                    //customBoneTransformation[i] = new Transform(internalPos, internalRot);
                    //customBoneTransformation[i] = Matrix.CreateFromQuaternion(internalRot) * Matrix.CreateTranslation(internalPos);
                }
                else
                {
                    //interpolate frame
                    //1 position
                    VmdKeyframe current = frames[index - 1];
                    VmdKeyframe next = frames[index];
                    float ratio = (g_time  - frames[index - 1].Time ) /
                        (frames[index].Time  - frames[index - 1].Time);

                    Vector3 internalPos = current.Position*(1 - ratio) + ratio * next.Position;
                    Quaternion internalRot =  Quaternion.Lerp(current.Rotation, next.Rotation, ratio);
                    //change customBoneTransformation
                    customBoneTransformation[i] = (new Transform(new Vector3(0, 0, 0), internalRot)) * (new Transform(internalPos, Quaternion.Identity));
                    
                    //customBoneTransformation[i] = new Transform(internalPos, internalRot);
                    //customBoneTransformation[i] = Matrix.CreateFromQuaternion(internalRot) * Matrix.CreateTranslation(internalPos);
                }

                boneLocalTransform[i] = customBoneTransformation[i] * boneLocalPose[i];
            }
        }

        /// <summary>
        /// update bones' position
        /// </summary>
        private void UpdateBones()
        {
            //TODO:TEST
            //customBoneTransformation[69] = Matrix.CreateRotationZ(game_time);
            
            for (int i = 0; i < BoneCount; i++)
            {
                boneFinalPos[i] = boneLocalTransform[i];// customBoneTransformation[i] * boneLocalPose[i];
                if (bonesParent[i] >= 0)
                {
                    boneFinalPos[i] *= boneFinalPos[bonesParent[i]];
                }
            }
            //update ik transformation[error]
            //for (int i = 0; i < IKs.IKChainCount; i++)
            //{
            //    for (int j = 1; j < IKs.IkChains[i].IkNodes.Length; j++)
            //    {
            //        boneFinalPos[IKs.IkChains[i].IkNodes[j]] *= boneFinalPos[IKs.IkChains[i].IkNodes[0]];
            //    }
            //}
        }
        /// <summary>
        /// update vertices by bone
        /// </summary>
        private void UpdateBoneVertices()
        {
            for (int i = 0; i < VerticesCount; i++)
            {
                //verticesOnly[i].Position = Vector3.Transform(vertices[i].Position, boneFinalPos[vertices[i].Bone1]);
                
                Vector3 Pos1 = Transform.TransformVector(vertices[i].Position, boneFinalPos[vertices[i].Bone1]);
                Vector3 Pos2 = Transform.TransformVector(vertices[i].PositionExtra, boneFinalPos[vertices[i].Bone2]);

                verticesOnly[i].Position = Vector3.Lerp(Pos1, Pos2, vertices[i].Weight);
            }
            //update face vertex
            
        }
        /// <summary>
        /// ��ͬ�����update
        /// ���render����ֻ�ǽ�������Ƴ�����������Ҫ�ض���Effect
        /// ������WP7��Ҳ�ǲ�֧�ֵ�
        /// WP7��һ�ж�ֻ����cpu����
        /// </summary>
        /// <param name="world"></param>
        /// <param name="view"></param>
        /// <param name="proj"></param>
        public void Render(Matrix world,Matrix view,Matrix proj)
        {
            //���������趨
            effect.World = world;
            effect.View = view;
            effect.Projection = proj;
            //�����û�ж������ã�Ӧ��Ҫdisable��
            effect.EnableDefaultLighting();

            //Miku Model��ͬ������һ���Model������˳���ǵ�������
            device.RasterizerState = RasterizerState.CullClockwise;
            int currentFaceVertexIndex=0;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    //�����������
                    effect.DiffuseColor = materials[i].Diffuse;
                    effect.Alpha = materials[i].Alpha;
                    effect.SpecularPower = materials[i].Shininess;
                    effect.SpecularColor = materials[i].Specular;
                    effect.AmbientLightColor = materials[i].Ambient;
                    //mmd�ܶಿ�ֶ���û������ģ�����ʹ��ambient Color��diffuseColor��ʵ�ֵ�
                    //�ܲ���˼��
                    effect.TextureEnabled = false;
                    if (!string.IsNullOrEmpty(materials[i].Name))
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = Content.Load<Texture2D>(materials[i].Name.Split('.')[0]);
                    }
                    //ÿһ��Material����һ��vertices
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verticesOnly, 0, verticesOnly.Length, indices, currentFaceVertexIndex, materials[i].FaceVertexCount / 3);
                    //�л�����һ��material
                    currentFaceVertexIndex += materials[i].FaceVertexCount;
                }
                
            }
            device.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        Bone FindBone(int i)
        {
            for (int j = 0; j < tempBoneList.Length; j++)
            {
                if (i == tempBoneList[j].BoneNum)
                    return tempBoneList[j];
            }
            throw new Exception("û���ҵ�");
        }
    }
}
