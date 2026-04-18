// App.js - Blazor Server App
window.blazorHelpers = {
    showModal: function(id) {
        var modal = document.getElementById(id);
        if (modal) modal.style.display = 'block';
    },
    hideModal: function(id) {
        var modal = document.getElementById(id);
        if (modal) modal.style.display = 'none';
    }
};
