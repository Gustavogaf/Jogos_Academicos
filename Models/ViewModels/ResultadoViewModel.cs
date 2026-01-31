using System.ComponentModel.DataAnnotations;

namespace Jogos_Academicos.Models.ViewModels
{
    public class ResultadoViewModel
    {
        [Required]
        public int JogoId { get; set; }

        public int? PlacarA { get; set; }
        public int? PlacarB { get; set; }

        public bool WoA { get; set; } // Time A ganhou por W.O.
        public bool WoB { get; set; } // Time B ganhou por W.O.
    }
}
