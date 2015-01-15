using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using PmdModelLib;

namespace PlayMiku
{
#if WINDOWS
    class BoneHelper
    {
        #region Variables
        //draw device
        GraphicsDevice device;
        //besiceffect
        BasicEffect be;
        //load ball
        ContentManager content;
        //sphere
        Model m_sphereModel;

        #endregion
        #region Constructor

        public BoneHelper(GraphicsDevice _device,ContentManager _content)
        {
            device = _device;
            content = _content;
            //load ball
            m_sphereModel = content.Load<Model>("Models/sphere");
            //effect
            be = new BasicEffect(device);
        }
        #endregion

        #region Line Helper
        //line
        VertexPositionColor[] vertices = new VertexPositionColor[]
        {
            new VertexPositionColor(new Vector3(0,0,0),Color.White),
            new VertexPositionColor(new Vector3(1,1,1),Color.White)
        };
        //draw line between nodes
        public void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            //if device is not ready
            if (device == null)
                return;
            be.World = Matrix.Identity;
            be.View = Game1.View;
            be.Projection = Game1.Projection;
            //set vertices
            vertices[0].Position = start;
            vertices[0].Color = color;
            vertices[1].Color = color;
            vertices[1].Position = end;

            //draw them
            be.DiffuseColor = color.ToVector3();
            be.CurrentTechnique.Passes[0].Apply();
            device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public void DrawLine(Vector3 start, Vector3 end)
        {
            //use default color[white]
            DrawLine(start, end, Color.White);
        }
        #endregion

        #region DrawSphere
        /// <summary>
        /// draw a ball at a given position
        /// </summary>
        /// <param name="Pos"></param>
        public void DrawSphere(Vector3 Pos,Color color)
        {
            foreach (ModelMesh mesh in m_sphereModel.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.World =Matrix.CreateScale(0.3f)* Matrix.CreateTranslation(Pos);
                    be.View = Game1.View;
                    be.Projection = Game1.Projection;
                    be.DiffuseColor = color.ToVector3(); ;
                }
                mesh.Draw();
            }
        }
        /// <summary>
        /// another function to draw sphere
        /// </summary>
        /// <param name="Pos"></param>
        public void DrawSphere(Vector3 Pos)
        {
            DrawSphere(Pos, Color.Gray);
        }
        #endregion

        #region Draw Bones
        /// <summary>
        /// draw bones
        /// </summary>
        /// <param name="boneFinalPos"></param>
        /// <param name="bonesParent"></param>
        public void DrawBones(Transform[] boneFinalPos, int[] bonesParent)
        {
            //disable zbuffer
            device.DepthStencilState = DepthStencilState.None;

            int BoneCount = boneFinalPos.Length;
            for (int i = 0; i < BoneCount; i++)
            {
                //draw ball
                DrawSphere(boneFinalPos[i].Pos);
                //if it has a parent
                if (bonesParent[i] >= 0)
                    DrawLine(boneFinalPos[i].Pos, boneFinalPos[bonesParent[i]].Pos);
            }

            //enable zbuffer test
            device.DepthStencilState = DepthStencilState.Default;

        }

        /// <summary>
        /// a helper to show ik node
        /// NOTE: IK type ==4 means a array of nodes
        /// type==2 means target to reach
        /// </summary>
        /// 
        /// <param name="boneFinalPos"></param>
        /// <param name="bones"></param>
        internal void DrawIK(Transform[] boneFinalPos, PmdModelLib.Bone[] bones,int types,Color color)
        {
            //disable zbuffer
            device.DepthStencilState = DepthStencilState.None;

            int BoneCount = boneFinalPos.Length;
            for (int i = 0; i < BoneCount; i++)
            {
                if (bones[i].Kind != types)
                    continue;
                //draw ball
                DrawSphere(boneFinalPos[i].Pos, color);
                //if it has a child
                if (bones[i].To>= 0)
                    DrawLine(boneFinalPos[i].Pos, boneFinalPos[bones[i].To].Pos, color);
            }

            //enable zbuffer test
            device.DepthStencilState = DepthStencilState.Default;
        }

        internal void DrawIK(Transform[] boneFinalPos, PmdModelLib.Bone[] bones)
        {
            DrawIK(boneFinalPos, bones, 4,Color.Red);
        }

        internal void DrawSingleIK(Transform[] boneFinalPos, PmdModelLib.Bone[] bones, int index, Color color)
        {
            //disable zbuffer
            device.DepthStencilState = DepthStencilState.None;

            int BoneCount = boneFinalPos.Length;
            for (int i = index; i < BoneCount; i++)
            {
                //draw ball
                DrawSphere(boneFinalPos[i].Pos, color);
                //if it has a child
                if (bones[i].To >= 0)
                    DrawLine(boneFinalPos[i].Pos, boneFinalPos[bones[i].To].Pos, color);

                break;
            }

            //enable zbuffer test
            device.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// draw sigle ik Chain
        /// </summary>
        /// <param name="globalPmdModelLibIkChain"></param>
        /// <param name="color"></param>
        internal void DrawIKChain(Transform[] boneFinalPos,IkChain singleIkChain, Color color)
        {
            //disable zbuffer
            device.DepthStencilState = DepthStencilState.None;

            int BoneCount = singleIkChain.IkNodeCount;
            for (int i = 0; i < BoneCount; i++)
            {
                //draw ball
                DrawSphere(boneFinalPos[singleIkChain.IkNodes[i]].Pos, color);
                //if it has a child
                if (i < BoneCount - 1)
                {
                    DrawLine(boneFinalPos[singleIkChain.IkNodes[i]].Pos,
                        boneFinalPos[singleIkChain.IkNodes[i + 1]].Pos, color);
                }
                else
                {
                    //DrawLine(boneFinalPos[singleIkChain.IkNodes[i]].Translation,
                        //boneFinalPos[singleIkChain.IkTarget].Translation, color);
                }

            }
            DrawSphere(boneFinalPos[singleIkChain.IkTarget].Pos, Color.Black);
            //enable zbuffer test
            device.DepthStencilState = DepthStencilState.Default;
        }
        #endregion





        #region Show Ik Chain by Transform

        internal void DrawIKChain(Transform[] boneFinalPos, Transform[] boneLocalBone, IkChain singleIkChain, Color color)
        {
            //disable zbuffer
            device.DepthStencilState = DepthStencilState.None;

            int BoneCount = singleIkChain.IkNodeCount;
            for (int i = 0; i < BoneCount; i++)
            {
                Vector3 dir = new Vector3(0, 0, 1);
                //draw ball
                DrawSphere(boneFinalPos[singleIkChain.IkNodes[i]].Pos, color);
                //if it has a child
                if (i < BoneCount - 1)
                {
                    //dir = boneLocalBone[i + 1].Pos - boneLocalBone[i].Pos;

                    DrawLine(boneFinalPos[singleIkChain.IkNodes[i]].Pos,
                        boneFinalPos[singleIkChain.IkNodes[i + 1]].Pos, color);

                    //dir
                    dir=Vector3.Transform(dir, boneFinalPos[singleIkChain.IkNodes[i]].Rot);
                    DrawLine(boneFinalPos[singleIkChain.IkNodes[i]].Pos,
                        boneFinalPos[singleIkChain.IkNodes[i]].Pos+dir, Color.Red);
                }
                else
                {
                    //DrawLine(boneFinalPos[singleIkChain.IkNodes[i]].Translation,
                    //boneFinalPos[singleIkChain.IkTarget].Translation, color);
                }

            }
            DrawSphere(boneFinalPos[singleIkChain.IkTarget].Pos, Color.Black);
            //enable zbuffer test
            device.DepthStencilState = DepthStencilState.Default;
        }
 
        #endregion
    }
#endif
}
