namespace KolayCAR.Broker.API.Models
{
    public class VehicleFilterMobile
    {
        public int Id { get; set; }

        /// <summary>
        /// Filtre Grup adı (Filters, FastFilters, OrderOptions)
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Child filtrelerdir (Filters => Fuel Filters, Transmission Filters)
        /// </summary>
        /// <example>
        /// Filters  => Parent
        ///     ↳ Fuel Filter => Parent
        ///         ↳ Gas
        ///         ↳ Diesel vb
        ///     ↳ Transmission Filter => Parent
        ///         ↳ Auto
        ///         ↳ Manuel
        ///         ↳ Semi Auto vb.
        ///     ↳ Vehicle Filter vb. => Parent
        /// Order Options => Parent
        ///     ↳ Increase
        ///     ↳ Decrease
        ///     ↳ Default vb.
        /// </example>
        public string ParentKey { get; set; }

        /// <summary>
        /// Filtrenin geleceği sıra
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// Filtre başlığının istek atılan dildeki karşılığı
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Filtrenin icon path'i
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Filtrenin unique değeri
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Filtre tipi (Fuel, Transmission vb.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Filtrenin istek atılan dildeki karşılığı (Diesel, Dizel vb.)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Order options
        /// </summary>
        public bool? IsDefault { get; set; }
    }
}
