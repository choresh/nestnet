#pragma warning disable IDE0290 // Use primary constructor
using GlobalE.Payments.Manager.Core.Modules.Disputes.Entities;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Entities;

namespace GlobalE.Payments.Manager.Core.Data
{
    public class DisputeWithHistory
    {
        public DisputeEntity? Dispute { get; set; }
        public DisputeHistoryEntity? History { get; set; }
    } 
}

#pragma warning restore IDE0290 // Use primary constructor