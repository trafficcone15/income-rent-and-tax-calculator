using SQLite;

namespace IncomeRelatedRent.Models
{
    [Table("Weekly_Deductions")]
    public class TaxDeductionsData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Earnings { get; set; }
        public double MTaxCodePAYE { get; set; }
        public double METaxCodePAYE { get; set; }
        public double StudentLoanDeductions { get; set; }
        public double KiwiSaverDeductionsAtThreePercent { get; set; }
        public double KiwiSaverDeductionsAtFourPercent { get; set; }
        public double KiwiSaverDeductionsAtSixPercent { get; set; }
        public double KiwiSaverDeductionsAtEightPercent { get; set; }
        public double KiwiSaverDeductionsAtTenPercent { get; set; }
        public double EmployersContributionTaxAtTenPointFivePercent { get; set; }
        public double GrossEmployerContributionAtTenPointFivePercent { get; set; }
        public double EmployersContributionTaxAtSeventeenPointFivePercent { get; set; }
        public double GrossEmployerContributionAtSeventeenPointFivePercent { get; set; }
        public double EmployersContributionTaxAtThirteenPercent { get; set; }
        public double GrossEmployerContributionAtThirteenPercent { get; set; }
        public double EmployersContributionTaxAtThirtyThreePercent { get; set; }
        public double GrossEmployerContributionAtThirtyThreePercent { get; set; }
        public double EmployersContributionTaxAtThirtyNinePercent { get; set; }
        public double GrossEmployerContributionAtThirtyNinePercent { get; set; }
    }
}