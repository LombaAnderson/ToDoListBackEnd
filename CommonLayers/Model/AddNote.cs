using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLayer.Model
{
    public class AddNoteRequest
    {
        public int Id { get; set; }
        [Required]
        public string Note { get; set; }
        public string DataAgendada { get; set; }
        public string HoraAgendada { get; set; }
        public bool Segunda { get; set; }
        public bool Terca { get; set; }
        public bool Quarta { get; set; }
        public bool Quinta { get; set; }
        public bool Sexta { get; set; }
        public bool Sabado { get; set; }
        public bool Domingo { get; set; }
    }

    public class AddNoteResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
