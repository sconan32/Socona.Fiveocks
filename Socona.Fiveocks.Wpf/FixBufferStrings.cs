using System.ComponentModel;

namespace Socona.Fiveocks
{
    public class FixBufferStrings
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private readonly char[,] buffer;

        public FixBufferStrings(int height, int width=80)
        {
            this.Width = width;
            this.Height = height;
            buffer=new char[height,width];
        }

        public void  AddLine(string str)
        {
            
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}