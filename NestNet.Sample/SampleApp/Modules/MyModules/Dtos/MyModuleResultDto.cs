namespace SampleApp.Modules.MyModules.Dtos
{
    /// <summary>
    /// * This is an auto-generated DTO class.
    /// * Do not modify this file directly as it will be regenerated.
    /// * To modify the properties, please update properties/attributes at the corresponding entity class.
    /// </summary>
    public class MyModuleResultDto
    {
		public required Int64 MyModuleId { get; set; }
		public required String Name { get; set; }
		public required Int64 Age { get; set; }
		public String? Email { get; set; }
		public required DateTime CreatedAt { get; set; }
		public required DateTime UpdatedAt { get; set; }
    }
}