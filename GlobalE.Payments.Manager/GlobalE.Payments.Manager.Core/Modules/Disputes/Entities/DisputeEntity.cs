using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GlobalE.Payments.Manager.Core.Modules.Disputes.Entities
{
    [Table("Disputes")]
    public class DisputeEntity : EntityBase
    {
        // This property enables code at 'NestNet.Infra' to handle the entity in general 
        // manner (without knowing the specific name 'DisputeId').
        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore
        )]
        [NotMapped] // Exclude property from DB.
        public override long Id
        {
            get { return DisputeId; }
            set { DisputeId = value; }
        }

        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Mandatory
        )]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DisputeId { get; set; }

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Optional,
            result: GenOpt.Mandatory
        )]
        public required string Name { get; set; }

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Optional,
            result: GenOpt.Mandatory
        )]
        public long Age { get; set; }

        [Prop(
            create: GenOpt.Optional,
            update: GenOpt.Optional,
            result: GenOpt.Optional
        )]
        public string? Email { get; set; }

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore
        )]
        [NotMapped] // Exclude property from DB.
        public string? MyVirtualField { get; set; }
    }
}