using GLOKON.Baiters.Core.Constants;

namespace GLOKON.Baiters.Server.Requests
{
    public class SendMessageRequest
    {
        public required string Message { get; set; }

        public string Colour { get; set; } = MessageColour.Default;
    }
}
