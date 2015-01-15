using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace PlayMiku
{
    public static class Font
    {
        struct MsgAtPos
        {
            public string msg;
            public int x, y;
        };

        static GraphicsDevice device=null;

        static List<MsgAtPos> Message = new List<MsgAtPos>();

        public static SpriteBatch spriteBatch=null;

        public static SpriteFont font = null;

        public static void Initialze(GraphicsDevice _device, ContentManager Content)
        {
            device = _device;
            spriteBatch = new SpriteBatch(_device);
            font = Content.Load<SpriteFont>("font");
        }
        public static void DrawMessage(string msg,int x,int y)
        {
            MsgAtPos msgPos=new MsgAtPos();
            msgPos.msg = msg;
            msgPos.x=x;
            msgPos.y=y;
            Message.Add(msgPos);
        }

        //draw
        public static void Draw()
        {
            if (spriteBatch == null)
                return;

            spriteBatch.Begin();
 
            foreach (var msg in Message)
	        {
                spriteBatch.DrawString(font, msg.msg, new Vector2(msg.x, msg.y), Color.White);
	        }
            spriteBatch.End();

            device.DepthStencilState = DepthStencilState.Default;
            Message.Clear();
        }
    }
}
