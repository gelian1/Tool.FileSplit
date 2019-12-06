using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tool.FileSplit.Standard
{
    /// <summary>
    /// 切割大文件
    /// </summary>
    public class Sectcut
    {
        #region GetInstance
        private static Lazy<Sectcut> _lazy = new Lazy<Sectcut>(() => { return new Sectcut(); });
        public static Sectcut GetInstance
        {
            get { return _lazy.Value; }
        }
        #endregion

        /// <summary>
        /// 源路径
        /// </summary>
        private static string SourcePath { get; set; }

        /// <summary>
        /// 目标路径
        /// </summary>
        private static string TargetPath { get; set; }

        /// <summary>
        /// 默认8MB
        /// </summary>
        private readonly static int SplitFileSize = 1024 * 1024 * 8;

        /// <summary>
        /// 分割文件计数
        /// </summary>
        private static int SplicFileNum = 1;

        private Stopwatch sw = new Stopwatch();

        private Sectcut()
        {
            SourcePath = AppDomain.CurrentDomain.BaseDirectory;
            TargetPath = SourcePath;
        }

        /// <summary>
        /// 日志文件切割
        /// </summary>
        public void FileSplitStart()
        {
            Console.WriteLine("欢迎使用此程序！");
            Console.WriteLine("此程序支持切割GB级的大文本文件，使用流程说明如下：");
            Console.WriteLine("1. 将此程序复制到要切割的文件平级目录");
            Console.WriteLine("2. 运行程序，并按照要求输入要切割的大文本文件的全名（例：1.log）");
            Console.WriteLine("3. 程序会自动在当前建立指定文件名的文件夹（例：1），文件夹中存放有切割好的文本文件");
            Console.WriteLine("----------------------------------------------------------------------");

            Console.WriteLine("请输入要切割的文本文件全名（包括后缀），输入完成按回车继续");
            string fileName = Console.ReadLine();

            sw.Start();

            Console.WriteLine("开始切割文件");
            var checkResult = Check(fileName);
            if (!checkResult) { return; }

            FileLoad(fileName);

            sw.Stop();
            Console.WriteLine("总耗时“{0}”秒", sw.Elapsed.Seconds);
        }

        #region 操作前检查
        /// <summary>
        /// 操作前检查
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool Check(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine("请输入文件名");
                return false;
            }

            #region 源目录
            SourcePath = string.Format("{0}{1}", SourcePath, fileName);
            if (!File.Exists(SourcePath))
            {
                Console.WriteLine("抱歉，“{0}”中没有该文件", SourcePath);
                return false;
            }
            #endregion

            #region 目标目录
            string directoryName = string.Empty;
            if (fileName.IndexOf('.') == -1)
            {
                directoryName = fileName;
            }
            else
            {
                directoryName = fileName.Substring(0, fileName.IndexOf('.'));
            }
            TargetPath = string.Format("{0}{1}/", TargetPath, directoryName);
            if (Directory.Exists(TargetPath))
            {
                Console.WriteLine("抱歉，中已经存在文件夹“{0}”，无法重新创建", TargetPath);
                return false;
            }
            else
            {
                Directory.CreateDirectory(TargetPath);
            }
            #endregion

            return true;
        }
        #endregion

        #region 加载文件
        /// <summary>
        /// 加载指定文件
        /// </summary>
        /// <param name="fileName">指定文件名</param>
        private void FileLoad(string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(SourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bool splitResult = true;
                    BinaryReader br = new BinaryReader(fs, System.Text.Encoding.UTF8);
                    while (splitResult)
                    {
                        fs.Position = (long)SplitFileSize * ((long)SplicFileNum - 1);
                        
						Console.WriteLine("“{0}”文件加载成功，开始分割", SourcePath);
						splitResult = Split(br, fileName);
                    }
                    br.Close();
                    br.Dispose();
                }

                Console.WriteLine("恭喜，分割流程完成！");
                Console.WriteLine("快去“{0}”中查看吧", TargetPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 分割文件
        /// </summary>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private bool Split(BinaryReader br, string fileName)
        {
            Console.WriteLine("--------------------------------------------");
            var contentBytes = br.ReadBytes(SplitFileSize);

            #region 将读取到的内容写入新文件中
            var newFilePath = CreateNewFilePath(fileName);
            Console.WriteLine("文件内容读取完成，开始写入新文件中，本次要写入的新文件：“{0}”", newFilePath);
            using (FileStream writeFs = new FileStream(newFilePath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(writeFs, Encoding.UTF8))
                {
                    bw.Write(contentBytes);
                }
            }
            Console.WriteLine("新文件写入完成");
            #endregion

            if (contentBytes.Length >= SplicFileNum)
            {
                Console.WriteLine("准备进行下次切割");
                return true;
            }
            return false;
        }
        #endregion

        #region 生成新的文件完整路径
        /// <summary>
        /// 生成新的文件完整路径
        /// </summary>
        /// <param name="fileName">要切割的文件名</param>
        private string CreateNewFilePath(string fileName)
        {
            var newFileName = fileName;
            string suffix = string.Empty;
            if (fileName.IndexOf('.') >= 0)
            {
                suffix = fileName.Substring(fileName.IndexOf('.'));
                newFileName = fileName.Substring(0, fileName.IndexOf('.'));
            }
            newFileName = string.Format("{0}{1}_{2}{3}", TargetPath, newFileName, SplicFileNum, suffix);

            SplicFileNum++;

            return newFileName;
        }
        #endregion
    }
}
