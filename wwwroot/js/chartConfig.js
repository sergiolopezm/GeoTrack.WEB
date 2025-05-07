// Objeto para manejar la inicialización de gráficos
window.initializeCharts = {
    createChart: function (canvasId, type, labels, data, labelText, color) {
        // Verificar si el canvas existe
        var canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas no encontrado:', canvasId);
            return;
        }

        // Verificar si Chart está definido
        if (typeof Chart === 'undefined') {
            console.error('Chart.js no está cargado');
            return;
        }

        // Crear la configuración del gráfico
        var config = {
            type: type,
            data: {
                labels: labels,
                datasets: [{
                    label: labelText,
                    data: data,
                    backgroundColor: color,
                    borderColor: color,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        };

        // Crear y devolver el gráfico
        return new Chart(canvas, config);
    }
};