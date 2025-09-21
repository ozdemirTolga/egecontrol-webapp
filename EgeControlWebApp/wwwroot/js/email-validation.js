function setEmailTo(type) {
    const emailValue = document.getElementById('emailAddress').value.trim();
    
    // E-posta adresi kontrolü
    if (!emailValue) {
        alert('E-posta adresi boş olamaz. Lütfen geçerli bir e-posta adresi girin.');
        return false;
    }
    
    // Basit e-posta formatı kontrolü
    const atSymbol = String.fromCharCode(64); // @ işareti için
    if (emailValue.indexOf(atSymbol) === -1 || emailValue.indexOf('.') === -1 || emailValue.length < 5) {
        alert('Geçersiz e-posta formatı. Lütfen doğru e-posta adresi girin.');
        return false;
    }
    
    document.getElementById(type + 'EmailTo').value = emailValue;
    return true;
}

// Form submit'i yakalayıp e-posta kontrolü yap
document.addEventListener('DOMContentLoaded', function() {
    const forms = document.querySelectorAll('.dropdown-item-form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const emailValue = document.getElementById('emailAddress').value.trim();
            const atSymbol = String.fromCharCode(64); // @ işareti için
            if (!emailValue || emailValue.indexOf(atSymbol) === -1) {
                e.preventDefault();
                alert('E-posta gönderebilmek için geçerli bir e-posta adresi giriniz.');
                return false;
            }
            // Hidden 'to' input değerini ayarla
            const toInput = form.querySelector('input[name="to"]');
            if (toInput) {
                toInput.value = emailValue;
            }
        });
    });
});
