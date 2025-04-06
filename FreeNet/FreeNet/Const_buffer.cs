
namespace FreeNet
{
    public struct Const_buffer 
    {
        public byte[] Value { get; private set; }
        public int position { get; private set; }

        public Const_buffer(byte[] value, int position)
        {
            this.Value = value;
            this.position = position;
        }
    }
}
