using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

//this is MMD importer
namespace MMDModelImporter
{
    #region PMD Importer
		
    #region how we organize the pmd model data

    public static class GlobleFuncs
    {
        static Encoding encoding = Encoding.GetEncoding("shift-jis");
        public static string GetString(BinaryReader reader, int numBytes)
        {
            byte[] bytes = reader.ReadBytes(numBytes);
            int i = 0;
            for (; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    break;
            }
            if (i < bytes.Length)
                return encoding.GetString(bytes, 0, i);

            return encoding.GetString(bytes);

        }
        public static Vector3 ReadVector3(BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static Vector2 ReadVector2(BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(),
                reader.ReadSingle());
        }
    }
    struct PMD_Header
    {
        public string PMDKey;  // 始终为Pmd
        public int Version;     // 版本
        public string ModelName;   // 模型名称
        public string Comment;    //256 模型说明
        public void Read(BinaryReader reader)
        {
            PMDKey = GlobleFuncs.GetString(reader, 3);
            Version = reader.ReadInt32();
            ModelName = GlobleFuncs.GetString(reader, 20);
            Comment = GlobleFuncs.GetString(reader, 256);
        }
    };
    struct PMD_Vertex // 42字节
    {
        public Vector3 Position; // 顶点坐标
        public Vector3 Normal;   // 顶点法线
        public Vector2 UV;   // 顶点贴图坐标
        public ushort Bone1;
        public ushort Bone2;// 骨骼索引（才俩个骨骼啊 - -）
        public byte Weight; //权重
        public byte NonEdgeFlag;    //无边标志（说明：当这个字节不为0时NonEdgeFlag=true）
        public void Read(BinaryReader reader)
        {
            Position = GlobleFuncs.ReadVector3(reader);
            Normal = GlobleFuncs.ReadVector3(reader);
            UV = GlobleFuncs.ReadVector2(reader);
            Bone1 = reader.ReadUInt16();
            Bone2 = reader.ReadUInt16();
            Weight = reader.ReadByte();
            NonEdgeFlag = reader.ReadByte();
        }

        internal void Write(ContentWriter output)
        {
            output.Write(Position);
            output.Write(Normal);
            output.Write(UV);
            output.Write(Bone1);
            output.Write(Bone2);
            output.Write(Weight);
        }
    };
    struct PMD_Material
    {
        public Vector3 Diffuse;  // 漫射光，包含alpha通道（Vector4.w代表）
        public float Alpha;
        public float Shininess;    // 发光度
        public Vector3 Specular;     // 镜面光
        public Vector3 Ambient;      // 环境光
        public byte ToonNo;  // 有符号，卡通着色纹理编号
        public byte Edge;   // 是否带边，当该字节>0时为true
        public int FaceVertexCount;  // 面顶点数
        public string Name;  // 包含了纹理路径，使用*分割（应该包含了混合模式吧，枚举：NONE、ADD、MUL）
        public void Read(BinaryReader reader)
        {
            Diffuse = GlobleFuncs.ReadVector3(reader);
            Alpha = reader.ReadSingle();
            Shininess = reader.ReadSingle();
            Specular = GlobleFuncs.ReadVector3(reader);
            Ambient = GlobleFuncs.ReadVector3(reader);
            ToonNo = reader.ReadByte();
            Edge = reader.ReadByte();
            FaceVertexCount = reader.ReadInt32();
            Name = GlobleFuncs.GetString(reader, 20);
            //暂时不处理sph纹理
            if (Name.EndsWith("sph"))
                Name = "";
            //包含了纹理路径，使用*分割（应该包含了混合模式吧，枚举：NONE、ADD、MUL）
            if (Name.EndsWith("spa"))
                Name = Name.Split('*')[0];
        }

        internal void Write(ContentWriter output)
        {
            output.Write(Diffuse);
            output.Write(Alpha);
            output.Write(Shininess);
            output.Write(Specular);
            output.Write(Ambient);
            output.Write(ToonNo);
            output.Write(Edge);
            output.Write(FaceVertexCount);
            output.Write(Name);
        }
    };
    struct PMD_Bone
    {
        public string Name;    // 骨骼名字
        public short Parent;     // 父亲骨骼
        public short To;         // 连接到
        public byte Kind;       // 类型（枚举：Rotate,RotateMove,IK, Unknown, IKLink, RotateEffect, IKTo, Unvisible, Twist,RotateRatio）
        public short IKNum;      // IK（反向运动学）数值
        public Vector3 Position; // 骨骼位置
        public void Read(BinaryReader reader)
        {
            Name = GlobleFuncs.GetString(reader, 20);
            Parent = reader.ReadInt16();
            To = reader.ReadInt16();
            Kind = reader.ReadByte();
            IKNum = reader.ReadInt16();
            Position = GlobleFuncs.ReadVector3(reader);
        }

        internal void Write(ContentWriter output)
        {
            output.Write(Name);
            output.Write(Parent);
            output.Write(To);
            output.Write(Position);
            /*****IK:added********/
            //TODO:RISK[there is a possibility:parent and to are not the same of what we got]
            output.Write(IKNum);
            output.Write(Kind);//IK:this value is 4
            /********************/
        }
    };
    struct PMD_IK
    {
        /// <summary>
        /// this is a bone we will reach
        /// </summary>
        public ushort IKTarget;
        /// <summary>
        /// and the end of chain IKNodes
        /// </summary>
        public ushort shortTarget;
        /// <summary>
        /// how many nodes of this chain
        /// </summary>
        public byte LinkCount;
        /// <summary>
        /// this is max iteration count:like 
        /// if(iteration>=100||we get a good result)
        /// {
        ///     break;
        /// }
        /// </summary>
        public ushort LoopCount;
        /// <summary>
        /// max angle between neighbor bones
        /// </summary>
        public float LimitOnce;
        /// <summary>
        /// this is our IKNodes
        /// </summary>
        public ushort[] LinkList;

        /// <summary>
        /// read pmd file to get ik information
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            IKTarget = reader.ReadUInt16();
            shortTarget = reader.ReadUInt16();
            LinkCount = reader.ReadByte();
            LoopCount = reader.ReadUInt16();
            LimitOnce = reader.ReadSingle();
            LinkList = new ushort[LinkCount];
            for (int i = 0; i < LinkCount; i++)
            {
                LinkList[i] = reader.ReadUInt16();
            }
        }
        /// <summary>
        /// what we will do is just write whole information to xnb file
        /// </summary>
        /// <param name="output"></param>
        public void Write(ContentWriter output)
        {
            output.Write(IKTarget);
            output.Write(shortTarget);
            output.Write(LinkCount);
            output.Write(LoopCount);
            output.Write(LimitOnce);
            for (int i = 0; i < LinkCount; i++)
            {
                output.Write(LinkList[i]);
            }
        }
    };
    struct SkinVertex
    {
        public uint Index;  // 皮肤对应的组成顶点
        public Vector3 Offset; // 顶点位移
        public void Read(BinaryReader reader)
        {
            Index = reader.ReadUInt32();
            Offset = GlobleFuncs.ReadVector3(reader);
        }

        public void Write(ContentWriter writer)
        {
            //index means another vertex in'base' morph
            writer.Write(Index);
            //pos of this vertex
            writer.Write(Offset);
        }
    };
    struct PMD_Skin
    {
        public string Name;    // 皮肤名字
        public uint VertexListCount; // 皮肤含有的顶点数量
        public byte Category;// 分类
        public SkinVertex[] VertexList;//=new SkinVertex[VertexListCount];  // 皮肤包含的所有顶点信息
        public void Read(BinaryReader reader)
        {
            Name = GlobleFuncs.GetString(reader, 20);
            VertexListCount = reader.ReadUInt32();
            Category = reader.ReadByte();
            VertexList = new SkinVertex[VertexListCount];
            for (int i = 0; i < VertexListCount; i++)
            {
                VertexList[i].Read(reader);
            }
        }
        /// <summary>
        /// writer for face morph
        /// </summary>
        /// <param name="writer"></param>
        public void Write(ContentWriter writer)
        {
            //name of morph[|base|]
            writer.Write(Name);
            //verts count
            writer.Write(VertexListCount);
            //we ignore the vertex type
            //this should be used in editor

            //all verts belong to this morph
            for (int i = 0; i < VertexListCount; i++)
            {
                VertexList[i].Write(writer);
            }
        }
    };
    struct NodeName
    {
        string Name;
        public void Read(BinaryReader reader)
        {
            Name = GlobleFuncs.GetString(reader, 50);
        }
    };
    struct BoneToNode
    {
        public ushort Bone;
        public byte Node;
        public void Read(BinaryReader reader)
        {
            Bone = reader.ReadUInt16();
            Node = reader.ReadByte();
        }
    };
    struct PMD_FrameWindow
    {
        public byte ExpressionListCount;
        public short[] ExpressionList;//ExpressionListCount// 读取后减1，对应皮肤数量
        public byte NodeNameCount;
        public NodeName[] NodeNameList;
        public ushort BoneToNodeCount;
        public BoneToNode[] BoneToNodeList;
        public void Read(BinaryReader reader)
        {
            ExpressionListCount = reader.ReadByte();
            ExpressionList = new short[ExpressionListCount];
            for (int i = 0; i < ExpressionListCount; i++)
            {
                ExpressionList[i] = reader.ReadInt16();
            }
            NodeNameCount = reader.ReadByte();
            NodeNameList = new NodeName[NodeNameCount];
            for (int i = 0; i < NodeNameCount; i++)
            {
                NodeNameList[i].Read(reader);
            }
            BoneToNodeCount = reader.ReadUInt16();
            BoneToNodeList = new BoneToNode[BoneToNodeCount];
            for (int i = 0; i < NodeNameCount; i++)
            {
                BoneToNodeList[i].Read(reader);
            }
        }
    };
    struct PMD_Body
    {
        public string Name;//20
        public short Bone;
        public byte Group;
        public ushort m_passGroupFlag;
        public byte BoxType;    //碰撞盒类型（枚举：Sphere,Box,Capsule）
        public Vector3 BoxSize;
        public Vector3 PositionFromBone;
        public Vector3 Rotation;
        public float Mass;
        public float PositionDamping;
        public float RotationDamping;
        public float Restitution;
        public float Friction;
        public byte Mode;
        public void Read(BinaryReader reader)
        {
            Name = GlobleFuncs.GetString(reader, 20);
            Bone = reader.ReadInt16();
            Group = reader.ReadByte();
            m_passGroupFlag = reader.ReadUInt16();
            BoxType = reader.ReadByte();
            BoxSize = GlobleFuncs.ReadVector3(reader);
            PositionFromBone = GlobleFuncs.ReadVector3(reader);
            Rotation = GlobleFuncs.ReadVector3(reader);
            Mass = reader.ReadSingle();
            PositionDamping = reader.ReadSingle();
            RotationDamping = reader.ReadSingle();
            Restitution = reader.ReadSingle();
            Friction = reader.ReadSingle();
            Mode = reader.ReadByte();

        }
    };
    struct PMD_Joint
    {
        public string Name;//20
        public int BodyA;
        public int BodyB;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Limit_MoveLow;
        public Vector3 Limit_MoveHigh;
        public Vector3 Limit_AngleLow;
        public Vector3 Limit_AngleHigh;
        public Vector3 SpConst_Move;
        public Vector3 SpConst_Rotate;
        public void Read(BinaryReader reader)
        {
            Name = GlobleFuncs.GetString(reader, 20);
            BodyA = reader.ReadInt32();
            BodyB = reader.ReadInt32();
            Position = GlobleFuncs.ReadVector3(reader);
            Rotation = GlobleFuncs.ReadVector3(reader);
            Limit_MoveLow = GlobleFuncs.ReadVector3(reader);
            Limit_MoveHigh = GlobleFuncs.ReadVector3(reader);
            Limit_AngleLow = GlobleFuncs.ReadVector3(reader);
            Limit_AngleHigh = GlobleFuncs.ReadVector3(reader);
            SpConst_Move = GlobleFuncs.ReadVector3(reader);
            SpConst_Rotate = GlobleFuncs.ReadVector3(reader);
        }
    };

    #endregion


    /// <summary>
    /// import PMD's structure
    /// </summary>
    class Importer_PmdModel
    {
        public PMD_Header header;    // 文件头
        public uint VertexCount;     // 顶点个数
        public PMD_Vertex[] VertexList;       // 顶点表
        public uint FaceCount;       // 面（三角形）个数
        public ushort[] FaceList;       // 面表
        public uint MaterialCount;   // 材质个数
        public PMD_Material[] MaterialList;     // 材质表
        public ushort BoneCount;     // 骨骼个数
        public PMD_Bone[] BoneList;     // 骨骼列表
        public ushort IKCount;       // IK链个数
        public PMD_IK[] IKList;       // IK表
        public ushort SkinCount;     // 皮肤数
        public PMD_Skin[] SkinList; // 皮肤列表
        public PMD_FrameWindow FrameWindow;  // ？
        // 附加信息段，如果发现到这里为止文件没有读完，那么继续
        public char flag;    // 如果不为0，说明包含英文信息
        // 下面的信息仅当flag!=0时有效
        //{
        public string ModelNameE;
        public string CommentE;
        public string[] NameE1;
        public string[] NameE2;
        public string[] NameE3;
        //}
        public string[] ToonNames;//=new string[10];
        // 附加信息段，如果发现到这里为止文件没有读完，那么继续
        public uint BodyCount;
        public PMD_Body[] BodyList;
        public uint JointCount;
        public PMD_Joint[] JointList;
        List<ExternalReference<TextureContent>> textures;


        /// <summary>
        /// read function
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            header = new PMD_Header();
            header.Read(reader);

            VertexCount = reader.ReadUInt32();
            VertexList = new PMD_Vertex[VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                VertexList[i].Read(reader);
            }

            FaceCount = reader.ReadUInt32();
            FaceList = new ushort[FaceCount];
            for (int i = 0; i < FaceCount; i++)
            {
                FaceList[i] = reader.ReadUInt16();
            }

            MaterialCount = reader.ReadUInt32();
            MaterialList = new PMD_Material[MaterialCount];

            textures = new List<ExternalReference<TextureContent>>();
            for (int i = 0; i < MaterialCount; i++)
            {
                MaterialList[i].Read(reader);
                if (!string.IsNullOrEmpty(MaterialList[i].Name))
                {
                    //string fileName = Path.GetFullPath(MaterialList[i].Name) ;
                    //if (!File.Exists(fileName))
                    //{ 
                    //    fileName=Path.GetDirectoryName(MaterialList[i].Name)+"\\Textures\\"+Path.get
                    //}
                    //if(!MaterialList[i].Name.EndsWith("sph"))
                    textures.Add(new ExternalReference<TextureContent>
                        (MaterialList[i].Name, new ContentIdentity(MaterialList[i].Name)));

                }
            }

            BoneCount = reader.ReadUInt16();
            BoneList = new PMD_Bone[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                BoneList[i].Read(reader);
            }

            IKCount = reader.ReadUInt16();
            IKList = new PMD_IK[IKCount];
            for (int i = 0; i < IKCount; i++)
            {
                IKList[i].Read(reader);
            }

            //get face morph information
            SkinCount = reader.ReadUInt16();
            SkinList = new PMD_Skin[SkinCount];
            for (int i = 0; i < SkinCount; i++)
            {
                SkinList[i].Read(reader);
            }

            FrameWindow.Read(reader);

            //剩下的的暂时不管
        }

        //build default texture accompany
        public void BuildTextures(ContentProcessorContext context)
        {
            //build texture.xnb files
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i] = context.BuildAsset<TextureContent, TextureContent>(textures[i], "TextureProcessor");
            }
        }
        //write data to xnb file
        //根据需要暂时只用到了vertices，material，bone信息
        //所以将他们写出
        public void Write(ContentWriter output)
        {

            #region Vertices And Triangles
            //write vertice to xnb data

            //count
            output.Write(VertexCount);
            //positions
            for (int i = 0; i < VertexCount; i++)
            {
                VertexList[i].Write(output);
            }

            //triangle count
            output.Write(FaceCount);
            //and their indices
            for (int i = 0; i < FaceCount; i++)
            {
                output.Write(FaceList[i]);
            }

            #endregion


            #region Materials

            //material;s count
            output.Write(MaterialCount);

            //material information
            for (int i = 0; i < MaterialCount; i++)
            {
                MaterialList[i].Write(output);
            }
            #endregion

            #region Bone
            //bones count
            output.Write(BoneCount);
            //bone information
            for (int i = 0; i < BoneList.Length; i++)
            {
                BoneList[i].Write(output);
            }
            #endregion

            #region Ik Nodes

            //ik count
            output.Write(IKCount);
            for (int i = 0; i < IKCount; i++)
            {
                IKList[i].Write(output);
            }
            #endregion

            #region write face morph
            //how many morph we have
            output.Write(SkinCount);
            //write every morph information
            for (int i = 0; i < SkinCount; i++)
            {
                //write [morph Name]
                //write [verts count]
                //write [evry vertex:[index in base]+[vertexPos]]
                SkinList[i].Write(output);
            }

            #endregion
        }


    } 
	#endregion
}
