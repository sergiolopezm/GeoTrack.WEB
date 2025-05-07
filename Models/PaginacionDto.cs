namespace GeoTrack.WEB.Models
{
    /// <summary>
    /// Modelo para paginación de resultados
    /// </summary>
    public class PaginacionDto<T> where T : class
    {
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public int ElementosPorPagina { get; set; }
        public List<T>? Lista { get; set; }

        public bool TienePaginaAnterior => Pagina > 1;
        public bool TienePaginaSiguiente => Pagina < TotalPaginas;

        public List<int> Paginas
        {
            get
            {
                var paginas = new List<int>();
                var inicio = Math.Max(1, Pagina - 2);
                var fin = Math.Min(TotalPaginas, Pagina + 2);

                for (int i = inicio; i <= fin; i++)
                {
                    paginas.Add(i);
                }

                return paginas;
            }
        }
    }
}
