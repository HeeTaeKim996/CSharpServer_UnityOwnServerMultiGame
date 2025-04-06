

namespace FreeNet
{
    public interface IPeer
    {
        void On_message(Const_buffer buffer);
        void On_removed();
        void Send(CPacket msg);
        void Disconnect();
        void Process_user_operation(CPacket msg);

    }
}
