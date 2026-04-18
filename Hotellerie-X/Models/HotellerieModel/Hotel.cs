using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotellerie_X.Models.HotellerieModel
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Le nom doit avoir entre 3 et 20 caractères.")]
        public string Nom { get; set; } = null!;

        [Range(1, 5, ErrorMessage = "Les étoiles doivent être entre 1 et 5.")]
        public int Etoiles { get; set; }

        [Required(ErrorMessage = "La ville est obligatoire.")]
        public string Ville { get; set; } = null!;

        [Required(ErrorMessage = "Le site web est obligatoire.")]
        [Url(ErrorMessage = "L'URL n'est pas valide.")]
        [Display(Name = "Site Web")]
        public string SiteWeb { get; set; } = null!;

        public string? Tel { get; set; }

        public string? Pays { get; set; }

        public virtual ICollection<Appreciation>? Appreciations { get; set; }
    }
}
