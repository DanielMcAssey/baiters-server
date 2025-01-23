namespace GLOKON.Baiters.Core.Constants
{
    public sealed class PacketType
    {
        public const string Handshake = "handshake";
        public const string ServerClose = "server_close";
        public const string PeerWasKicked = "peer_was_kicked";
        public const string ClientWasKicked = "client_was_kicked";
        public const string PeerWasBanned = "peer_was_banned";
        public const string ClientWasBanned = "client_was_banned";
        public const string RequestActors = "request_actors";
        public const string ActorRequestSend = "actor_request_send";
        public const string InstanceActor = "instance_actor";
        public const string ActorUpdate = "actor_update";
        public const string ActorAnimationUpdate = "actor_animation_update";
        public const string ActorAction = "actor_action";
        public const string Message = "message";
        // NOTE: Typo in "received" is in WebFishing not here
        public const string LetterReceived = "letter_recieved";
        public const string LetterWasAccepted = "letter_was_accepted";
        public const string LetterWasDenied = "letter_was_denied";
        public const string ChalkPacket = "chalk_packet";
        public const string NewPlayerJoin = "new_player_join";
        public const string PlayerPunch = "player_punch";
        public const string UserJoinedWebLobby = "user_joined_weblobby";
        public const string UserLeftWebLobby = "user_left_weblobby";
        public const string ReceiveWebLobby = "receive_weblobby";
    }
}
