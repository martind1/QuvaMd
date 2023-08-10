
using Quva.Model.Enums;

namespace Quva.Model.Dtos.SignalR
{
    public class SignalRMessage
    {
        public Guid Guid { get; set; }
        public string Message { get; set; }
        public SignalRMessageType MessageType { get; set; }
        public int Count { get; set; }
    }
}
