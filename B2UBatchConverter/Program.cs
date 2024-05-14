using System.Text;

namespace B2UBatchConverter
{
    class Program
    {
        public class AnalyzeResult
        {
            public string Content;
            public Encoding Encoding;
        }

        //REF：http://goo.gl/jAJgIr by Rick Strahl
        public static AnalyzeResult AnalyzeFile(string srcFile)
        {
            //預設為Big5
            Encoding enc = Encoding.GetEncoding(950);

            //由前五碼識別出UTF8、Unicode、UTF32等編碼，其餘則視為
            byte[] buffer = new byte[5];
            using (FileStream file = new FileStream(srcFile, FileMode.Open))
            {
                file.Read(buffer, 0, 5);
                file.Close();

                if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                    enc = Encoding.UTF8;
                else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                    enc = Encoding.Unicode;
                else if (buffer[0] == 0 && buffer[1] == 0 &&
                         buffer[2] == 0xfe && buffer[3] == 0xff)
                    enc = Encoding.UTF32;
                else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                    enc = Encoding.UTF7;
            }
            //使用指定的Encoding讀取內容
            return new AnalyzeResult()
            {
                Content = File.ReadAllText(srcFile, enc),
                Encoding = enc
            };
        }

        static void Main(string[] args)
        {
            //args = new string[] { "D:\\Lab\\L805\\ConApp" };
            string path = args[0];
            //列舉要搜尋轉碼的副檔名
            var scanFileTypes = "vb,cs,js".Split(',');
            //略過不處理的資料夾名稱
            var skipFolders = "bin,obj".Split(',');
            foreach (var file in
                //列舉所有子目錄下的檔案
                Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                //取得副檔名
                var ext = Path.GetExtension(file).TrimStart('.').ToLower();
                //若非預先指定的副檔名就略過不處理
                if (!scanFileTypes.Contains(ext)) continue;
                //處於\bin\* \obj\*目錄下的檔案也一律略過
                if (skipFolders.Any(o => file.Contains(
                    Path.DirectorySeparatorChar + o + Path.DirectorySeparatorChar)))
                    continue;

                //讀取檔案內容並識別編碼
                var analysis = AnalyzeFile(file);
                if (analysis.Encoding.CodePage == 950) //BIG5編號檔案才要處理
                {
                    Console.Write("Process File {0}...", file);
                    //將原檔更名為*.big5.bak
                    var bakFile = file + ".big5.bak";
                    if (File.Exists(bakFile)) File.Delete(bakFile);
                    File.Move(file, bakFile);
                    //重新以UTF8寫入
                    File.WriteAllText(file, analysis.Content, Encoding.UTF8);
                    Console.WriteLine(" done!");
                }
                else
                {
                    Console.WriteLine("Skip File {0} / {1}", file,
                        analysis.Encoding.EncodingName);
                }
            }
        }
    }
}
