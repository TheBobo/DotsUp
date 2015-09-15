using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRChatDatabase.Models
{
    public class GameTables
    {
        [Key]
        public int Id { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public string GameId { get; set; }
        public string UserInTurn { get; set; }

        public string WinnerId { get; set; }
        public string TurnamentId { get; set; }


        public decimal Amount { get; set; }
        public bool Paid { get; set; }

        public int PlayerOneBoxes { get; set; }
        public int PlayerTwoBoxes { get; set; }
        [StringLength(20)]
        public string PlayerOneId { get; set; }
        [StringLength(20)]
        public string PlayerTwoId { get; set; }
        public GameTables()
        {

        }
    }
}
