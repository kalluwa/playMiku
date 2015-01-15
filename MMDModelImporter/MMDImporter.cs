using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace MMDModelImporter
{
    /// <summary>
    /// 读取pmd文件，并将读取之后的数据进行输出
    /// </summary>
    [ContentImporter(".pmd", DisplayName = "PmdImporter")]
    class PmdImporter : ContentImporter<Importer_PmdModel>
    {
        /// <summary>
        /// 读取pmd文件
        /// </summary>
        /// <param name="filename">Pmd 文件名</param>
        /// <param name="context">不用管</param>
        /// <returns></returns>
        public override Importer_PmdModel Import(string filename, ContentImporterContext context)
        {

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
            {
                Importer_PmdModel pmd = new Importer_PmdModel();
                //thia is what magic happened
                pmd.Read(reader);

                return pmd;
            }
        }
    }

    /// <summary>
    /// 这是我们的processor，其实并不做什么，只是将数据进行输出一下，走一个过程
    /// </summary>
    [ContentProcessor(DisplayName = "PmdProcessor")]
    class PMDProcessor : ContentProcessor<Importer_PmdModel, Importer_PmdModel>
    {
        //process the texture
        public override Importer_PmdModel Process(Importer_PmdModel input, ContentProcessorContext context)
        {
            //也可以直接返回input，如果不想管理纹理的话
            input.BuildTextures(context);
            return input;
        }
    }


    /*相关配置文件，十分重要*/
    [ContentTypeWriter]
    class CWriterPMD : ContentTypeWriter<Importer_PmdModel>
    {
        protected override void Write(ContentWriter output, Importer_PmdModel value)
        {
            value.Write(output);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Importer_PmdModel).AssemblyQualifiedName;
        }
        //下面指示，运行时中该由哪一个函数去处理xnb文件的读取
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "PmdModelLib.PmdModelReader, PmdModelLib, Version=1.0, Culture=neutral";
        }
    }

    //////////////////////////////////everything below is for vmd//////////////////////////////////////////////////////////
    /// <summary>
    /// 读取vmd文件，并将读取之后的数据进行输出
    /// </summary>
    [ContentImporter(".vmd", DisplayName = "VmdImporter")]
    class VmdImporter : ContentImporter<Importer_VmdAnimation>
    {
        /// <summary>
        /// 读取vmd文件
        /// </summary>
        /// <param name="filename">Vmd 文件名</param>
        /// <param name="context">不用管</param>
        /// <returns></returns>
        public override Importer_VmdAnimation Import(string filename, ContentImporterContext context)
        {

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
            {
                Importer_VmdAnimation vmd = new Importer_VmdAnimation();
                //thia is what magic happened
                vmd.Read(reader);

                return vmd;
            }
        }
    }

    /// <summary>
    /// 这是我们的processor，其实并不做什么，只是将数据进行输出一下，走一个过程
    /// </summary>
    [ContentProcessor(DisplayName = "VmdProcessor")]
    class VmdProcessor : ContentProcessor<Importer_VmdAnimation, Importer_VmdAnimation>
    {
        //process the texture
        public override Importer_VmdAnimation Process(Importer_VmdAnimation input, ContentProcessorContext context)
        {
            return input;
        }
    }

    /*相关配置文件，十分重要*/
    [ContentTypeWriter]
    class CWriterVmd : ContentTypeWriter<Importer_VmdAnimation>
    {
        protected override void Write(ContentWriter output, Importer_VmdAnimation value)
        {
            value.Write(output);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Importer_VmdAnimation).AssemblyQualifiedName;
        }
        //下面指示，运行时中该由哪一个函数去处理xnb文件的读取
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "PmdModelLib.VmdAnimationReader, PmdModelLib, Version=1.0, Culture=neutral";
        }
    }
}
