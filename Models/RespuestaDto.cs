namespace GeoTrack.WEB.Models
{
    /// <summary>
    /// Modelo para las respuestas estándar de la API
    /// </summary>
    public class RespuestaDto
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? Detalle { get; set; }
        public object? Resultado { get; set; }

        public static RespuestaDto ErrorPorDefecto()
        {
            return new RespuestaDto
            {
                Exito = false,
                Mensaje = "Error de conexión",
                Detalle = "No se pudo conectar con el servidor. Por favor, intenta nuevamente más tarde."
            };
        }

        public static RespuestaDto Desde(Exception ex)
        {
            return new RespuestaDto
            {
                Exito = false,
                Mensaje = "Error de sistema",
                Detalle = ex.Message
            };
        }
    }

    /// <summary>
    /// Versión genérica para respuestas con tipo específico
    /// </summary>
    public class RespuestaDto<T> where T : class
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? Detalle { get; set; }
        public T? Resultado { get; set; }

        public static RespuestaDto<T> Desde(RespuestaDto respuesta)
        {
            return new RespuestaDto<T>
            {
                Exito = respuesta.Exito,
                Mensaje = respuesta.Mensaje,
                Detalle = respuesta.Detalle,
                Resultado = respuesta.Resultado as T
            };
        }
    }
}
