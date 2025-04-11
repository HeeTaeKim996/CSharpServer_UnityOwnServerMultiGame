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
    public enum Pr_ta_lobby_action : byte
    {
        enter_room = 0,

        create_room = 1,


        end
    }
    public enum Pr_ta_room_target : byte
    {
        all = 0,

        others = 1,

        masterCLient = 2,
        
        room = 3,

        end
    }

    public enum Pr_ta_room_action : byte
    {
        ts,

        game_start_masterClient,

        game_load_completed,

        exit_room,

        end
    }


    public enum Pr_client_action : byte
    {
        ts = 0,

        lobby_actin = 1,

        room_action = 2,

        game_late_start = 3
    }
    public enum Pr_ca_lobby__action
    {
        lobby_list_info = 0,
    }
    public enum Pr_ca_room_action
    {
        room_info,

        room_start,

        game_start,

        back_to_lobby
    }



}
