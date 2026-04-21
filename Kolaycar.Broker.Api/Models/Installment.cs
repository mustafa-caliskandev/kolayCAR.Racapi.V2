using System.ComponentModel.DataAnnotations.Schema;

namespace KolayCAR.Broker.API.Models
{
    public class Installment
    {
        public int Id { get; set; }
        public int PaymentSettingId { get; set; }
        public int InstallmentCount { get; set; }

        public float InstallmentTotalAmount { get; set; }
        public float InstallmentAmount { get; set; }
        public float InstallmentPercent { get; set; }

        public string Comment { get; set; }

        public bool IsActive { get; set; }

        public virtual PaymentSetting PaymentSetting { get; set; }

        [NotMapped]
        public float InstallmentCommisionAmout { get; set; }
    }
}
