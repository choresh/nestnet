using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Entities
{
    [Table("DisputeHistories")]
    public class DisputeHistoryEntity : EntityBase
    {
        // This property enables code at 'NestNet.Infra' to handle the entity in general 
        // manner (without knowing the specific name 'DisputeHistoryId').
        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore
        )]
        [NotMapped] // Exclude property from DB.
        public override long Id
        {
            get { return DisputeHistoryId; }
            set { DisputeHistoryId = value; }
        }

        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Mandatory
        )]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DisputeHistoryId { get; set; }

        // TODO: FK
        public long DisputeId { get; set; }

        [Prop(
           create: GenOpt.Optional,
           update: GenOpt.Optional,
           result: GenOpt.Optional
       )]

        public long CurrSum { get; set; }
    }
}