namespace GLOKON.Baiters.Server.Requests
{
    public struct SendLetterRequest
    {
        public SendLetterRequest()
        {
        }

        public required string Header { get; set; }

        public required string Body { get; set; }

        public required string Closing { get; set; }

        public IList<string>? Items { get; set; } = null;
    }
}
