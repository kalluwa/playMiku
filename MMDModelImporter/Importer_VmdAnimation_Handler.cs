using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace MMDModelImporter
{
    /*
     if(!MyFile::Exist(filename))
	{
		std::string errorStr("文件没有找到：/n");
		errorStr.append(filename);
		throw errorStr;
	}
	MyFile file;
	file.Open(filename);
	m_pMotionData=new VmdMotionData;
	file.ReadBuff(m_pMotionData->m_cFlagName,30);
	int Version=atoi(&m_pMotionData->m_cFlagName[21]);//*(int*)&m_pMotionData->m_cFlagName[21];//error ignore
	if(Version!=2)
	{
		throw std::string("VMD版本号不为2.0");
	}
	file.ReadBuff(m_pMotionData->m_cMotionName,20);
	file.Read(&m_pMotionData->m_iMotionCount);
	//allocate memory
	m_pMotionData->m_pBoneMotions=new BoneMotionData[m_pMotionData->m_iMotionCount];
	//测试字符串
	char temp[10];
	int MaxFrame=0;

	int icc=0,icc2=0;
	for(int i=0;i<m_pMotionData->m_iMotionCount;i++)
	{
		//file.Read(&m_pMotionData->m_pBoneMotions[i]);
		file.ReadBuff(&m_pMotionData->m_pBoneMotions[i],sizeof(BoneMotionData)-4);
		for(int j=0;j<m_iNumOfBones;j++)
		{
			if(memcmp(m_pMotionData->m_pBoneMotions[i].m_cBoneName,
				m_pBones[j].m_cName,14)==0)
			{
				m_pMotionData->m_pBoneMotions[i].m_iBoneNum=j;
				
				//test
				if(MaxFrame<m_pMotionData->m_pBoneMotions[i].m_uiFrameNum)
					MaxFrame=m_pMotionData->m_pBoneMotions[i].m_uiFrameNum;
				//keyframe数据写入Bone中
				icc++;
				//1:准备数据
				VmdKeyFrame* keyframe=new VmdKeyFrame;
				keyframe->m_iFrameNo=m_pMotionData->m_pBoneMotions[i].m_uiFrameNum;
				keyframe->m_vAngle=m_pMotionData->m_pBoneMotions[i].m_qRot.GetEulers();
				keyframe->m_vPos=m_pMotionData->m_pBoneMotions[i].m_vPos;
				//2：添加到正确的骨骼中（还没有排序）
				m_pBones[j].m_pKeyframes.push_back(keyframe);
				break;
			}
			else
			{
				icc2++;
			}
		}
		
	}
	char tempstr[20];
			sprintf(tempstr,"%i  %i\n",icc,icc2);
			OutputDebugStringA(tempstr);
	m_fAnimationTime=MaxFrame*FPS;
	for(int i=0;i<m_iNumOfBones;i++)
	{
		VmdKeyFrame* temp;
		//冒泡排序帧FrameNo
		for(int j=0;j<(int)m_pBones[i].m_pKeyframes.size();j++)
		{
			for(int k=j+1;k<(int)m_pBones[i].m_pKeyframes.size();k++)
			{
					//如果排在前面的帧号大于排在后面的，将其交换
				if(m_pBones[i].m_pKeyframes[j]->m_iFrameNo>
					m_pBones[i].m_pKeyframes[k]->m_iFrameNo)
				{
					temp=m_pBones[i].m_pKeyframes[j];
					m_pBones[i].m_pKeyframes[j]=m_pBones[i].m_pKeyframes[k];
					m_pBones[i].m_pKeyframes[k]=temp;
				}
			}
		}
	}
	//TODO:delete
	//delete []m_pMotionData;
	file.Close();

	for(unsigned short i=0;i<m_iNumOfBones;i++)
	{
		if(m_pBones[i].m_sParent!=-1)
		{
			char temp[20];
			sprintf(temp,"%i->%i\t%i",i,m_pBones[i].m_sParent,m_pBones[i].m_sIkNum);
			if(m_pBones[i].m_sIkNum!=0)
				os<<temp<<endl;
			//OutputDebugStringA(temp);
		}
	}
     */
    #region Data structure
    
    struct BoneMotionData
    {
        /// <summary>
        /// 15 bytes
        /// </summary>
	    public string       m_cBoneName;
	    public uint         m_uiFrameNum;
	    public Vector3      m_vPos;
	    public Quaternion   m_qRot;
	    public byte[]       m_bInterportation;//64bytes未知，不作处理
	    //public int          m_iBoneNum;
    };

    class FaceMotionFrame:IComparable
    {
        public string m_cName;//15bytes
        public uint m_bIndexOfFrame;//4bytes index of this frame
        public float m_bWeight;//4bytes 0.0-1.0

        //sort this by compare
        public int CompareTo(object obj)
        {
            FaceMotionFrame keyframe = obj as FaceMotionFrame;
            if (obj == null)
                throw new ArgumentException("Object is not a FaceMotionFrame.");
            return m_bIndexOfFrame.CompareTo(keyframe.m_bIndexOfFrame);
        }
    };
    class Keyframe : IComparable
    {
        public string     m_strBoneName;
        public uint       m_uiFrameNum;
        public Vector3    m_vPos;
        public Quaternion m_qRot;

        //sort this by compare
        public int CompareTo(object obj)
        {
            Keyframe keyframe = obj as Keyframe;
            if (obj == null)
                throw new ArgumentException("Object is not a Keyframe.");
            return m_uiFrameNum.CompareTo(keyframe.m_uiFrameNum);
        }
    };
    /// <summary>
    /// 动作数据
    /// </summary>
    struct VmdMotionData
    {
	    public string m_cFlagName;//=new byte[30];
	    public string m_cMotionName;//=new byte[20];
	    public ulong m_iMotionCount;
	    public BoneMotionData[] m_pBoneMotions;
	    //对于之后的FaceMotion,CameraMotion等等，暂不作处理
        public FaceMotionFrame[] m_pFaceMotions;
    };
	#endregion

    class Importer_VmdAnimation
    {
        #region Variables
        //Data
        VmdMotionData vmdData = new VmdMotionData();
        List<Keyframe>[] keyframes;
        List<string> boneNames; 
        //helper
        uint maxFrameNumber = 0;
        #endregion
        /// <summary>
        /// read vmd animation data
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            //1flagName
            vmdData.m_cFlagName=GlobleFuncs.GetString(reader,30);
            //check version
            int version=int.Parse(vmdData.m_cFlagName.Substring(20));
            if(version!=2)
            {
                throw new InvalidOperationException("VMD版本号不为2.0");
            }
            //2motionName
            vmdData.m_cMotionName=GlobleFuncs.GetString(reader,20);
            //3motionCount[TODO:ulong==int64]
            vmdData.m_iMotionCount=reader.ReadUInt32();

            //4 it's keyframes now
            vmdData.m_pBoneMotions=new BoneMotionData[vmdData.m_iMotionCount];
            //sorting the frame
            boneNames = new List<string>();
            
            for (uint i = 0; i < vmdData.m_iMotionCount; i++)
			{
			    BoneMotionData boneData=vmdData.m_pBoneMotions[i];
                //fill the keyframe data
                boneData.m_cBoneName=GlobleFuncs.GetString(reader,15);
                if (!boneNames.Contains(boneData.m_cBoneName))
                {
                    boneNames.Add(boneData.m_cBoneName);
                }
                boneData.m_uiFrameNum=reader.ReadUInt32();
                //position
                boneData.m_vPos.X=reader.ReadSingle();
                boneData.m_vPos.Y=reader.ReadSingle();
                boneData.m_vPos.Z=reader.ReadSingle();
                //quaternion
                boneData.m_qRot.X=reader.ReadSingle();
                boneData.m_qRot.Y=reader.ReadSingle();
                boneData.m_qRot.Z=reader.ReadSingle();
                boneData.m_qRot.W=reader.ReadSingle();
                //64 bytes
                boneData.m_bInterportation=new byte[64];
                boneData.m_bInterportation=reader.ReadBytes(64);
                //relative bone index
                //boneData.m_iBoneNum=reader.ReadInt16();

                vmdData.m_pBoneMotions[i] = boneData;
			}
            

            #region Keyframe in Bone Movement
            //arrange the bone keyframe
            keyframes = new List<Keyframe>[boneNames.Count];
            for (int i = 0; i < boneNames.Count; i++)
            {
                keyframes[i] = new List<Keyframe>();
            }
            for (uint i = 0; i < vmdData.m_iMotionCount; i++)
            {
                BoneMotionData boneData = vmdData.m_pBoneMotions[i];
                Keyframe frame = new Keyframe();
                frame.m_qRot = boneData.m_qRot;
                frame.m_uiFrameNum = boneData.m_uiFrameNum;

                //get the max frame number to calculate time
                if (frame.m_uiFrameNum > maxFrameNumber)
                    maxFrameNumber = frame.m_uiFrameNum;
                frame.m_vPos = boneData.m_vPos;
                frame.m_strBoneName = boneData.m_cBoneName;
                //add this frame
                keyframes[boneNames.IndexOf(boneData.m_cBoneName)].Add(frame); ;
            }
            //sort the frames by its frameNumber
            for (int i = 0; i < keyframes.Length; i++)
            {
                keyframes[i].Sort();
            } 
            #endregion


            //###################face motion###################
            #region everything for face motion
            uint nFaceMotionCount = reader.ReadUInt32();
            vmdData.m_pFaceMotions = new FaceMotionFrame[nFaceMotionCount];
            List<FaceMotionFrame> faceMotionFrames = new List<FaceMotionFrame>();
            for (int i = 0; i < nFaceMotionCount; i++)
            {
                FaceMotionFrame faceData = new FaceMotionFrame();
                //15char
                faceData.m_cName = GlobleFuncs.GetString(reader, 15);
                //index of frame
                faceData.m_bIndexOfFrame = reader.ReadUInt32();
                //weight of [base] vertex
                faceData.m_bWeight = reader.ReadSingle();

                faceMotionFrames.Add(faceData);
                //vmdData.m_pFaceMotions[i] = faceData;
            }
            faceMotionFrames.Sort();
            vmdData.m_pFaceMotions = faceMotionFrames.ToArray();
            #endregion
        }
        /// <summary>
        /// output:
        /// first for animation movement
        /// bone name/index,Frames Count,Frames;
        /// bone name/index,Frames Count,Frames
        /// ...
        /// ...
        /// second for
        /// </summary>
        /// <param name="output"></param>
        public void Write(ContentWriter output)
        {
            //the speed of animation,the time is [0,10]s
            output.Write(maxFrameNumber * 0.001f);

            float invMaxFrameNumber=1.0f/maxFrameNumber;
            //write action frames
            #region Movement

            //write bone count
            output.Write(keyframes.Length);
            for (int i = 0; i < keyframes.Length; i++)
            {
                List<Keyframe> framesOfSingleBone=keyframes[i];
                //write single frame data here
                
                //1 name of this bone
                output.Write(boneNames[i]);
                //2 frames count of this bone
                output.Write(framesOfSingleBone.Count);
                //3 data[all frames of this bone]
                for (int j = 0; j < framesOfSingleBone.Count; j++)
                {
                    //we don't need frame number anymore for them being sorted
                    Keyframe currentFrame=framesOfSingleBone[j];
                    //time:frame index
                    //saturate the time to (0,1)
                    output.Write(currentFrame.m_uiFrameNum * invMaxFrameNumber);
                    //frame bone pos
                    output.Write(currentFrame.m_vPos);
                    //frame bone rotation
                    output.Write(currentFrame.m_qRot);
                }
            }
            #endregion

            #region Face Movement

            //face count
            output.Write(vmdData.m_pFaceMotions.Length);

            for (int i = 0; i < vmdData.m_pFaceMotions.Length; i++)
            {
                FaceMotionFrame frame = vmdData.m_pFaceMotions[i];
                //name
                output.Write(frame.m_cName);
                //indexofframe [use as time]
                output.Write((float)(frame.m_bIndexOfFrame * invMaxFrameNumber));
                //weight of base vertex
                output.Write(frame.m_bWeight);
            } 
            #endregion

        }
    }
}
