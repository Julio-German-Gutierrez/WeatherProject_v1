using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WeatherProject_v1.Models
{
    /*
     * This class represents one record in the Table created
     * from the information in the ".cvs" file.
     * It was use by EF (code first) to create the table.
     */
    public class Register
    {
        [Key]
        public int RegisterId { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime Date { get; set; }

        [Column(TypeName = "bit")]
        public bool IsExterior { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public double Temperature { get; set; }

        public int Humidity { get; set; }
    }
}
