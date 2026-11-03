
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventorySystem.Model
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a supplier.")]
        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999999)]
        public decimal TotalAmount { get; set; } = 0;
        public ICollection<PurchaseOrderItem> Items { get; set; }
    = new List<PurchaseOrderItem>();
    }
}