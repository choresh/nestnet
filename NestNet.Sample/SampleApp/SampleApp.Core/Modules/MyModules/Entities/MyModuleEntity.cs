using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Core.Modules.MyModules.Entities
{
    [Table("MyModules")]
    public class MyModuleEntity : EntityBase
    {
        // This property enables code at 'NestNet.Infra' to handle the entity in general 
        // manner (without knowing the specific name 'MyModuleId').
        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore
        )]
        [NotMapped] // Exclude property from DB.
        public override long Id
        {
            get { return MyModuleId; }
            set { MyModuleId = value; }
        }

        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Mandatory
        )]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MyModuleId { get; set; }

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