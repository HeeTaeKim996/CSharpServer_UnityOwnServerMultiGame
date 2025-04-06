using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet
{
    public enum Pr_target : byte
    {
        lobby = 0,

        room = 1,



        end
    }
    public enum Pr_lobbyAction : byte
    {
        enter_room = 0,

        create_room = 1,


        end
    }
    public enum Pr_room_target : byte
    {
        all = 0,

        others = 1,

        masterCLient = 2,


        end
    }

    public enum Pr_room_action : byte
    {
        ts = 0,

        end
    }

    public enum Pr_client_action : byte 
    {
        ts = 0
    }



}
