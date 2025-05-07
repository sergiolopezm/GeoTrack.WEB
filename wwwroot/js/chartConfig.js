window.initializeCharts = {
    createChart: function (canvasId, tipo, etiquetas, datos, titulo, color) {
        var ctx = document.getElementById(canvasId);
        if (ctx) {
            var colores = Array(datos.length).fill(color);

            new Chart(ctx, {
                type: tipo,
                data: {
                    labels: etiquetas,
                    datasets: [{
                        label: titulo,
                        data: datos,
                        backgroundColor: colores,
                        borderColor: colores,
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                precision: 0
                            }
                        }
                    }
                }
            });
        } else {
            console.error("Canvas element with id " + canvasId + " not found");
        }
    }
};