
using Quva.Model.Enums;

namespace Quva.Model.Dtos.SignalR
{
    public class SignalRMessage
    {
        public Guid Guid { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Message { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SignalRMessageType MessageType { get; set; }
        public int Count { get; set; }
    }
}
