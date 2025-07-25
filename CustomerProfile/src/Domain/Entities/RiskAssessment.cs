using src.Domain.Enums;

namespace src.Domain.Entities
{
    public class RiskAssessment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public RiskAssessmentType RiskAssessmentType { get; set; } // Type of risk assessment (e.g., AML, PEP, etc.)
        public RiskAssessmentSummary RiskAssessmentSummary { get; set; } // Summary of the risk assessment
        public RiskLevel RiskLevel { get; set; } // Overall risk level (High, Moderate, Low)
        public DateTimeOffset AssessmentDate { get; set; } // Date of the assessment
        public string? Assessor { get; set; } // Person who performed the assessment
        public string? Notes { get; set; } // Additional notes or comments


        // Helper Properties
        public bool IsHighRisk => RiskLevel == RiskLevel.High;
        public bool IsModerateRisk => RiskLevel == RiskLevel.Medium;
        public bool IsLowRisk => RiskLevel == RiskLevel.Low;

    }
}
