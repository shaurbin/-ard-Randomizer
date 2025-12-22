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

        public FileName(string path)
        {
            var directories = path.Split("/");
            Directories = directories[0..^1];
            Name = directories[directories.Length - 1];
            NameWithoutExt = Name.Split(".")[0];
            
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
