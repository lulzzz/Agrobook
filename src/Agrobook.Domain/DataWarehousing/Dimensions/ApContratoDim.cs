﻿using System.Data.Entity.ModelConfiguration;

namespace Agrobook.Domain.DataWarehousing.Dimensions
{
    public class ApContratoDim
    {
        public int Sid { get; set; }
        public string IdContrato { get; set; }
        public string NombreContrato { get; set; }
        public bool EsAdenda { get; set; }
        public string IdContratoDeLaAdenda { get; set; }
    }

    public class ContratoDimMap : EntityTypeConfiguration<ApContratoDim>
    {
        public ContratoDimMap()
        {
            this.HasKey(e => e.Sid);


            this.ToTable("ContratoDims");
        }
    }
}
