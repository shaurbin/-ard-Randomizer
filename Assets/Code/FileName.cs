using System;
using System.Text;

namespace Assets.Code
{
    public class FileName
    {
        public string[] Directories;
        public string Path;
        public string Name;
        public string NameWithoutExt;
        public string Postfix;
        public string NameOfPostfix;
        public int Number;

        public FileName(string path)
        {
            var directories = path.Split("/");
            Directories = directories[0..^1];
            Name = directories[directories.Length - 1];
            NameWithoutExt = Name.Split(".")[0];

            bool readNumber = false;
            StringBuilder numberBuilder = new StringBuilder();
            foreach (var symbol in NameWithoutExt)
            {
                if (Char.IsDigit(symbol) && !readNumber)
                    readNumber = true;
                
                if (readNumber)
                    numberBuilder.Append(symbol);
            }

            if (readNumber)
                Number = Int32.Parse(numberBuilder.ToString());

            if (path.Contains("~"))
            {
                string[] a = Directories[^1].Split("~");
                Postfix = a[1];
                NameOfPostfix = a[0];
            }

            StringBuilder pathBuilder = new StringBuilder();
            foreach (var x in Directories)
            {
                pathBuilder.Append(x);
                pathBuilder.Append('/');
            }
            pathBuilder.Remove(pathBuilder.Length - 1, 1);
            Path = pathBuilder.ToString();
        }
    }
}
