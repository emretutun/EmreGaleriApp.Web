document.addEventListener('DOMContentLoaded', function () {
    // ConfirmOrder formunu seç
    const form = document.querySelector('form[asp-action="ConfirmOrder"]');
    if (!form) return; // Form yoksa çık

    form.addEventListener('submit', async function (e) {
        e.preventDefault(); // Formun normal gönderimini engelle

        // Sepetteki araç ID'lerini topla
        const carIds = [];
        document.querySelectorAll('.car-id').forEach(input => {
            const id = parseInt(input.value);
            if (!isNaN(id)) {
                carIds.push(id);
            }
        });

        if (carIds.length === 0) {
            alert("Sepetinizde araç yok.");
            return;
        }

        // Her araç için müsaitlik kontrolü yap
        for (const carId of carIds) {
            let available = false;

            try {
                available = await window.connection.invoke("CheckIfCarAvailable", carId);
            } catch (error) {
                console.error("SignalR bağlantı hatası:", error.toString());
                alert("Sunucuya bağlanırken hata oluştu. Lütfen sayfayı yenileyin.");
                return;
            }

            if (!available) {
                alert(`Üzgünüz, ${carId} ID'li araç şu anda başkası tarafından kiralanıyor.`);
                return;
            }
        }



        // Tüm araçlar müsaitse formu gönder
        form.submit();
    });
});
