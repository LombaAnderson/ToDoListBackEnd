using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLayer.Model
{
    public class GetNoteRequest
    {
        [Required]
        public int PageNumber { get; set; }

        [Required]
        public int NumberOfRecordPerPage { get; set; }

        [Required]
        public string SortBy { get; set; } // ASC, DESC
    }

    public class GetNoteResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int CurrentPage { get; set; }
        public decimal TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public List<GetNote> data { get; set; }

    }

    public class GetNote
    {
        public int NoteId { get; set; }
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
}
