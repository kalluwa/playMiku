using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace PmdModelLib
{
    /// <summary>
    /// 这个类的功能就是告诉XNA
    /// 使用Content.Load<PmdMode>("XXXXX")来读取我们的模型
    /// </summary>
    public class PmdModelReader : ContentTypeReader<PmdModel>
    {
        protected override PmdModel Read(ContentReader input, PmdModel existingInstance)
        {
            //构造函数什么也不做
            PmdModel model = new PmdModel();
            //this is where magic will happen at
            model.Load(input);

            return model;
        }
    }

    /// <summary>
    /// 这个类的功能就是告诉XNA
    /// 使用Content.Load<PmdMode>("XXXXX")来读取我们的模型
    /// </summary>
    public class VmdAnimationReader : ContentTypeReader<VmdAnimation>
    {
        protected override VmdAnimation Read(ContentReader input, VmdAnimation existingInstance)
        {
            //构造函数什么也不做
            VmdAnimation animation = new VmdAnimation();
            //this is where magic will happen at
            animation.Load(input);

            return animation;
        }
    }
    
}
