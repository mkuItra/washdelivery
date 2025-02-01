function showDrawer(id) {
    const drawer = document.getElementById(id);
    drawer.classList.remove('translate-y-full');
    document.body.style.overflow = 'hidden';
}

function hideDrawer(id) {
    const drawer = document.getElementById(id);
    drawer.classList.add('translate-y-full');
    document.body.style.overflow = '';
}

// Dodaj obsługę gestów
document.querySelectorAll('.drawer').forEach(drawer => {
    let touchStart = null;

    drawer.addEventListener('touchstart', (e) => {
        touchStart = e.touches[0].clientY;
    });

    drawer.addEventListener('touchmove', (e) => {
        if (!touchStart) return;
        const touchEnd = e.touches[0].clientY;
        const diff = touchEnd - touchStart;
        if (diff > 50) {
            hideDrawer(drawer.id);
        }
    });

    drawer.addEventListener('touchend', () => {
        touchStart = null;
    });
}); 